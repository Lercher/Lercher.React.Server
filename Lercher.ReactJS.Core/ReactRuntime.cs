using System;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Lercher.ReactJS.Core
{
    public class ReactRuntime
    {
        const string STR_RenderToString = "renderToString";
        const string STR_RenderToStaticMarkup = "renderToStaticMarkup";

        public readonly JsEnginePool ReactPool;

        public ReactRuntime(Action<ReactConfiguration> configure)
        {
            using (var cfg = new ReactConfiguration())
            {
                configure(cfg);
                ReactPool = cfg.GetReactPool();
            }
        }

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

        public ReactResult Run(string reactDomServerMethodName, string componentName, object model)
        {
            using (var engine = ReactPool.Engine)
                return Run(reactDomServerMethodName, componentName, model, engine);
        }

        public ReactResult RenderToString(string componentName, object model, JsEngine engine)
        {
            return Run(STR_RenderToString, componentName, model, engine);
        }
        public ReactResult RenderToString(string componentName, object model)
        {
            return Run(STR_RenderToString, componentName, model);
        }

        public ReactResult RenderToStaticMarkup(string componentName, object model, JsEngine engine)
        {
            return Run(STR_RenderToStaticMarkup, componentName, model, engine);
        }

        public ReactResult RenderToStaticMarkup(string componentName, object model)
        {
            return Run(STR_RenderToStaticMarkup, componentName, model);
        }
    }
}
