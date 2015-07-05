/// <reference path="../app/util.js"/>
/// <reference path="../app/require.js"/>

var page = {
    go: function () {
        util.message("Page 1");

        require(["Scripts/page2/init"]);
    }
};