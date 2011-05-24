/// <reference path="../base/namespace.js" />
/// <reference path="../lib/Class.js" />
/// <reference path="../lib/knockout.js" />

Example.ColorPickerViewModel = Class.extend({
    init: function () {
        this.red = ko.observable(0);
        this.green = ko.observable(0);
        this.blue = ko.observable(0);

        this.color = ko.dependentObservable(function () {
            return "#" +
                   ('0' + this.red().toString(16)).substr(-2) +
                   ('0' + this.green().toString(16)).substr(-2) +
                   ('0' + this.blue().toString(16)).substr(-2);
        }, this);
    }
});