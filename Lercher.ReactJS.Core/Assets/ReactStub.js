// Just a stub to call ReactDOMServer methods
function PrepareReact(rfunc, component) {
    return function (modelOrString) {
        var model = modelOrString;
        var modelWasJson = (typeof modelOrString === "string");
        if (modelWasJson)
            model = JSON.parse(modelOrString);
        var element = React.createElement(component, model);
        var render = rfunc(element);
        var modelAsJson = null;
        if (modelWasJson)
            modelAsJson = JSON.stringify(model, null, ' '); // pretty printed
        return { modelAsJson, render };
    };
}