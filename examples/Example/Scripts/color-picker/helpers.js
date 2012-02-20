/// <reference path="../app/namespace.js" />

(function () {

    // Private functions. Only used by this file; not exposed globally.
    function convertNumberToHexString(number) {
        return ('0' + number.toString(16)).substr(-2);
    }

    function randomByte() {
        // Random number between 0 and 255.
        return Math.floor(Math.random() * 256);
    }

    // Public helper functions
    this.Example.helpers = {

        getColorHexString: function (red, green, blue) {
            return "#" +
                convertNumberToHexString(red) +
                convertNumberToHexString(green) +
                convertNumberToHexString(blue);
        },

        randomColor: function () {
            return {
                red: randomByte(),
                green: randomByte(),
                blue: randomByte()
            };
        }

    };

}).call(this);