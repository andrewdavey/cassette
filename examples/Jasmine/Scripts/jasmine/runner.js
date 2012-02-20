/// <reference path="jasmine.js"/>
/// <reference path="jasmine-html.js"/>

(function () {
    var jasmineEnv = jasmine.getEnv();
    jasmineEnv.updateInterval = 1000;

    var trivialReporter = new jasmine.TrivialReporter();

    jasmineEnv.addReporter(trivialReporter);

    jasmineEnv.specFilter = function (spec) {
        return trivialReporter.specFilter(spec);
    };

    window.onload = function () {
        jasmineEnv.execute();
    };
}());