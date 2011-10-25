/// <reference path="~/jquery"/>

$(function () {

    // Dynamically update the total price when the number of servers is changed.
    var form = $("#buy-form"),
        serverNumber = form.find("#servernumber"),
        totalCost = form.find("#totalcost"),
        licenseHolder = $("#licenseholder");

    serverNumber.change(function () {
        var value = parseInt(serverNumber.val(), 10);
        totalCost.text('$' + value);
    });

    form.submit(function () {
        if ($.trim(licenseHolder.val()) === "") {
            alert("Please enter name of company or person to be license-holder.");
            setTimeout(function () { licenseHolder.focus(); }, 1);
            return false;
        }
    });
    // For noscript users the form is hidden by default.
    // So show it now.
    form.show();
});