﻿using System;
using System.Collections.Generic;
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
            /* For JSX see https://gist.github.com/zpao/7f3f2063c3c2a39132e3 , https://github.com/babel/babel-standalone 
             * and https://github.com/facebook/react/issues/5497
             * all these use babel-standalone and even the babl REPL Website https://babeljs.io/repl
             * so we do too.
             * We need the function transformCode(code, url) from the gist as well as the var babelOpts, see below
             */
            var c = "\"use strict\"; var HelloWorld = React.createClass({ displayName: \"HelloWorld\",  render: function render() { return React.createElement(\"div\", null, \"Hello \", this.props.name, \"!\"); } }); ";
            var cJSX = "var HelloWorld = React.createClass({  render() { return (<div>Hello {this.props.name}!</div>); }  });";
            Console.WriteLine("Hello from Core");
            ThreadPool.SetMinThreads(8, 200);
            var sl = new ScriptLoader();
            sl.AddUrl("https://unpkg.com/react@15/dist/react.min.js");
            sl.AddUrl("https://unpkg.com/react-dom@15/dist/react-dom.min.js");
            sl.AddUrl("https://unpkg.com/react-dom@15/dist/react-dom-server.min.js");

            // TODO: babel is only needed for compiling JSX scripts. We shall do this with a different JsEnginePool and inject only the result scripts.
            // For now we only _load_ standalone-babel.
            sl.AddUrl("https://unpkg.com/babel-standalone@6/babel.min.js");
            sl.AddUrl("https://unpkg.com/babel-polyfill@6/dist/polyfill.min.js");

            var sm = sl.WaitForScriptManager();
            sm.AddScriptContent("var sm = 3; var square = ( a => a*a );", "", 100);
            sm.AddScriptContent(c, "HelloWorld", 110);
            using (var pool = new JsEnginePool(sm))
            {
                int n = 10;
                var cd = new CountdownEvent(n);
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