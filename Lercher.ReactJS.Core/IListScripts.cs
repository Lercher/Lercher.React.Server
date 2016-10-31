using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    public interface IListScripts
    {
        IEnumerable<ScriptItem> Scripts { get; }
        void Freeze();

        /// <summary>
        /// A SHA256 hash value of all the loaded scripts in order.
        /// </summary>
        /// <remarks>
        /// It is available only after freezing, before it is null. 
        /// The value should be identical over OS processes with the same script contents.
        /// </remarks>
        byte[] Hash { get; }
    }
}
