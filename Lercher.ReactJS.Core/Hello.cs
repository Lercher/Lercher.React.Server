using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    public static class Hello
    {
        public static void SayHello()
        {
            Console.WriteLine("Hello from Core");
            var JSX = "var HelloWorld = React.createClass({  render() { return (<div>Hello {this.props.name}!</div>); }  });";

            // preparation phase
            var rt = new ReactRuntime(cfg => {
                cfg.ScriptContentLoader = (s => s); // we load literals for now, not files.
                cfg.AddJsx(JSX);
            });

            // runtime phase
            using (var engine = rt.ReactPool.Engine)
            {
                var o1 = engine.Evaluate("ReactDOMServer.renderToStaticMarkup(React.createElement(HelloWorld, { name: \"Mike Meyers\" }))");
                Console.WriteLine(o1);
                var o2 = engine.Evaluate("ReactDOMServer.renderToString(React.createElement(HelloWorld, { name: \"Mike Meyers\" }))");
                Console.WriteLine(o2);

                var rr = rt.RenderToStaticMarkup(engine, "HelloWorld", new { name = "World using engine" });
                Console.WriteLine(rr.render);
            }

            var sw = Stopwatch.StartNew();
            var r = rt.RenderToStaticMarkup("HelloWorld", new { name = "World" });
            sw.Stop();
            Console.WriteLine("{0} in {1}", r.render, sw.Elapsed); // 1.6ms on my i7 machine


            Console.WriteLine("Compiling {0} ...", JSX);
            var babel = new ScriptLoader();
            babel.AddUrl("https://unpkg.com/babel-standalone@6/babel.min.js");
            babel.AddUrl("https://unpkg.com/babel-polyfill@6/dist/polyfill.min.js");
            var babelRepository = babel.GetRepository();
            babelRepository.AddAssetResource("ArrayConverter.js"); // function convertToJsArray(host)
            babelRepository.AddAssetResource("JSX.js"); // function transformCode(code, url), function transformCode__2()
            using (var babelPool = new JsEnginePool(babelRepository))
            using (var e = babelPool.Engine)
            {
                var compiled = e.Evaluate("transformCode", JSX);
                Console.WriteLine(compiled);
            }


            /* For JSX see https://gist.github.com/zpao/7f3f2063c3c2a39132e3 , https://github.com/babel/babel-standalone 
             * and https://github.com/facebook/react/issues/5497
             * all these use babel-standalone and even the babl REPL Website https://babeljs.io/repl
             * so we do too.
             * We need the function transformCode(code, url) from the gist as well as the var babelOpts, see below
             */
            var c = "\"use strict\"; var HelloWorld = React.createClass({ displayName: \"HelloWorld\",  render: function render() { return React.createElement(\"div\", null, \"Hello \", this.props.name, \"!\"); } }); ";
            var sl = new ScriptLoader();
            sl.AddUrl("https://unpkg.com/react@15/dist/react.min.js");
            sl.AddUrl("https://unpkg.com/react-dom@15/dist/react-dom.min.js");
            sl.AddUrl("https://unpkg.com/react-dom@15/dist/react-dom-server.min.js");
            var repository = sl.GetRepository();
            repository.AddAssetResource("ArrayConverter.js"); // function convertToJsArray(host)
            repository.AddScriptContent("var sm = 3; var square = ( a => a*a );", "Test");
            repository.AddScriptContent(c, "HelloWorld");
            using (var pool = new JsEnginePool(repository))
            {
                int n = 10;
                var cd = new CountdownEvent(n);
                ThreadPool.SetMinThreads(8, 200);
                for (var i = 0; i < n; i++)
                {
                    var ii = i;
                    ThreadPool.QueueUserWorkItem(
                        (o) =>
                        {
                            using (var e = pool.Engine)
                            {
                                e.Calc2(ii);
                            }
                            cd.Signal();
                        }, null);
                }
                cd.Wait();
                Console.WriteLine("completed");
            }
        }
    }
}

#if false
// The options we'll pass will be pretty inline with what we're expecting people
// to write. It won't cover every use case but will set ES2015 as the baseline
// and transform JSX. We'll also support 2 in-process syntaxes since they are
// commonly used with React: class properties, Flow types, & object rest spread.
var babelOpts = {
  presets: [
    'react',
    'es2015',
  ],
  plugins: [
    'transform-class-properties',
    'transform-object-rest-spread',
    'transform-flow-strip-types',
  ],
  sourceMaps: 'inline',
};


/**
 * Actually transform the code.
 *
 * @param {string} code
 * @param {string?} url
 * @return {string} The transformed code.
 * @internal
 */
function transformCode(code, url) {
  var source;
  if (url != null) {
    source = url;
  } else {
    source = 'Inline Babel script';
    inlineScriptCount++;
    if (inlineScriptCount > 1) {
      source += ' (' + inlineScriptCount + ')';
    }
  }

  var transformed;
  try {
    transformed = babel.transform(code, assign({filename: source}, babelOpts));
  } catch (e) {
    throw e;
  }

  return transformed.code;
}

// --------------------------------------------- or from C:\Daten\GitHub\React.NET\src\React.Core\BabelConfig.cs

		public BabelConfig()
		{
			// Use es2015-no-commonjs by default so Babel doesn't prepend "use strict" to the start of the
			// output. This messes with the top-level "this", as we're not actually using JavaScript modules
			// in ReactJS.NET yet.
			Presets = new HashSet<string> { "es2015-no-commonjs", "stage-1", "react" };
			Plugins = new HashSet<string>();
		}
#endif