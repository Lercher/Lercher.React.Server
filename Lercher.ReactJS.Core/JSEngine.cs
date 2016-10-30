using System;
using System.Linq;
using System.Diagnostics.Contracts;
using Microsoft.ClearScript.V8;
using System.Threading;

namespace Lercher.ReactJS.Core
{
    public class JsEngine : IDisposable
    {
        private JsEnginePool Pool { get; }
        private readonly V8ScriptEngine engine = new V8ScriptEngine();
        public int SerialNumber { get; }

        public JsEngine(JsEnginePool pool, int nr)
        {
            Contract.Assert(pool != null, nameof(pool) + " is null.");
            Pool = pool;
            SerialNumber = nr;
        }

        internal void Execute(string script, string name)
        {
            engine.Execute(name, script);
        }

        public void Calc2(int i)
        {
            // var o = engine.Evaluate("ReactDOM.render.toString() //1+1+sm+square(4)");
            var method = i % 2 == 0 ? "renderToStaticMarkup" : "renderToString";
            var o = engine.Evaluate("ReactDOMServer." + method + "(React.createElement(HelloWorld, { name: \"Mike Meyers\" }))");
            Console.WriteLine("Engine #{0,-3} Thread {3,-3} Request #{2,-3} says: {1}.", SerialNumber, o, i, Thread.CurrentThread.ManagedThreadId);
        }

        internal void Close()
        {
            engine.Dispose();
            Console.WriteLine("Engine #{0,-3} Thread {3,-3} disposed.", SerialNumber, 0, 0, Thread.CurrentThread.ManagedThreadId);
        }

        void IDisposable.Dispose()
        {
            Pool.ReturnToPool(this);
        }
    }
}
