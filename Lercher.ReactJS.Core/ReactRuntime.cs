using System;
using System.Linq;

namespace Lercher.ReactJS.Core
{
    public class ReactRuntime
    {
        public readonly JsEnginePool ReactPool;

        public ReactRuntime(JsEnginePool reactPool)
        {
            ReactPool = reactPool;
        }

        public ReactResult Run(JsEngine engine, string reactDomServerMethodName, string componentName, object model)
        {
            var expr = string.Format("PrepareReact(ReactDOMServer.{0}, {1})", reactDomServerMethodName, componentName);
            dynamic result = engine.Evaluate(expr, model);
            return new ReactResult() { element = result.element, model = result.model, render = result.render };
        }
    }
}
