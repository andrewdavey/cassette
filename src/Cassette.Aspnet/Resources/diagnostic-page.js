// @reference Cassette.Aspnet.Resources.jquery.js
// @reference Cassette.Aspnet.Resources.knockout.js

$(function () {
    ko.applyBindings(data);

    var defaultTab = "#about";
    var tabLinks = $("#tabs > ul > a");
    var tabContents = $("#tabs > div > div");

    var showCurrentTab = function () {
        var hash = window.location.hash;
        if (hash.length <= 1) {
            hash = defaultTab;
        }
        tabContents.hide();
        $(hash).show();
        $("a.active").removeClass("active");
        $("a[href=" + hash + "]").addClass("active");
    };
    $(window).bind("hashchange", showCurrentTab);
    showCurrentTab();

    if (!window.location.hash && window.history && window.history.replaceState) {
        history.replaceState(null, document.title, defaultTab);
    }

    $("#rebuild-cache").click(function (e) {
        e.preventDefault();

        var button = $(this);
        var text = button.text();
        button.attr("disabled", "disabled").text("Rebuilding...");

        $.post(window.location, { action: "rebuild-cache" }, function () {
            alert("Bundle cache rebuilt.");
            button.removeAttr("disabled").text(text);
        });
    });
});