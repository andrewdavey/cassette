/// <reference path="~/jquery"/>
$(function () {
    var tweets = $(".tweets");
    var tweetIndex = 0;
    setInterval(function () {
        $(tweets[tweetIndex]).fadeOut(2000);
        tweetIndex++;
        if (tweetIndex === tweets.length) {
            tweetIndex = 0;
        }
        $(tweets[tweetIndex]).fadeIn(2000);
    }, 5000);

});