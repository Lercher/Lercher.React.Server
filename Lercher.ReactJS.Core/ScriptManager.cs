using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    public class ScriptManager : IManageScripts
    {
        private readonly ConcurrentBag<ScriptManagerItem> scripts = new ConcurrentBag<ScriptManagerItem>();
        private bool closed = false;


        IEnumerable<ScriptManagerItem> IManageScripts.Scripts
        {
            get
            {
                return scripts.OrderBy(s => s.Sequence);
            }
        }

        public void AddScriptContent(string script, string url, int sequence)
        {
            if (string.IsNullOrEmpty(script))
                return;
            if (closed) throw new ApplicationException("can't add a script to a closed scriptmanager. Note: a scriptmanager is closed when it is associated with a " + nameof(JsEnginePool));
            scripts.Add(new ScriptManagerItem() { Code = script, Name = url, Sequence = sequence });
        }

        void IManageScripts.Close()
        {
            closed = true;
        }
    }
}
