using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics.Contracts;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// A threadsafe way to allocate and reuse JS processors.
    /// Note: There is no upper limit in creating new processors but if you use this component with .Net's Threadpool, 
    /// you should get at max processors as you have logical CPUs in your box.
    /// </summary>
    public class JsEnginePool : IDisposable
    {
        private readonly ConcurrentBag<JsEngine> pool = new ConcurrentBag<JsEngine>();
        private bool closing = false;
        private static int n = 0;

        /// <summary>
        /// The list of scripts that were loaded to all <see cref="JsEngine"/>s of this pool.
        /// </summary>
        public readonly IListScripts ScriptRepository;

        // count the Engines in the wild plus 1 for the closing process.
        // see https://social.msdn.microsoft.com/Forums/vstudio/en-US/aa49f92c-01a8-4901-9846-91bc1587f3ae/countdownevent-initialcount-of-zero?forum=parallelextensions 
        private readonly CountdownEvent counter = new CountdownEvent(1);

        /// <summary>
        /// A name for the pool for debugging
        /// </summary>
        public string Name = "(unnamed)";

        /// <summary>
        /// Create a new pool and load the repository's scripts to each single <see cref="JsEngine"/>.
        /// The repository is frozen (see <see cref="ScriptRepository"/>) so that all engines have the same scripts loaded.
        /// </summary>
        /// <param name="repository"></param>
        public JsEnginePool(IListScripts repository)
        {
            Contract.Assert(repository != null, nameof(repository) + " is null.");
            ScriptRepository = repository;
            ScriptRepository.Freeze();
        }

        private void Close()
        {
            if (closing) throw new ApplicationException(string.Format("{0}.{1} must not be called twice.", nameof(JsEnginePool), nameof(Close)));
            closing = true;
            counter.Signal(); // here is the one, new CountdownEvent(1)
            counter.Wait();
            JsEngine e;
            while (pool.TryTake(out e))
                e.Close();
        }

        internal JsEngine ReturnToPool(JsEngine engine)
        {
            pool.Add(engine);
            counter.Signal();
            return engine;
        }

#pragma warning disable S2372 // Exceptions should not be thrown from property getters
        /// <summary>
        /// Allocate an engine from the pool. More engines are created on demand. 
        /// Dispose of the engine after use, it is then placed in the pool for future use.
        /// A typical usage pattern (C#) would be "using (var engine = pool.Engine) { ... engine.Execute(something); ... }".
        /// </summary>
        public JsEngine Engine
        {
            get
            {
                if (closing) throw new ApplicationException("can't acquire a new engine from a closed pool.");
                counter.AddCount();
                JsEngine result;
                if (!pool.TryTake(out result))
                    result = NewInitializedEngine();
                result.StartNewStopwatch();
                return result;
            }
        }
#pragma warning restore S2372 // Exceptions should not be thrown from property getters

        private JsEngine NewInitializedEngine()
        {
            int nn = Interlocked.Increment(ref n);
            var e = new JsEngine(this, nn);
            foreach (var s in ScriptRepository.Scripts)
            {
                e.Execute(s.Code, s.Name);
            }
            return e;
        }

        void IDisposable.Dispose()
        {
            if (closing) return;
            Close();
        }
    }
}
