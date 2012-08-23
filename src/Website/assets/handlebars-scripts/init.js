var i18n = {
    translate: function () {
        return 'Bonjour';
    }
};

$(function(){
    
    var i18n = {
        translate: function (s) {
            var obj = {
                'Name': 'Nom',
                'The': 'Le'
            };
            return obj[s] || s;
        }
    };
    
    console.log(JST['test'].render({
        author: {
            first: 'Jean',
            last: 'Luc'
        },
        i18n: function() {
            return function (s) {
                return (i18n && typeof i18n.translate === 'function') ? i18n.translate(s) : s;
            };
        }
    }));

});