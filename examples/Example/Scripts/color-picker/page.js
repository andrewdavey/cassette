/// <reference path="../jquery/jquery.js" />
/// <reference path="../lib/knockout.js" />
/// <reference path="../app/namespace.js" />
/// <reference path="ColorPickerViewModel.js" />

$(function () {
    // Entry point of page code.
    // Initialize KnockoutJS to data bind a view model to the UI.
    ko.applyBindings(
        // The view page must assign a global viewData property
        // for us to use here.
        new Example.ColorPickerViewModel(window.pageViewData)
    );
});