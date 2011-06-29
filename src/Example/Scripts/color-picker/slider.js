/// <reference path="../lib/knockout.js" />
/// <reference path="../lib/jquery.js" />
/// <reference path="../lib/jquery-ui.js" />

// Extend knockout with a custom binding handler.
// This displays a jQuery UI slider control which is bound to the
// value of an observable view model property.

ko.bindingHandlers['slider'] = {

    init: function (element, valueAccessor) {
        initialValue = ko.utils.unwrapObservable(valueAccessor());
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