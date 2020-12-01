var SentinelVue = (function () {

    var init = function () {
        let scripts = document.getElementsByTagName("script");
        let fullUrl = scripts[scripts.length - 1].src;
        let replaceIndex = fullUrl.indexOf('js/');
        let baseUrl = fullUrl.substring(0, replaceIndex);

        Vue.config.errorHandler = function (err, vm, info) {
            var errorObj = { stack: err.stack };
            Sentinel.handleError(err.message, '', 0, 0, errorObj, info, baseUrl);
        }
    }

    return {
        init: init
    }
})();

SentinelVue.init();