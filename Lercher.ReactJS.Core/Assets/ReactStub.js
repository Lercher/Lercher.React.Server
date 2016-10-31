// Just a stub to call ReactDOMServer methods
function PrepareReact(rfunc, component) {
    return function (modelOrString) {
        var model = modelOrString;
        if (typeof modelOrString === "string")
            model = JSON.parse(modelOrString);
        var element = React.createElement(component, model);
        var render = rfunc(element);
        return { element, model, render };
    };
}