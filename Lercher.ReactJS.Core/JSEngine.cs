using System;
using System.Linq;
using System.Diagnostics.Contracts;
using Microsoft.ClearScript.V8;
using System.Threading;
using System.Collections.Generic;

namespace Lercher.ReactJS.Core
{
    public class JsEngine : IDisposable
    {
        private JsEnginePool Pool { get; }
        private readonly V8ScriptEngine engine = new V8ScriptEngine();
        public int SerialNumber { get; }
        private readonly List<string> services = new List<string>();

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
        public void AddService(string globalname, object service)
        {
            engine.AddHostObject(globalname, service);
            services.Add(globalname);
        }

        /// <summary>
        /// evaluate an expression OR call a function with parameters
        /// </summary>
        /// <param name="expression">A standard JS expressen, if parameters are missing, a function pointer, if parameters are present.</param>
        /// <param name="parameters">parameters are passed as an object array with the global name '__', then the expression 
        /// is extended with '.apply(null, __)' and evaluated. '__' is deleted afterwards. Note that this will be null in the function call.</param>
        /// <returns>the evaluated value</returns>
        public dynamic Evaluate(string expression, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                engine.AddHostObject("__", parameters);
                var ret = engine.Evaluate(expression + ".apply(null, convertToJsArray(__))");
                engine.Execute("delete __;");
                return ret;
            }
            return engine.Evaluate(expression);
        }

        public void Calc2(int i)
        {
            // var o = engine.Evaluate("ReactDOM.render.toString() //1+1+sm+square(4)");
            var method = i % 2 == 0 ? "renderToStaticMarkup" : "renderToString";
            //var o = engine.Evaluate("ReactDOMServer." + method + "(React.createElement(HelloWorld, { name: \"Mike Meyers\" }))");
            var o = engine.Evaluate("(function () {var model = { name: \"Mike Meyers\" }; return ReactDOMServer." + method + "(React.createElement(HelloWorld, model));})()");
            Console.WriteLine("Engine #{0,-3} Thread {3,-3} Request #{2,-3} says: {1}.", SerialNumber, o, i, Thread.CurrentThread.ManagedThreadId);

        }

        internal void Close()
        {
            engine.Dispose();
            Console.WriteLine("Engine #{0,-3} Thread {3,-3} disposed.", SerialNumber, 0, 0, Thread.CurrentThread.ManagedThreadId);
        }

        void IDisposable.Dispose()
        {
            foreach (var s in services)
                engine.Execute(string.Format("delete {0};", s));
            services.Clear();
            Pool.ReturnToPool(this);
        }
    }
}
