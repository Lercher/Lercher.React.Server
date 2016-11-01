using System;
using System.Linq;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// Represents the results and intermediary results of a complete ReactDOMServer.renderToStaticMarkup() or ReactDOMServer.renderToString() call.
    /// See also <see cref="ReactRuntime"/>.
    /// </summary>
    public class ReactResult
    {
        /// <summary>
        /// The result of a React.createElement(component, model) call
        /// </summary>
        public dynamic element;

        /// <summary>
        /// The model structure after calls to React.createElement(component, model) and ReactDOMServer.renderToStaticMarkup() / ReactDOMServer.renderToString()
        /// </summary>
        public dynamic model;

        /// <summary>
        /// The return value of the ReactDOMServer.renderToStaticMarkup() / ReactDOMServer.renderToString() call.
        /// </summary>
        public string render;
    }
}
