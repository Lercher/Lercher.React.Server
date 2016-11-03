using System;
using System.Linq;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// Represents the results and intermediary results of a complete ReactDOMServer.renderToStaticMarkup() or ReactDOMServer.renderToString() call.
    /// See also <see cref="ReactRuntime"/>.
    /// </summary>
    public struct ReactResult
    {
        /// <summary>
        /// The model structure after calls to React.createElement(component, model) and ReactDOMServer.renderToStaticMarkup() / ReactDOMServer.renderToString()
        /// </summary>
        public string modelAsJson;

        /// <summary>
        /// The return value of the ReactDOMServer.renderToStaticMarkup() / ReactDOMServer.renderToString() call.
        /// </summary>
        public string render;
    }
}
