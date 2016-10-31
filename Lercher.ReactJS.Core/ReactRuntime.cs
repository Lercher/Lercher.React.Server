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
        /// <param name="engine">the engine that evaluates the JS code.</param>
        /// <param name="reactDomServerMethodName">should be renderToStaticMarkup (without client side ReactJS markup) or renderToString.</param>
        /// <param name="componentName">a JS variable name that holds a React class such as "HelloWorld" after "var HelloWorld = React.createClass(...);" in a preloaded script.</param>
        /// <param name="model">The model data. If this is a string, it is interpreted as JSON and converted to a structure before creating the React element.</param>
        /// <returns>the created React element, the model as a structure and the render text.</returns>
        /// <remarks>For details on this process, see the embedded resource script called ReactStub.js</remarks>
        public ReactResult Run(JsEngine engine, string reactDomServerMethodName, string componentName, object model)
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
                return Run(engine, reactDomServerMethodName, componentName, model);
        }

        public ReactResult RenderToString(JsEngine engine, string componentName, object model)
        {
            return Run(engine, STR_RenderToString, componentName, model);
        }
        public ReactResult RenderToString(string componentName, object model)
        {
            return Run(STR_RenderToString, componentName, model);
        }

        public ReactResult RenderToStaticMarkup(JsEngine engine, string componentName, object model)
        {
            return Run(engine, STR_RenderToStaticMarkup, componentName, model);
        }

        public ReactResult RenderToStaticMarkup(string componentName, object model)
        {
            return Run(STR_RenderToStaticMarkup, componentName, model);
        }
    }
}
