var Sentinel = (function () {

    var applicationName = 'UNKNOWN';

    var internalInit = function () {
        let scripts = document.getElementsByTagName("script");
        let fullUrl = scripts[scripts.length - 1].src;
        let replaceIndex = fullUrl.indexOf('js/');
        let baseUrl = fullUrl.substring(0, replaceIndex);
        window.onerror = function (message, source, lineno, colno, error) {
            handleError(message, source, lineno, colno, error, null, baseUrl);
            return true;
        };
    }

    var init = function (appName) {
        applicationName = appName;
    }

    var handleError = function (message, source, lineno, colno, error, vueinfo, baseUrl) {
        var errorData = {
            applicationName: applicationName,
            message: message,
            source: source,
            lineno: lineno,
            colno: colno,
            stack: error ? error.stack : '',
            vueinfo: vueinfo,
            agent: window.navigator.userAgent,
            platform: window.navigator.platform
        };
        var xhr = new XMLHttpRequest();
        var url = baseUrl + 'Error/PostError';
        xhr.open('POST', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.send(JSON.stringify(errorData));
    }

    return {
        internalInit: internalInit,
        init: init,
        handleError: handleError
    }
})();

Sentinel.internalInit();