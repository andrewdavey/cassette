/// <reference path="namespace.js" />
/// <reference path="../lib/Class.js" />

(function () {

    this.Example.Event = Class.extend({

        init: function () {
            this.handlers = [];
        },

        addHandler: function (handler) {
            this.handlers.push(handler);
        },

        raise: function () {
            var eventArgs = arguments;
            this.handlers.forEach(function (handler) {
                handler.apply(null, eventArgs);
            });
        }

    });

}).call(this);