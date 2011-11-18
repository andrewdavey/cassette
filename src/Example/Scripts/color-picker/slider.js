/// <reference path="../jquery/jquery.js" />
/// <reference path="../lib/knockout.js" />
/// <reference path="../lib/jquery-ui.js" />

(function () {

    // Extend knockout with a custom binding handler.
    // This displays a jQuery UI slider control which is bound to the
    // value of an observable view model property.

    this.ko.bindingHandlers['slider'] = {

        init: function (element, valueAccessor) {
            var initialValue = ko.utils.unwrapObservable(valueAccessor());
            // Create the jQuery UI slider.
            $(element).slider({
                min: 0,
                max: 255,
                value: initialValue,
                // On slide, update the view model property.
                slide: function (event, ui) {
                    valueAccessor()(ui.value);
                }
            });
        },

        update: function (element, valueAccessor) {
            // When the view model property changes, update the slider.
            var value = ko.utils.unwrapObservable(valueAccessor());
            $(element).slider('option', 'value', value);
        }

    };

}).call(this);