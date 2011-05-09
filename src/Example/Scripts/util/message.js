/// <reference path="../base/namespace.js" />
/// <reference path="../lib/jquery.js" />
/// <reference path="test.coffee" />

Example.showMessage = function (message) {
    var t = new Example.Test();
    $("#message").text(message + t.hello);
};