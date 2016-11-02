using System;
using System.Linq;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// Configurator for a ReactEnvironment. 
    /// Loads ReactJS and Babel scripts from the https://unpkg.com CDN. 
    /// Loads and compiles static JSX files to JS. Adds JS files.
    /// </summary>
    /// <remarks>
    /// Usage protocol:
    /// 1 using(new)
    /// 0..1 set/check UseBabel/ReactVersion
    /// 0..1 set/check ScriptContentLoader
    /// then
    /// 1..n AddJsx() / AddJs()
    /// then
    /// 1 Freeze() -> ReactEnvironment
    /// then
    /// 1 Dispose() or end of using
    /// </remarks>
    public class ReactConfiguration : IDisposable
    {
        /// <summary>
        /// Change this value to load a particular Babel version. Default = 'latest'.
        /// </summary>
        public string UseBabelVersion = "latest"; // 6

        /// <summary>
        /// Change this value to load a particular ReactJS version. Default = 'latest'.
        /// </summary>
        public string UseReactVersion = "latest"; // 15

        /// <summary>
        /// The <see cref="ScriptLoader"/> for Babel scripts.
        /// </summary>
        public readonly ScriptLoader BabelLoader = new ScriptLoader();

        /// <summary>
        /// The <see cref="ScriptLoader"/> for ReactJS scripts.
        /// </summary>
        public readonly ScriptLoader ReactLoader = new ScriptLoader();

        /// <summary>
        /// The <see cref="ScriptRepository"/> for Babel scripts.
        /// </summary>
        public ScriptRepository BabelRepository = null;

        /// <summary>
        /// The <see cref="ScriptRepository"/> for ReactJS scripts.
        /// </summary>
        public ScriptRepository ReactRepository = null;

        private JsEnginePool BabelPool = null;
        private JsEnginePool ReactPool = null;
        internal ScriptsWatcher Watcher = null;
        private int n = 0;

        /// <summary>
        /// The implementation to transform script names to their content.
        /// Defaults to System.IO.File.ReadAllText.
        /// </summary>
        public Func<string, string> ScriptContentLoader = System.IO.File.ReadAllText;


        /// <summary>
        /// Request Babel and ReactJS scripts from https://unpkg.com and freeze the two <see cref="ScriptLoader"/>s.
        /// Add our own appropriate JS helper scripts from embedded ressources in this library.
        /// </summary>
        public void LoadExternalScripts()
        {
            if (BabelRepository != null && ReactRepository != null) return;

            BabelLoader.AddUrl("https://unpkg.com/babel-standalone@{0}/babel.min.js", UseBabelVersion);
            BabelLoader.AddUrl("https://unpkg.com/babel-polyfill@{0}/dist/polyfill.min.js", UseBabelVersion);

            ReactLoader.AddUrl("https://unpkg.com/react@{0}/dist/react.min.js", UseReactVersion);
            ReactLoader.AddUrl("https://unpkg.com/react-dom@{0}/dist/react-dom.min.js", UseReactVersion);
            ReactLoader.AddUrl("https://unpkg.com/react-dom@{0}/dist/react-dom-server.min.js", UseReactVersion);

            BabelRepository = BabelLoader.GetRepository();
            BabelRepository.AddAssetResource("ArrayConverter.js"); // function convertToJsArray(host)
            BabelRepository.AddAssetResource("JSX.js"); // function transformCode(code, url), function transformCode__2()

            ReactRepository = ReactLoader.GetRepository();
            ReactRepository.AddAssetResource("ArrayConverter.js"); // function convertToJsArray(host)
            ReactRepository.AddAssetResource("ReactStub.js"); // function PrepareReact(rfunc, component)
        }

        /// <summary>
        /// Freeze the Babel <see cref="ScriptRepository"/> and create a <see cref="JsEnginePool"/> for JSX to JS compilation with Babel.
        /// </summary>
        /// <returns>A <see cref="JsEnginePool"/> with initialized Babel, ready for compiling JSX to JS</returns>
        private JsEnginePool GetBabelPool()
        {
            LoadExternalScripts();
            if (BabelPool == null)
            {
                BabelPool = new JsEnginePool(BabelRepository);
                BabelPool.Name = "BabelPool";
            }
            return BabelPool;
        }

        /// <summary>
        /// Add a static JSX file. The name is fed to <see cref="ScriptContentLoader"/> to get the contents of the script.
        /// Defaults to loading the local file with name as path.
        /// This content is then compiled to JS using Babel with react addin as a compiler.
        /// Finally the JS script is added to the <see cref="ReactRepository"/>.
        /// </summary>
        /// <param name="name">Locator of a JSX script resource. Usually a file path.</param>
        public void AddJsx(string name)
        {
            CheckNonFrozen();
            n++;
            using (var e = GetBabelPool().Engine)
            {
                var JSX = ScriptContentLoader(name);
                var compiled = e.Evaluate("transformCode", JSX);
                ReactRepository.AddScriptContent(compiled.ToString(), Sanitize(name, n));
            }
        }

        /// <summary>
        /// Add a static JS file. The name is fed to <see cref="ScriptContentLoader"/> to get the contents of the script.
        /// Defaults to loading the local file with name as path.
        /// The contents are added to the <see cref="ReactRepository"/>.
        /// Note that this method does no JSX to JS compilation, but see also <see cref="AddJsx(string)"/>.
        /// </summary>
        /// <param name="name">Locator of a JS script resource. Usually a file path.</param>
        public void AddJs(string name)
        {
            CheckNonFrozen();
            n++;
            var script = ScriptContentLoader(name);
            ReactRepository.AddScriptContent(script, Sanitize(name, n));
        }

        public void WatchDirectory(string path, TimeSpan gracePeriod)
        {
            if (Watcher != null) throw new ApplicationException("you can attach only one directory to be watched for changes");
            Watcher = new ScriptsWatcher(path, gracePeriod);
            Watcher.EnumerateJsAndJsx(this);
        }

        private static string Sanitize(string url, int n)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) throw new Exception();
                System.IO.Path.Combine(url); // throws if invalid file
                if (url.Contains('*')) throw new Exception();
                if (url.Contains('?')) throw new Exception();
                return url.Replace(System.IO.Path.DirectorySeparatorChar, '/');
            }
            catch (Exception)
            {
                return string.Format("script{0}", n);
            }
        }


        private void CheckNonFrozen()
        {
            if (ReactPool != null) throw new ApplicationException("can't add scripts to a frozen " + nameof(ReactConfiguration));
        }

        void IDisposable.Dispose()
        {
            if (BabelPool != null)
                ((IDisposable)BabelPool).Dispose();
            BabelPool = null;
        }

        /// <summary>
        /// Freezes script compilation and/or loading and returns a new initialized and configured <see cref="ReactRuntime"/>.
        /// </summary>
        /// <returns>A <see cref="ReactRuntime"/> that was configured by this object.</returns>
        public ReactRuntime Freeze()
        {
            GetReactPool();
            return new ReactRuntime(ReactPool);
        }

        internal JsEnginePool GetReactPool()
        {
            LoadExternalScripts();
            if (ReactPool == null)
            {
                ReactPool = new JsEnginePool(ReactRepository);
                ReactPool.Name = "ReactPool";
            }
            return ReactPool;
        }
    }
}
