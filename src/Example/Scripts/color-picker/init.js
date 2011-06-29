/// <reference path="../lib/jquery.js" />
/// <reference path="ColorPickerViewModel.js" />

$(function () {
    ko.applyBindings(
        // The view page must assign a global viewData property
        // for us to use up here.
        new Example.ColorPickerViewModel(window.pageViewData)
    );
});