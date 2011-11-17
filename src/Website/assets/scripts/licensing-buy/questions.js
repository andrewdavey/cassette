/// <reference path="../jquery/jquery.js"/>

$(function () {

    // Toggle the answers when questions are clicked.
    $(".helpanswer").hide();
    $(".helpquestion").click(function () {
        var question = $(this);
        var answer = question.parent("dt").next(".helpanswer");
        answer.toggle();
        question.toggleClass("open");
        return false;
    });

});