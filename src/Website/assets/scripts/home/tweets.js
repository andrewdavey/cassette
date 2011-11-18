/// <reference path="~/assets/scripts/jquery/jquery.js"/>
$(function () {
    setInterval(function () {
        $(".tweets:visible").fadeOut(2000);
        $(".tweets:not(:visible)").fadeIn(2000);
    }, 7500);
});