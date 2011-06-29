/// <reference path="../app/namespace.js" />
(function () {

    function convertNumberToHexString(number) {
        return ('0' + number.toString(16)).substr(-2);
    }

    this.Example.helpers = {
        getColorHexString: function (red, green, blue) {
            return "#" +
                convertNumberToHexString(red) +
                convertNumberToHexString(green) +
                convertNumberToHexString(blue);
        },
        randomColor: function () {
            return {
                red: Math.floor(Math.random() * 255),
                green: Math.floor(Math.random() * 255),
                blue: Math.floor(Math.random() * 255)
            };
        }
    };

}).call(this);