using System;
using System.Linq;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// Items of <see cref=" IListScripts"/>
    /// </summary>
    public class ScriptItem
    {
        /// <summary>
        /// Script content
        /// </summary>
        public string Code;

        /// <summary>
        /// Script name to be passed to the JS processor. Usually a URL or a filename.
        /// </summary>
        public string Name;

        /// <summary>
        /// Sequence number to fix the order of presenting the <see cref="Code"/> to the JS processor
        /// </summary>
        public int Sequence;
    }
}
