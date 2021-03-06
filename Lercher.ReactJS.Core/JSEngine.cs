﻿using System;
using System.Linq;
using System.Diagnostics.Contracts;
using Microsoft.ClearScript.V8;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// A facade on top of a ClearScript/V8 JS processor
    /// </summary>
    public class JsEngine : IDisposable
    {
        private JsEnginePool Pool { get; }
        private readonly V8ScriptEngine engine = new V8ScriptEngine();
        private readonly Stopwatch sw = new Stopwatch();

        /// <summary>
        /// An identifying number, that is unique among the <see cref="JsEnginePool"/>, this engine is allocated from.
        /// </summary>
        public readonly int SerialNumber;

        private readonly List<string> services = new List<string>();

        internal JsEngine(JsEnginePool pool, int nr)
        {
            Contract.Assert(pool != null, nameof(pool) + " is null.");
            Pool = pool;
            SerialNumber = nr;
            SetContext(null);
        }

        internal void StartNewStopwatch()
        {
            sw.Restart();
        }

        /// <summary>
        /// The name of the pool, this engine is allocated from.
        /// </summary>
        public string PoolName { get { return Pool.Name; } }

        internal void Execute(string script, string name)
        {
            engine.Execute(name, script);
        }

        /// <summary>
        /// Hash value identifying the loaded scripts contents
        /// </summary>
        public byte[] ScriptsHash { get
            {
                return Pool.ScriptRepository.Hash;
            }
        }

        /// <summary>
        /// Add a .Net host object with a given name to the globel namespace of the JS processor. 
        /// The object is removed (JS delete) from the engine when it is disposed of.
        /// </summary>
        /// <param name="globalname">the name of the host object in the JS global namespace</param>
        /// <param name="service">the .Net host object to be used as a service</param>
        public void AddHostService(string globalname, object service)
        {
            engine.AddHostObject(globalname, service);
            services.Add(globalname);
        }

        private string selfJson;

        /// <summary>
        /// Set the context (i.e. this) for the next calls to <see cref="Evaluate(string, object[])"/>. Defaults to null.
        /// </summary>
        /// <param name="self">This object will be serialized to JSON an submitted as 'this' to all next calls to <see cref="Evaluate(string, object[])"/>.</param>
        public void SetContext(object self)
        {
            selfJson = Newtonsoft.Json.JsonConvert.SerializeObject(self);
        }

        /// <summary>
        /// evaluate an expression OR call a function with parameters
        /// </summary>
        /// <param name="expression">A standard JS expressen, if parameters are missing, a function pointer, if parameters are present.</param>
        /// <param name="parameters">parameters are passed as an object array with the global name '__.p', then the expression 
        /// is extended with '.apply(JSON.parse(__.t), __.p)' and evaluated. '__' is deleted afterwards. 
        /// Note that 'this' will be the <see cref="SetContext(object)"/> in the function call, but 'this' will not be migrated back to the host.</param>
        /// <returns>the evaluated value</returns>
        public dynamic Evaluate(string expression, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                var __ = new { t = selfJson, p = parameters };
                engine.AddHostObject("__", __);
                var ret = engine.Evaluate(expression + ".apply(JSON.parse(__.t), convertToJsArray(__.p))");
                engine.AddHostObject("__", new object());
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
            sw.Stop();
            Console.WriteLine("Engine {4}#{0,-3} Thread {3,-3} disposed.", SerialNumber, 0, 0, Thread.CurrentThread.ManagedThreadId, PoolName);
        }

        void IDisposable.Dispose()
        {
            foreach (var s in services)
            {
                engine.AddHostObject(s, new object());
                engine.Execute(string.Format("delete {0};", s));
            }
            services.Clear();
            SetContext(null);
            sw.Stop();
            if (Pool.GarbageCollection != JsEnginePoolGarbageStrategy.Automatic)
                engine.CollectGarbage(exhaustive: (Pool.GarbageCollection == JsEnginePoolGarbageStrategy.ExhaustiveAfterUse));
            Console.WriteLine("Engine {0}#{1,-3} Thread {2,-3} used for {3}.", PoolName, SerialNumber, Thread.CurrentThread.ManagedThreadId, sw.Elapsed);
            Pool.ReturnToPool(this);
        }
    }
}
