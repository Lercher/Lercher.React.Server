var HelloWorld = React.createClass({
    render() {
        return (
            <form action="#" method="post">
			    <div>
			        Hello {this.props.name}!
                    Not {this.props.name}? ... then enter your name: <input name="name" value={this.props.name}/>. Is it {global.form.name}?
                    <Inner name={this.props.name} />
                    <br/>
                    {convertToJsArray(this.props.values).map((v) =>
                        <InputBox for={v}/>
                    )}
			    </div>
                <div>
                    <input type="submit" value="Submit"/>
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
        return <input name={n} value={this.props.for}/>;
    }
});