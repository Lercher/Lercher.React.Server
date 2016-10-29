using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics.Contracts;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    public class JsEnginePool : IDisposable
    {
        private readonly ConcurrentBag<JsEngine> pool = new ConcurrentBag<JsEngine>();
        private bool closing = false;
        private static int n = 0;
        public readonly IManageScripts ScriptManager;

        // count the Engines in the wild plus 1 for the closing process.
        // see https://social.msdn.microsoft.com/Forums/vstudio/en-US/aa49f92c-01a8-4901-9846-91bc1587f3ae/countdownevent-initialcount-of-zero?forum=parallelextensions 
        private readonly CountdownEvent counter = new CountdownEvent(1);

        public JsEnginePool(IManageScripts scriptmanager)
        {
            Contract.Assert(scriptmanager != null, nameof(scriptmanager) + " is null.");
            ScriptManager = scriptmanager;
            ScriptManager.Close();
        }

        public void Close()
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
        public JsEngine Engine
        {
            get
            {
                if (closing) throw new ApplicationException("can't acquire a new engine from a closed pool.");
                counter.AddCount();
                JsEngine result;
                if (pool.TryTake(out result))
                    return result;
                return NewInitializedEngine();
            }
        }
#pragma warning restore S2372 // Exceptions should not be thrown from property getters

        private JsEngine NewInitializedEngine()
        {
            int nn = Interlocked.Increment(ref n);
            var e = new JsEngine(this, nn);
            foreach (var s in ScriptManager.Scripts)
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
