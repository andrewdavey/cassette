/// <reference path="../app/namespace.js" />
/// <reference path="../app/Event.js" />
/// <reference path="../lib/Class.js" />
/// <reference path="../lib/jquery.js" />
/// <reference path="helpers.js" />

(function () {

    var helpers = this.Example.helpers;
    var Event = this.Example.Event;

    this.Example.ColorViewModel = Class.extend({

        init: function (viewData) {
            this.url = viewData.url;
            this.color = helpers.getColorHexString(
                viewData.red,
                viewData.green,
                viewData.blue
            );
            this.onDeleted = new Event();
        },

        deleteColor: function () {
            $.ajax({
                type: "delete",
                url: this.url
            });
            this.onDeleted.raise(this);
        }

    });

}).call(this);