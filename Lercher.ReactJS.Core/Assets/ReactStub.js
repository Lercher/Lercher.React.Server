// Just a stub to call ReactDOMServer methods
// The outer function has only JS processor controlled parameters
// while the inner function only has host controlled parameters, including 'this'.
function noop() { };

function PrepareReact(rfunc, component, preprocess, postprocess) {
    preprocess = preprocess || noop;
    postprocess = postprocess || noop;
    return function (modelOrString) {
        var model = modelOrString;
        var modelWasJson = (typeof modelOrString === "string");
        if (modelWasJson)
            model = JSON.parse(modelOrString);
        preprocess.call(this, model);
        var element = React.createElement(component, model);
        var render = rfunc(element);
        postprocess.call(this, model);
        var modelAsJson = null;
        if (modelWasJson)
            modelAsJson = JSON.stringify(model, null, ' '); // pretty printed
        return { modelAsJson, render };
    };
}