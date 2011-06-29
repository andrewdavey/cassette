/// <reference path="../base/namespace.js" />
/// <reference path="../lib/Class.js" />
/// <reference path="../lib/knockout.js" />
/// <reference path="../lib/jquery.js" />

Example.ColorPickerViewModel = Class.extend({

    // Constructor of the view model object
    init: function (viewData) {
        this.red = ko.observable(viewData.FavoriteColor.Red);
        this.green = ko.observable(viewData.FavoriteColor.Green);
        this.blue = ko.observable(viewData.FavoriteColor.Blue);

        this.color = ko.dependentObservable(this.getCurrentColorHex, this);
    },

    getCurrentColorHex: function () {
        return "#" +
                this.convertNumberToHexString(this.red()) +
                this.convertNumberToHexString(this.green()) +
                this.convertNumberToHexString(this.blue());
    },

    convertNumberToHexString: function (number) {
        return ('0' + number.toString(16)).substr(-2);
    },

    random: function () {
        this.red(Math.floor(Math.random() * 255));
        this.green(Math.floor(Math.random() * 255));
        this.blue(Math.floor(Math.random() * 255));
    },

    save: function () {
        $.post(
            "/home/save",
            {
                red: this.red(),
                blue: this.blue(),
                green: this.green()
            }
        );
    }

});