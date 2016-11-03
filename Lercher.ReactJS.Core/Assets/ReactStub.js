// Just a stub to call ReactDOMServer methods
// The outer function has only JS processor controlled parameters
// while the inner function only has host controlled parameters, including 'this'.
var global = {};
function PrepareReact(rfunc, component) {
    return function (modelOrString) {
        global.form = this;
        var model = modelOrString;
        var modelWasJson = (typeof modelOrString === "string");
        if (modelWasJson)
            model = JSON.parse(modelOrString);
        var element = React.createElement(component, model);
        var render = rfunc(element);
        var modelAsJson = null;
        if (modelWasJson)
            modelAsJson = JSON.stringify(model, null, ' '); // pretty printed
        delete global.form;
        return { modelAsJson, render };
    };
}