using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// The standard implementation of a <see cref="IListScripts"/>.
    /// Usually created by a <see cref="ScriptLoader"/>.
    /// </summary>
    public class ScriptRepository : IListScripts
    {
        private readonly ConcurrentBag<ScriptItem> scripts = new ConcurrentBag<ScriptItem>();
        private bool frozen = false;
        internal int Sequence = 0;
        private byte[] hash;

        IEnumerable<ScriptItem> IListScripts.Scripts
        {
            get
            {
                return scripts.OrderBy(s => s.Sequence);
            }
        }

        // Note: This is not a direct requirement for compiling and rendering React code, 
        // but for it's intended use case, where we need to know if some rendered UI 
        // is or is not built from identical sources.
        // Note: The currently implemented hash algorithm ignores order.
        byte[] IListScripts.Hash
        {
            get
            {
                return hash;
            }
        }

        /// <summary>
        /// Add a script to the list.
        /// </summary>
        /// <param name="script">JS script content</param>
        /// <param name="name">Name of the script to pass to the JS processor, usually a filename or a URL.</param>
        public void AddScriptContent(string script, string name)
        {
            Sequence++;
            AddScriptContent(script, name, Sequence);
        }

        /// <summary>
        /// Add a script to the list with an ordering number.
        /// This method is handy, if you load scripts asynchronuously for yourself, but our <see cref="ScriptLoader"/> can do this for you.
        /// </summary>
        /// <param name="script">JS script content</param>
        /// <param name="name">Name of the script to pass to the JS processor, usually a filename or a URL.</param>
        /// <param name="sequence">Number used for ordering, lower numbers are listed first.</param>
        public void AddScriptContent(string script, string name, int sequence)
        {
            if (string.IsNullOrEmpty(script)) return;
            if (frozen) throw new ApplicationException("can't add a script to a frozen scriptmanager. Note: a " + nameof(ScriptRepository) + " becomes frozen when it is associated with a " + nameof(JsEnginePool));
            scripts.Add(new ScriptItem() { Code = script, Name = name, Sequence = sequence });
            Console.WriteLine("Loaded script {0}", name);
        }

        /// <summary>
        /// Add a script, which contents are stored as an embedded resource in the folder Assets.
        /// </summary>
        /// <param name="filename">The filename of the embedded resource.</param>
        internal void AddAssetResource(string filename)
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
                foreach(var s in ((IListScripts)this).Scripts)
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
