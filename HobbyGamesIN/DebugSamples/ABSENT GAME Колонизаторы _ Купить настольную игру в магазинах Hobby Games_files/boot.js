requirejs.config({
    baseUrl: '/assets/js/checkout/app/',
    waitSeconds: 0,
    paths: {
        'backbone': '../lib/backbone-1.1.2',
        // 'jquery': 'https://code.jquery.com/jquery-2.2.4.min',
        'jquery': '../lib/jquery-2.2.4.min',
        'underscore': '../lib/underscore/underscore-min',
        'sprintf': '../lib/underscore/sprintf',
        'require': '../lib/requirejs/require',
        'text': '../lib/requirejs/text',
        'domReady': '../lib/requirejs/domReady',
        'i18n': '../lib/requirejs/i18n',
        'jquery.autocomplete': '../lib/jquery/jquery.autocomplete',
        'jquery.suggestions': '../lib/jquery/jquery.suggestions',
        'jquery.cookie': '../lib/jquery/jquery.cookie',
        //'jquery.mask': '../lib/jquery/jquery.mask',
        'jquery.mask': '../lib/jquery/jqmask.min',
        'selectFx': '../lib/select/selectFx',
        'classie': '../lib/classie',
        'livequery': '../lib/jquery.livequery',
        'slimscroll': '../lib/jquery.slimscroll',
        'input.number': '../lib/input.number',
        'fancybox': '../lib/fancybox/jquery.fancybox',
        'bxSlider': '../lib/bxslider/jquery.bxslider',
        'jqueryui': '../lib/jquery-ui',
        'trunk': '../lib/trunk8',
        'rating': '../lib/stars',
        'owlCarousel': '../lib/owl/owl.carousel.min',
        'ofi': '../lib/ofi.min',
        'spoiler': '../lib/jquery.spoiler.min',
        'scrollspy': '../lib/scrollspy',
        'vex': '../lib/vex.combined',
        'noUiSlider': '../lib/nouislider.min',
        'snapper': '../lib/snap',
        'rippleEffect': '../lib/ripple-effect',
        'lazytube': '../lib/jquery.lazytube',
        'lightgallery': '../lib/lightgallery/js/lightgallery',
        'lg-thumbnail': '../lib/lightgallery/js/lg-thumbnail',
        'sailplay': '../lib/sailplay.magic.min',
        'YMaps': 'https://api-maps.yandex.ru/2.1/?lang=ru-RU'

        // application : 'Application',
        // router : 'Router'
    },
    shim: {
        backbone: {
            deps: ['jquery', 'underscore'],
            exports: 'Backbone'
        },
        underscore: {
            exports: '_'
        },
        selectFx: {
            deps: ['jquery', 'classie', 'livequery', 'slimscroll'],
            exports: 'SelectFx'
        }
    }
});

if (window.config.version) {
    requirejs.config({
        urlArgs: 'v='+window.config.version
    });
}

requirejs.onError = function (err) {
    console.log(err.requireType);

    if (err.requireType === 'timeout') {
        console.log('modules: ' + err.requireModules);
    }

    window.requirejsError = err;
    throw err;
};

var startModule = window.config.serverEnv === 'development'
    ? '../index'
    : '../build-js/Core';

define([startModule]);
