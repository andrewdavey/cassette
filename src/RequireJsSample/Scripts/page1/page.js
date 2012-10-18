/// <reference path="../app/util.js"/>

var page = {
    go: function () {
        util.message("Page 1");

        require(["Scripts/page2/init"]);
    }
};