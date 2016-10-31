// convert a host array to a JS array

function convertToJsArray(host) {
    var a = [];
    var len = host.Length;
    for (var i = 0; i < len; i++)
        a[i] = host[i];
    return a;
}