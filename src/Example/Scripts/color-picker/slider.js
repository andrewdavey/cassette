/// <reference path="../lib/knockout.js" />
/// <reference path="../lib/jquery.js" />
/// <reference path="../lib/jquery-ui.js" />

ko.bindingHandlers['slider'] = {

    init: function (element, valueAccessor) {
        $(element).slider({
            min: 0,
            max: 255,
            value: ko.utils.unwrapObservable(valueAccessor()),
            slide: function (event, ui) {
                valueAccessor()(ui.value);
            }
        });
    },

    update: function (element, valueAccessor) {
        var value = ko.utils.unwrapObservable(valueAccessor());
        $(element).slider('option', 'value', value);
    }

};