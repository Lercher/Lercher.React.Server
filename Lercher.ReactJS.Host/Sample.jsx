/*
 *  Copyright (c) 2015, Facebook, Inc.
 *  All rights reserved.
 *
 *  This source code is licensed under the BSD-style license found in the
 *  LICENSE file in the root directory of this source tree. An additional grant
 *  of patent rights can be found in the PATENTS file in the same directory.
 */

var HelloWorld = React.createClass({
    render() {
        return (
			<div>
			    Hello {this.props.name}!
                <Inner name={this.props.name} />
                {convertToJsArray(this.props.values).map((v) =>
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