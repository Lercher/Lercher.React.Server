﻿using System;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// Used to instanciate and render ReactJS components to strings.
    /// Main entry point of this library.
    /// </summary>
    public class ReactRuntime
    {
        const string STR_RenderToString = "renderToString";
        const string STR_RenderToStaticMarkup = "renderToStaticMarkup";

        /// <summary>
        /// A <see cref=" JsEnginePool"/> to allocate initialized ReactJS processors from.
        /// </summary>
        public readonly JsEnginePool ReactPool;

        /// <summary>
        /// Create a runtime with a configuring Action.
        /// Enables inline refreshing of script repositories.
        /// </summary>
        /// <param name="configure">An <see cref="Action{ReactConfiguration}"/> that configures this runtime. Now and in case of inline refreshment.</param>
        public ReactRuntime(Action<ReactConfiguration> configure)
        {
            using (var cfg = new ReactConfiguration())
            {
                configure(cfg);
                ReactPool = cfg.GetReactPool();
            }
        }

        /// <summary>
        /// Create a runtime with a static prefabricated <see cref="JsEnginePool"/>.
        /// This is a low level method to create a runtime e.g. without loading ReactJS and/or Babel.
        /// Runtimes created with this constructor can't be refreshed inline.
        /// </summary>
        /// <param name="reactPool">A <see cref="JsEnginePool"/> that can run ReactDOMServer methods, has ReactJS classes and our JS function PrepareReact/2.</param>
        public ReactRuntime(JsEnginePool reactPool)
        {
            ReactPool = reactPool;
        }

        /// <summary>
        /// Most basic method to run some react rendering code on a component and a model.
        /// </summary>
        /// <param name="reactDomServerMethodName">should be renderToStaticMarkup (without client side ReactJS markup) or renderToString.</param>
        /// <param name="componentName">a JS variable name that holds a React class such as "HelloWorld" after "var HelloWorld = React.createClass(...);" in a preloaded script.</param>
        /// <param name="model">The model data. If this is a string, it is interpreted as JSON and converted to a structure before creating the React element.</param>
        /// <param name="engine">the engine that evaluates the JS code.</param>
        /// <returns>the created React element, the model as a structure and the render text.</returns>
        /// <remarks>
        /// <para>Using a JSON serialized model is usually quicker and more direct to use, because you don't have to convert Host arrays to JS arrays with eg. our convertToJsArray() function.
        /// If you have to use a method of the model object - it doesn't survive the serialization process - you must stick to the probably slower JS-to-Host-and-back fiddling.
        /// </para>
        /// <para>For additional details on on the call, see the embedded resource script called ReactStub.js</para>
        /// </remarks>
        public ReactResult Run(string reactDomServerMethodName, string componentName, object model, JsEngine engine)
        {
            Contract.Assert(engine != null, nameof(engine) + " is null.");
            Contract.Assert(!string.IsNullOrEmpty(reactDomServerMethodName), nameof(reactDomServerMethodName) + " is null or empty.");
            Contract.Assert(!string.IsNullOrEmpty(componentName), nameof(componentName) + " is null or empty.");
            Contract.Assert(model != null, nameof(model) + " is null.");

            var expr = string.Format("PrepareReact(ReactDOMServer.{0}, {1})", reactDomServerMethodName, componentName);
            dynamic result = engine.Evaluate(expr, model);
            return new ReactResult() { element = result.element, model = result.model, render = result.render };
        }

        /// <summary>
        /// See <see cref="Run(string, string, object, JsEngine)"/>. The engine is allocated from and returned to the <see cref="ReactPool"/> automatically.
        /// </summary>
        /// <param name="reactDomServerMethodName">should be renderToStaticMarkup (without client side ReactJS markup) or renderToString.</param>
        /// <param name="componentName">a JS variable name that holds a React class such as "HelloWorld" after "var HelloWorld = React.createClass(...);" in a preloaded script.</param>
        /// <param name="model">The model data. If this is a string, it is interpreted as JSON and converted to a structure before creating the React element.</param>
        /// <returns>the created React element, the model as a structure and the render text.</returns>
        public ReactResult Run(string reactDomServerMethodName, string componentName, object model)
        {
            using (var engine = ReactPool.Engine)
                return Run(reactDomServerMethodName, componentName, model, engine);
        }

        /// <summary>
        /// See <see cref="Run(string, string, object, JsEngine)"/> using renderToString.
        /// </summary>
        /// <param name="componentName">a JS variable name that holds a React class such as "HelloWorld" after "var HelloWorld = React.createClass(...);" in a preloaded script.</param>
        /// <param name="model">The model data. If this is a string, it is interpreted as JSON and converted to a structure before creating the React element.</param>
        /// <param name="engine">the engine that evaluates the JS code.</param>
        /// <returns>the created React element, the model as a structure and the render text.</returns>
        public ReactResult RenderToString(string componentName, object model, JsEngine engine)
        {
            return Run(STR_RenderToString, componentName, model, engine);
        }

        /// <summary>
        /// See <see cref="Run(string, string, object, JsEngine)"/> using renderToString. The engine is allocated from and returned to the <see cref="ReactPool"/> automatically.
        /// </summary>
        /// <param name="componentName">a JS variable name that holds a React class such as "HelloWorld" after "var HelloWorld = React.createClass(...);" in a preloaded script.</param>
        /// <param name="model">The model data. If this is a string, it is interpreted as JSON and converted to a structure before creating the React element.</param>
        /// <returns>the created React element, the model as a structure and the render text.</returns>
        public ReactResult RenderToString(string componentName, object model)
        {
            return Run(STR_RenderToString, componentName, model);
        }

        /// <summary>
        /// See <see cref="Run(string, string, object, JsEngine)"/> using renderToStaticMarkup.
        /// </summary>
        /// <param name="componentName">a JS variable name that holds a React class such as "HelloWorld" after "var HelloWorld = React.createClass(...);" in a preloaded script.</param>
        /// <param name="model">The model data. If this is a string, it is interpreted as JSON and converted to a structure before creating the React element.</param>
        /// <param name="engine">the engine that evaluates the JS code.</param>
        /// <returns>the created React element, the model as a structure and the render text.</returns>
        public ReactResult RenderToStaticMarkup(string componentName, object model, JsEngine engine)
        {
            return Run(STR_RenderToStaticMarkup, componentName, model, engine);
        }

        /// <summary>
        /// See <see cref="Run(string, string, object, JsEngine)"/> using renderToStaticMarkup. The engine is allocated from and returned to the <see cref="ReactPool"/> automatically.
        /// </summary>
        /// <param name="componentName">a JS variable name that holds a React class such as "HelloWorld" after "var HelloWorld = React.createClass(...);" in a preloaded script.</param>
        /// <param name="model">The model data. If this is a string, it is interpreted as JSON and converted to a structure before creating the React element.</param>
        /// <returns>the created React element, the model as a structure and the render text.</returns>
        public ReactResult RenderToStaticMarkup(string componentName, object model)
        {
            return Run(STR_RenderToStaticMarkup, componentName, model);
        }
    }
}
