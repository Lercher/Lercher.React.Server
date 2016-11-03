// infrastructure

var global;

function reactPreprocessor(model) {
    global = {
        form: this,
        actions: []
    };
}

function reactPostprocessor(model) {
    global.actions.forEach(a => a());
    delete global.form;
    delete global.actions;
}

function action(name, current, change) {
    if (!global.form) return current;
    var value = global.form[name];
    if (typeof value === "undefined") return current;
    if (value === current) return current;
    var a = () => change(value);
    global.actions.push(a);
    return value;
}

function v_i_ar(n, self) {
    return action(n, self.props.v, (value) => self.props.ar[self.props.i] = value);
}

// react components

var HelloWorld = React.createClass({
    render() {
        return (
            <form action="#" method="post">
                <div>
                    Hello {this.props.name}!
                    Not {this.props.name}? ... then enter your name: <input name="name" value={this.props.name } />. Is it {global.form.name}?
                    <Inner name={this.props.name} />
                    <br />
                    {convertToJsArray(this.props.values).map((v, i, ar) =>
                        <InputBox v={v} i={i} ar={ar} />
                    )}
                </div>
                <div>
                    <input type="submit" value="Submit" />
                </div>
            </form>
        );
    }
});

var Inner = React.createClass({
    render() {
        return (
            <a href="#">
                Reload s (= {this.props.name})
            </a>
        );
    }
});

var InputBox = React.createClass({
    render() {
        var n = names.getNextName();
        var v = v_i_ar(n, this);
        return <input name={n} value={v} />;
    }
});