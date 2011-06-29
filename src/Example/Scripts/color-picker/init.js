/// <reference path="../lib/jquery.js" />
/// <reference path="ColorPickerViewModel.js" />
/// <reference path="slider.js" />

$(function () {
    ko.applyBindings(new Example.ColorPickerViewModel(window.viewData));
});