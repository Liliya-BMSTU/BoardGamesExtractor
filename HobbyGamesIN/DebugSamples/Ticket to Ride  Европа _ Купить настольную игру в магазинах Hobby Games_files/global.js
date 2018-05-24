// console-заглушки
if (!window.console) window.console = {};
if (!window.console.log) window.console.log = function () {};

(function(window) {
    var SELECTORS = {
        GLOBAL_LOADER: '.global-loader'
    };

    var Application = function() {};
    Application.prototype = {

        addStyle: function (href, callback) {
            if (!$('head link[href="' + href + '"]').length) {

                var successFn = function() {
                        if (callback && $.isFunction(callback)) callback.apply(this, arguments);
                        //console.log(href + ' added');
                    },
                    link = document.createElement('link');

                link.type = 'text/css';
                link.href = href;
                link.rel = 'stylesheet';
                document.head.appendChild(link);

                link.onload = link.onerror = function() {
                    if (!this.executed) {
                        this.executed = true;
                        successFn();
                    }
                };

                link.onreadystatechange = function() {
                    var self = this;
                    if (this.readyState == 'complete' || this.readyState == 'loaded') {
                        setTimeout(function() { self.onload() }, 0);
                    }
                };

            }
        },

        addScript: function (src, callback) {
            if (!$('body script[src="' + src + '"]').length) {
                var successFn = function() {
                        if (callback && $.isFunction(callback)) callback.apply(this, arguments);
                        //console.log(src + ' added');
                    },
                    script = document.createElement('script');

                script.type = 'text/javascript';
                script.src = src;
                document.body.appendChild(script);

                script.onload = script.onerror = function() {
                    if (!this.executed) {
                        this.executed = true;
                        successFn();
                    }
                };

                script.onreadystatechange = function() {
                    var self = this;
                    if (this.readyState == 'complete' || this.readyState == 'loaded') {
                        setTimeout(function() { self.onload() }, 0);
                    }
                };

            }
        },

        showGlobalLoader: function() {
            $(SELECTORS.GLOBAL_LOADER).each(function () {
                if ($(this).parent().is(':visible')) {
                    $(this).show();
                    return false;
                }
            });
        },

        hideGlobalLoader: function() {
            $(SELECTORS.GLOBAL_LOADER).hide();
        },

        flashMessage: function ($object, text, cb) {
            if ($object.length) {
                $object.text(text)
                    .fadeIn(200)
                    .delay(3000)
                    .queue(function(n) {
                        var $this = $(this);
                        $this.fadeOut(200);
                        $this.text('');
                        n();
                        cb && cb();
                    });
            }
        }
    };

    window.APP = new Application();
})(window);