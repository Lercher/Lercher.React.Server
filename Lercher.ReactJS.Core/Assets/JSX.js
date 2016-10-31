/* This file is a modified excerpt from https://gist.github.com/zpao/7f3f2063c3c2a39132e3
 * see https://github.com/facebook/react/issues/5497
 *
 * see https://github.com/babel/babel-standalone for using babel-standalone:

=== Usage ===

Load  babel.js  or  babel.min.js  in your environment. This will expose Babel's API in a  Babel  object:

var input = 'const getMessage = () => "Hello World";';
var output = Babel.transform(input, { presets: ['es2015'] }).code;
 */

function transformCode(code) {
    // The options we'll pass will be pretty inline with what we're expecting people
    // to write. It won't cover every use case but will set ES2015 as the baseline
    // and transform JSX. We'll also support 2 in-process syntaxes since they are
    // commonly used with React: class properties, Flow types, & object rest spread.
    //+
    // Use es2015-no-commonjs by default so Babel doesn't prepend "use strict" to the start of the
    // output. This messes with the top-level "this", as we're not actually using JavaScript modules
    // in ReactJS.NET yet.
    var babelOpts = {
        presets: ['react', "es2015-no-commonjs", "stage-1"],
        plugins: ['transform-class-properties', 'transform-object-rest-spread', 'transform-flow-strip-types'],
        sourceMaps: false // 'inline'
    };

    var transformed;
    try {
        transformed = Babel.transform(code, babelOpts);
    } catch (e) {
        throw e;
    }
    return transformed.code;
}
