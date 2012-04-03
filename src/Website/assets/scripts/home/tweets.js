/// <reference path="~/assets/scripts/jquery/jquery.js"/>
/// <reference path="../../vsdoc/something-vsdoc.js" /> -ignore


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
    }, 8000);

});