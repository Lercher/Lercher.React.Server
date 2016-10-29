using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    public class ScriptManager : IManageScripts
    {
        private readonly List<string> scripts = new List<string>();
        private bool closed = false;


        IEnumerable<string> IManageScripts.Scripts
        {
            get
            {
                return scripts;
            }
        }

        public void AddScriptContent(string script)
        {
            if (string.IsNullOrEmpty(script))
                return;
            if (closed) throw new ApplicationException("can't add a script to a closed scriptmanager. Note: a scriptmanager is closed when it is associated with a " + nameof(JsEnginePool));
            scripts.Add(script);
        }

        void IManageScripts.Close()
        {
            closed = true;
        }
    }
}
