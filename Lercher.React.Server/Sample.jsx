
var HelloWorld = React.createClass({
    render() {
        return (
			<div>
			    Hello {this.props.name}!
                <Inner name={this.props.name} />
                {this.props.values.map((v) =>
                    <InputBox for={v}/>
                )}
			</div>
		);
    }
});

var Inner = React.createClass({
    render() {
        return (
            <a href=".">
                Reload me (= {this.props.name})
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