using System;
using System.Linq;

namespace Lercher.ReactJS.Core
{
    /// <summary>
    /// Creator for a ReactEnvironment, start here.
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
        public string UseBabelVersion = "latest"; // 6
        public string UseReactVersion = "latest"; // 15
        public readonly ScriptLoader BabelLoader = new ScriptLoader();
        public readonly ScriptLoader ReactLoader = new ScriptLoader();
        public ScriptRepository BabelRepository = null;
        public ScriptRepository ReactRepository = null;
        private JsEnginePool BabelPool = null;
        private JsEnginePool ReactPool = null;
        private int n = 0;

        public Func<string, string> ScriptContentLoader = System.IO.File.ReadAllText;

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

        private JsEnginePool GetBabelPool()
        {
            LoadExternalScripts();
            if (BabelPool == null)
                BabelPool = new JsEnginePool(BabelRepository);
            return BabelPool;
        }

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

        public void AddJs(string name)
        {
            CheckNonFrozen();
            n++;
            var script = ScriptContentLoader(name);
            ReactRepository.AddScriptContent(script, Sanitize(name, n));
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
            catch(Exception)
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
                BabelPool.Dispose();
            BabelPool = null;
        }

        public ReactRuntime Freeze()
        {
            GetReactPool();
            return new ReactRuntime(ReactPool);
        }

        internal JsEnginePool GetReactPool()
        {
            LoadExternalScripts();
            if (ReactPool == null)
                ReactPool = new JsEnginePool(ReactRepository);
            return ReactPool;
        }
    }
}
