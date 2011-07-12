/// <reference path="../app/namespace.js" />
/// <reference path="../lib/Class.js" />
/// <reference path="../lib/knockout.js" />
/// <reference path="../lib/jquery.js" />
/// <reference path="ColorViewModel.js" />
/// <reference path="helpers.js" />

// Immediate anonymous function wraps entire file to prevent code leaking into global scope.
(function () {

    // Locally reference external objects and functions. (Optional, but can save on lots of typing!)
    var helpers = this.Example.helpers;
    var ColorViewModel = this.Example.ColorViewModel;

    // Add a new class to the application's global namespace
    this.Example.ColorPickerViewModel = Class.extend({

        // Constructor of the view model object
        init: function (pageViewData) {
            this.colorsUrl = pageViewData.colorsUrl;

            // Observable properties
            this.red = ko.observable(0);
            this.green = ko.observable(0);
            this.blue = ko.observable(0);
            this.color = ko.dependentObservable(this.getCurrentColorHex, this);
            this.savedColors = ko.observableArray([]);

            this.downloadColors();
        },

        downloadColors: function () {
            var colorPicker = this;
            $.get(
                this.colorsUrl,
                function (colors) {
                    colors.forEach(function (colorData) {
                        colorPicker.addColor(colorData);
                    });
                }
            );
        },

        getCurrentColorHex: function () {
            return helpers.getColorHexString(
                this.red(),
                this.green(),
                this.blue()
            );
        },

        random: function () {
            var color = helpers.randomColor();

            this.red(color.red);
            this.green(color.green);
            this.blue(color.blue);
        },

        save: function () {
            var data = {
                red: this.red(),
                blue: this.blue(),
                green: this.green()
            };
            $.ajax({
                type: "post",
                url: this.colorsUrl,
                data: data,
                complete: function (xhr) {
                    // Expect the server to return with status code 201 Created
                    // and a URL for the new color resource.
                    data.url = xhr.getResponseHeader("Location");
                    this.addColor(data);
                } .bind(this)
            });
        },

        addColor: function (colorData) {
            var viewModel = new ColorViewModel(colorData);
            viewModel.onDeleted.addHandler(this.onColorDeleted.bind(this));
            this.savedColors.push(viewModel);
        },

        onColorDeleted: function (colorViewModel) {
            this.savedColors.remove(colorViewModel);
        }

    });

}).call(this);