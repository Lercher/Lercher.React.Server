using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

namespace Lercher.ReactJS.Core
{
    public class ScriptRepository : IListScripts
    {
        private readonly ConcurrentBag<ScriptItem> scripts = new ConcurrentBag<ScriptItem>();
        private bool frozen = false;
        public int Sequence = 0;
        private byte[] hash;

        public IEnumerable<ScriptItem> Scripts
        {
            get
            {
                return scripts.OrderBy(s => s.Sequence);
            }
        }

        // Note: This is not a direct requirement for compiling and rendering React code, 
        // but for it's intended use case, where we need to know if some rendered UI 
        // is or is not built from identical sources.
        public byte[] Hash
        {
            get
            {
                return hash;
            }
        }

        public void AddScriptContent(string script, string url)
        {
            Sequence++;
            AddScriptContent(script, url, Sequence);
        }

        public void AddScriptContent(string script, string url, int sequence)
        {
            if (string.IsNullOrEmpty(script)) return;
            if (frozen) throw new ApplicationException("can't add a script to a frozen scriptmanager. Note: a " + nameof(ScriptRepository) + " becomes frozen when it is associated with a " + nameof(JsEnginePool));
            scripts.Add(new ScriptItem() { Code = script, Name = url, Sequence = sequence });
            Console.WriteLine("Loaded script {0}", url);
        }

        public void AddAssetResource(string filename)
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            using (var st = a.GetManifestResourceStream(this.GetType(), "Assets." + filename))
            using (var tr = new System.IO.StreamReader(st))
            {
                var script = tr.ReadToEnd();
                AddScriptContent(script, "resource:" + filename);
            }
        }
        
        void IListScripts.Freeze()
        {
            frozen = true;
            var n = 0;
            using (var hasher = HashAlgorithm.Create("SHA256"))
            {
                foreach(var s in Scripts)
                {
                    n++;
                    var b = System.Text.Encoding.UTF8.GetBytes(s.Code);
                    var h = hasher.ComputeHash(b);
                    if (hash == null)
                        hash = h;
                    else
                    {
                        for (int i = 0; i < h.Length; i++)
                            hash[i] ^= h[i]; // XOR
                    }
                }
            }
            Console.WriteLine("Hash of {0:n0} script(s) is {1}", n, Convert.ToBase64String(hash));
        }
    }
}
