using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// A JsEnginePool is initalized from an IListScripts, that is a readonly ordered list of JS Script contents
    /// </summary>
    public interface IListScripts
    {
        /// <summary>
        /// List the stored scripts in order.
        /// </summary>
        IEnumerable<ScriptItem> Scripts { get; }

        /// <summary>
        /// After freezing, you can't add more scripts to the repository.
        /// A repository is frozen at the latest, when it is assitiated with a JsEnginePool
        /// </summary>
        void Freeze();

        /// <summary>
        /// A SHA256 hash value of all the loaded scripts in order.
        /// </summary>
        /// <remarks>
        /// It is available only after freezing, before freezing it's value null. 
        /// The value should be identical over OS processes with the same script contents.
        /// The current implementation does not yet honor ordering.
        /// </remarks>
        byte[] Hash { get; }
    }
}
