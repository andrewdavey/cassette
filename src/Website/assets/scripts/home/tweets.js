/// <reference path="~/jquery"/>
$(function () {
    setInterval(function () {
        $(".tweets:visible").fadeOut(2000);
        $(".tweets:not(:visible)").fadeIn(2000);
    }, 9000);
});