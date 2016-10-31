// convert a host array to a JS array

function convertToJsArray(host) {
    if (Array.isArray(host))
        return host; // don't convert a JS array

    // it's a .Net host array
    var a = [];
    var len = host.Length; 
    for (var i = 0; i < len; i++)
        a[i] = host[i];
    return a;
}