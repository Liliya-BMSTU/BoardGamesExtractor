define(['jquery', 'underscore', 'noUiSlider'], function($, _, noUiSlider) {
    var SELECTORS = {
        KEYWORD                 : '.header-container input[name="keyword"]',
        PRICE_FROM              : '.filter input[name="price_from"]',
        PRICE_TO                : '.filter input[name="price_to"]',
        PLAYERS_FROM            : '.filter input[name="players_from"]',
        EASY_DIFFICULT_FROM     : '.filter input[name="easy_difficult_from"]',
        EASY_DIFFICULT_TO       : '.filter input[name="easy_difficult_to"]',
        DO_THINK_TO             : '.filter input[name="do_think_to"]',
        DO_THINK_FROM           : '.filter input[name="do_think_from"]',
        CALC_CHANCE_TO          : '.filter input[name="calc_chance_to"]',
        CALC_CHANCE_FROM        : '.filter input[name="calc_chance_from"]',
        SPECIAL                 : '.filter input[name="special"]',
        HIT                     : '.filter input[name="hit"]',
        NEW                     : '.filter input[name="new"]',
        PROMO                   : '.filter input[name="promo"]',
        PREORDER                : '.filter input[name="preorder"]',
        SORT                    : '.filter select[name="sort"]',

        SEARCH_FORM             : '.filter',
        SEARCH_FORM_FIELDS      : '.filter input[type="text"]',
        //SEARCH_ARROW          : '#panelPickup .arrow-btn-left',
        PARENT                  : '.category-left',
        BALLOON                 : '.category-left .balloon',
        MOBILE_BALLOON          : '.category-left__result__product',
        CATEGORIES              : '.category-left__categories',
        CATEGORY                : '.category-left__categories input[name="category"]',
        AGE                     : '.filter input[name="age"]',
        AGE_CHECKED             : '.filter input[name="age"]:checked',
        TIME                    : '.filter input[name="time"]',
        TIME_CHECKED            : '.filter input[name="time"]:checked',
        MANUFACTURERS           : '.filter input[name="manufacturers"]',
        MANUFACTURERS_CHECKED   : '.filter input[name="manufacturers"]:checked',
        EASY_DIFFICULT_SLIDER   : '.filter #easy_difficult-range-slider',
        DO_THINK_SLIDER         : '.filter #do_think-range-slider',
        CALC_CHANCE_SLIDER      : '.filter #calc_chance-range-slider',

        RESET_FILTER            : '.filter .reset-filter'
    };
    var MIN_MAX_VALUES = {
        PRICE           : [0, window.config.maxProductPrice],
        PLAYERS         : [0, 15],
        EASY_DIFFICULT  : [1, 5],
        DO_THINK        : [1, 5],
        CALC_CHANCE     : [1, 5]
    };

    var defaults = {
        url: '/catalog/search',
        urlBalloon: '/?route=lib/feed/balloon'
    };

    var LeftSearch = function(options) {
        this.options = $.extend({}, defaults, options);

        this.init();
        this.delegateEvents();
    };
    LeftSearch.prototype = {
        contentSearch: function() {
            var url                  = this.options.url + '?',
                data                 = {},
                $keyword             = $(SELECTORS.KEYWORD).eq(1).val() ? $(SELECTORS.KEYWORD).eq(1).val() : $(SELECTORS.KEYWORD).eq(0).val(),
                $price_from          = $(SELECTORS.PRICE_FROM).val() >> 0,
                $price_to            = $(SELECTORS.PRICE_TO).val() >> 0,
                $special             = $(SELECTORS.SPECIAL),
                $hit                 = $(SELECTORS.HIT),
                $new                 = $(SELECTORS.NEW),
                $promo               = $(SELECTORS.PROMO),
                $preorder            = $(SELECTORS.PREORDER),
                $players_from        = $(SELECTORS.PLAYERS_FROM).val() >> 0,
                $easy_difficult_from = $(SELECTORS.EASY_DIFFICULT_FROM).val() >> 0,
                $easy_difficult_to   = $(SELECTORS.EASY_DIFFICULT_TO).val() >> 0,
                $do_think_from       = $(SELECTORS.DO_THINK_FROM).val() >> 0,
                $do_think_to         = $(SELECTORS.DO_THINK_TO).val() >> 0,
                $calc_chance_from    = $(SELECTORS.CALC_CHANCE_FROM).val() >> 0,
                $calc_chance_to      = $(SELECTORS.CALC_CHANCE_TO).val() >> 0,
                $sort                = $(SELECTORS.SORT).val(),
                $category            = $(SELECTORS.CATEGORY).val(),
                $age                 = $(SELECTORS.AGE),
                $time                = $(SELECTORS.TIME),
                $manufacturers       = $(SELECTORS.MANUFACTURERS);

            if ($keyword) {
                data.keyword = $keyword;
            }

            if ($special.is(':checked')) {
                data.special = 1;
            }

            if ($hit.is(':checked')) {
                data.hit = 1;
            }

            if ($new.is(':checked')) {
                data.new = 1;
            }

            if ($promo.is(':checked')) {
                data.promo = 1;
            }

            if ($preorder.is(':checked')) {
                data.preorder = 1;
            }

            var age = [];
            $age.each(function(){
                if ($(this).is(':checked')) {
                    age.push($(this).val());
                }
            });

            var time = [];
            $time.each(function(){
                if ($(this).is(':checked')) {
                    time.push($(this).val());
                }
            });

            var manufacturers = [];
            $manufacturers.each(function(){
                if ($(this).is(':checked')) {
                    manufacturers.push($(this).val());
                }
            });

            if (age.length) {
                data.age = age;
            }

            if (time.length) {
                data.time = time;
            }

            if ($category) {
                data.category_id = $category;
            }

            if (manufacturers.length) {
                data.manufacturers = manufacturers.join(',');
            }

            if (MIN_MAX_VALUES.PRICE[0] !== $price_from || MIN_MAX_VALUES.PRICE[1] !== $price_to) {
                if ($price_from) {
                    data.price_from = $price_from;
                }
                if ($price_to && $price_to < window.config.maxProductPrice) {
                    data.price_to = $price_to;
                }
            }

            if (MIN_MAX_VALUES.PLAYERS[0] !== $players_from) {
                if ($players_from) {
                    data.players_from = $players_from;
                }
            }

            if (MIN_MAX_VALUES.EASY_DIFFICULT[0] !== $easy_difficult_from || MIN_MAX_VALUES.EASY_DIFFICULT[1] !== $easy_difficult_to) {
                if ($easy_difficult_from) {
                    data.easy_difficult_from = $easy_difficult_from;
                }
                if ($easy_difficult_to) {
                    data.easy_difficult_to = $easy_difficult_to;
                }
            }

            if (MIN_MAX_VALUES.DO_THINK[0] !== $do_think_from || MIN_MAX_VALUES.DO_THINK[1] !== $do_think_to) {
                if ($do_think_from) {
                    data.do_think_from = $do_think_from;
                }
                if ($do_think_to) {
                    data.do_think_to = $do_think_to;
                }
            }

            if (MIN_MAX_VALUES.CALC_CHANCE[0] !== $calc_chance_from || MIN_MAX_VALUES.CALC_CHANCE[1] !== $calc_chance_to) {
                if ($calc_chance_from) {
                    data.calc_chance_from = $calc_chance_from;
                }
                if ($calc_chance_to) {
                    data.calc_chance_to = $calc_chance_to;
                }
            }

            if ($sort && $sort !== 'rating') {
                var split = $sort.split(' ');
                data.sort = split[0];
                if (split.length > 1) {
                    data.order = split[1];
                }
            }

            document.location = url + $.param(data).replace(/%2C/g, ',');
        },
        showSearchBalloon: function($input) {
            var that = this,
                $div = $input.parent('div'),
                $balloon = $(SELECTORS.BALLOON),
                $mobile_balloon = $(SELECTORS.MOBILE_BALLOON),
                offset_center = ($div.height() - $balloon.height())/2,
                top = $div.offset().top - $(SELECTORS.PARENT).offset().top + offset_center;

            $.ajax({
                url : that.options.urlBalloon,
                data : that.searchOptions,
                type: 'get',
                dataType : 'json',
                success : function(json) {
                    if ('total' in json) {
                        $balloon.find('span').text(json.total);
                        $mobile_balloon.find('span').text(json.total);

                        if (json.total) {
                            $balloon.find('.do-search').show();
                            $balloon.find('.reset-filter').hide();
                        } else {
                            $balloon.find('.do-search').hide();
                            $balloon.find('.reset-filter').show();
                        }

                        $balloon.css({top: top}).addClass('active');
                    }
                }
            });
        },
        hideSearchBalloon: function() {
            $(SELECTORS.BALLOON).removeClass('active');
        },
        setSearchOption: function ($input) {
            var $optionKey = $input.attr('name'),
                val = $input.attr('type') === 'checkbox' ? $input.is(':checked')>>0 : $input.val(),
                specialValues = [
                    'price_from', 'price_to',
                    'players_from', 'players_to',
                    'easy_difficult_from', 'easy_difficult_to',
                    'do_think_from', 'do_think_to',
                    'calc_chance_from', 'calc_chance_to'
                ];

            if (val && val != 0) {
                this.searchOptions[$optionKey] = val;
            } else {
                delete this.searchOptions[$optionKey];
            }

            // для min-max полей не передаем ничего, если выбран весь диапазон
            if (specialValues.indexOf($optionKey) !== -1) {
                var $price_from         = $(SELECTORS.PRICE_FROM).val() >> 0,
                    $price_to           = $(SELECTORS.PRICE_TO).val() >> 0,
                    $players_from       = $(SELECTORS.PLAYERS_FROM).val() >> 0,
                    $easy_difficult_from = $(SELECTORS.EASY_DIFFICULT_FROM).val() >> 0,
                    $easy_difficult_to   = $(SELECTORS.EASY_DIFFICULT_TO).val() >> 0,
                    $do_think_from       = $(SELECTORS.DO_THINK_FROM).val() >> 0,
                    $do_think_to         = $(SELECTORS.DO_THINK_TO).val() >> 0,
                    $calc_chance_from    = $(SELECTORS.CALC_CHANCE_FROM).val() >> 0,
                    $calc_chance_to      = $(SELECTORS.CALC_CHANCE_TO).val() >> 0;

                switch ($optionKey) {
                    case 'price_from':
                    case 'price_to':
                        if (MIN_MAX_VALUES.PRICE[0] === $price_from && MIN_MAX_VALUES.PRICE[1] === $price_to) {
                            delete this.searchOptions[$optionKey];
                        }
                        break;
                    case 'players_from':
                        if (MIN_MAX_VALUES.PLAYERS[0] === $players_from) {
                            delete this.searchOptions[$optionKey];
                        }
                        break;
                    case 'easy_difficult_from':
                    case 'easy_difficult_to':
                        if (MIN_MAX_VALUES.EASY_DIFFICULT[0] === $easy_difficult_from && MIN_MAX_VALUES.EASY_DIFFICULT[1] === $easy_difficult_to) {
                            delete this.searchOptions[$optionKey];
                        }
                        break;
                    case 'do_think_from':
                    case 'do_think_to':
                        if (MIN_MAX_VALUES.DO_THINK[0] === $do_think_from && MIN_MAX_VALUES.DO_THINK[1] === $do_think_to) {
                            delete this.searchOptions[$optionKey];
                        }
                        break;
                    case 'calc_chance_from':
                    case 'calc_chance_to':
                        if (MIN_MAX_VALUES.CALC_CHANCE[0] === $calc_chance_from && MIN_MAX_VALUES.CALC_CHANCE[1] === $calc_chance_to) {
                            delete this.searchOptions[$optionKey];
                        }
                        break;

                }
            }
        },
        setSearchAge: function() {
            delete this.searchOptions.age;
            var age = $(SELECTORS.AGE_CHECKED).map(function(num,input){return input.value}).get();
            if (age) {
                this.searchOptions.age = age;
            }
        },
        setSearchTime: function() {
            delete this.searchOptions.time;
            var time = $(SELECTORS.TIME_CHECKED).map(function(num,input){return input.value}).get();
            if (time) {
                this.searchOptions.time = time;
            }
        },
        setSearchManufacturers: function() {
            delete this.searchOptions.manufacturers;
            var manufacturers = $(SELECTORS.MANUFACTURERS_CHECKED).map(function(num,input){return input.value}).get().join(',');
            if (manufacturers) {
                this.searchOptions.manufacturers = manufacturers;
            }
        },

        checkResetFilter: function() {
            var $easy_difficult_from = $(SELECTORS.EASY_DIFFICULT_FROM).val() >> 0,
                $easy_difficult_to   = $(SELECTORS.EASY_DIFFICULT_TO).val() >> 0,
                $do_think_from       = $(SELECTORS.DO_THINK_FROM).val() >> 0,
                $do_think_to         = $(SELECTORS.DO_THINK_TO).val() >> 0,
                $calc_chance_from    = $(SELECTORS.CALC_CHANCE_FROM).val() >> 0,
                $calc_chance_to      = $(SELECTORS.CALC_CHANCE_TO).val() >> 0,
                $special             = $(SELECTORS.SPECIAL).is(':checked') >> 0,
                $price_from          = $(SELECTORS.PRICE_FROM).val() >> 0,
                $price_to            = $(SELECTORS.PRICE_TO).val() >> 0,
                $players_from        = $(SELECTORS.PLAYERS_FROM).val() >> 0,
                $manufacturers       = $(SELECTORS.MANUFACTURERS_CHECKED),
                $age                 = $(SELECTORS.AGE_CHECKED),
                $time                = $(SELECTORS.TIME_CHECKED),

            isDefaultParams = $easy_difficult_from === this.defaultOptions.easy_difficult_from
                               && $easy_difficult_to === this.defaultOptions.easy_difficult_to
                               && $do_think_from === this.defaultOptions.do_think_from
                               && $do_think_to === this.defaultOptions.do_think_to
                               && $calc_chance_from === this.defaultOptions.calc_chance_from
                               && $calc_chance_to === this.defaultOptions.calc_chance_to
                               && $price_from === this.defaultOptions.price_from
                               && $price_to === this.defaultOptions.price_to
                               && $players_from === this.defaultOptions.players_from
                               && $special>>0 === this.defaultOptions.special>>0
                               && !$manufacturers.length
                               && !$age.length
                               && !$time.length;

            $(SELECTORS.RESET_FILTER).toggleClass('inactive', isDefaultParams);
        },

        onChangeSearchOption: function(e) {
            console.log('change search options');
            var $input = $(e.currentTarget);
            this.hideSearchBalloon();
            this.setSearchOption($input);
            this.checkResetFilter();
            this.showSearchBalloon($input);
        },

        onChangeSearchAge: function (e) {
            this.hideSearchBalloon();
            this.setSearchAge();
            this.checkResetFilter();
            this.showSearchBalloon($(e.currentTarget));
        },

        onChangeSearchTime: function (e) {
            this.hideSearchBalloon();
            this.setSearchTime();
            this.checkResetFilter();
            this.showSearchBalloon($(e.currentTarget));
        },

        onChangeSearchManufacturers: function (e) {
            this.hideSearchBalloon();
            this.setSearchManufacturers();
            this.checkResetFilter();
            this.showSearchBalloon($(e.currentTarget));
        },

        init: function() {
            var that = this;
            this.searchOptions = {};
            this.searchFieldsSelector = [
                SELECTORS.KEYWORD,
                SELECTORS.PRICE_FROM,
                SELECTORS.PRICE_TO,
                SELECTORS.PLAYERS_FROM,
                SELECTORS.EASY_DIFFICULT_FROM,
                SELECTORS.EASY_DIFFICULT_TO,
                SELECTORS.DO_THINK_FROM,
                SELECTORS.DO_THINK_TO,
                SELECTORS.CALC_CHANCE_FROM,
                SELECTORS.CALC_CHANCE_TO,
                SELECTORS.SPECIAL
            ].join(', ');
            $(this.searchFieldsSelector).each(function(num, input) {
                that.setSearchOption($(input));
            });

            this.setSearchAge();
            this.setSearchTime();
            this.setSearchManufacturers();

            noUiSlider
                .create($(SELECTORS.EASY_DIFFICULT_SLIDER).get(0), {
                    start: [parseInt($(SELECTORS.EASY_DIFFICULT_FROM).val()), parseInt($(SELECTORS.EASY_DIFFICULT_TO).val())],
                    step: 1,
                    tooltips: [false, false],
                    connect: true,
                    range: {
                        min: MIN_MAX_VALUES.EASY_DIFFICULT[0],
                        max: MIN_MAX_VALUES.EASY_DIFFICULT[1]
                    },
                    format: {
                        to: function ( value ) {
                            return parseInt(value);
                        },
                        from: function (value) {
                            return value;
                        }
                    }
                })
                .on('change', function(values) {
                    $(SELECTORS.EASY_DIFFICULT_FROM).val(values[0]);
                    $(SELECTORS.EASY_DIFFICULT_TO).val(values[1]);

                    that.setSearchOption($(SELECTORS.EASY_DIFFICULT_FROM));
                    that.setSearchOption($(SELECTORS.EASY_DIFFICULT_TO));
                    that.checkResetFilter();

                    that.showSearchBalloon($(SELECTORS.EASY_DIFFICULT_SLIDER));
                });

            noUiSlider
                .create($(SELECTORS.DO_THINK_SLIDER).get(0), {
                    start: [parseInt($(SELECTORS.DO_THINK_FROM).val()), parseInt($(SELECTORS.DO_THINK_TO).val())],
                    step: 1,
                    tooltips: [false, false],
                    connect: true,
                    range: {
                        min: MIN_MAX_VALUES.DO_THINK[0],
                        max: MIN_MAX_VALUES.DO_THINK[1]
                    },
                    format: {
                        to: function ( value ) {
                            return parseInt(value);
                        },
                        from: function (value) {
                            return value;
                        }
                    }
                })
                .on('change', function(values) {
                    $(SELECTORS.DO_THINK_FROM).val(values[0]);
                    $(SELECTORS.DO_THINK_TO).val(values[1]);

                    that.setSearchOption($(SELECTORS.DO_THINK_FROM));
                    that.setSearchOption($(SELECTORS.DO_THINK_TO));
                    that.checkResetFilter();

                    that.showSearchBalloon($(SELECTORS.DO_THINK_SLIDER));
                });

            noUiSlider
                .create($(SELECTORS.CALC_CHANCE_SLIDER).get(0), {
                    start: [parseInt($(SELECTORS.CALC_CHANCE_FROM).val()), parseInt($(SELECTORS.CALC_CHANCE_TO).val())],
                    step: 1,
                    tooltips: [false, false],
                    connect: true,
                    range: {
                        min: MIN_MAX_VALUES.CALC_CHANCE[0],
                        max: MIN_MAX_VALUES.CALC_CHANCE[1]
                    },
                    format: {
                        to: function ( value ) {
                            return parseInt(value);
                        },
                        from: function (value) {
                            return value;
                        }
                    }
                })
                .on('change', function(values) {
                    $(SELECTORS.CALC_CHANCE_FROM).val(values[0]);
                    $(SELECTORS.CALC_CHANCE_TO).val(values[1]);

                    that.setSearchOption($(SELECTORS.CALC_CHANCE_FROM));
                    that.setSearchOption($(SELECTORS.CALC_CHANCE_TO));
                    that.checkResetFilter();

                    that.showSearchBalloon($(SELECTORS.CALC_CHANCE_SLIDER));
                });


            this.searchOptions = _.defaults(this.searchOptions, window.searchState);

            this.defaultOptions = $.extend({}, window.searchState, {
                do_think_from: MIN_MAX_VALUES.DO_THINK[0],
                do_think_to: MIN_MAX_VALUES.DO_THINK[1],
                easy_difficult_from: MIN_MAX_VALUES.EASY_DIFFICULT[0],
                easy_difficult_to: MIN_MAX_VALUES.EASY_DIFFICULT[1],
                calc_chance_from: MIN_MAX_VALUES.CALC_CHANCE[0],
                calc_chance_to: MIN_MAX_VALUES.CALC_CHANCE[1],
                price_from: MIN_MAX_VALUES.PRICE[0],
                price_to: MIN_MAX_VALUES.PRICE[1],
                players_from: MIN_MAX_VALUES.PLAYERS[0],
                special: 0,
                manufacturers: '',
                age: [],
                time: []
            });
        },

        delegateEvents: function() {
            var that = this;
            $(document)
                .on('click', '.do-search', function(e) {
                    e.preventDefault();
                    that.contentSearch();
                })
                .on('click', SELECTORS.RESET_FILTER, function(e) {
                    e.preventDefault();

                    that.searchOptions = $.extend({}, that.defaultOptions);

                    $(SELECTORS.EASY_DIFFICULT_FROM).val(that.defaultOptions.easy_difficult_from);
                    $(SELECTORS.EASY_DIFFICULT_TO).val(that.defaultOptions.easy_difficult_to);
                    $(SELECTORS.DO_THINK_FROM).val(that.defaultOptions.do_think_from);
                    $(SELECTORS.DO_THINK_TO).val(that.defaultOptions.do_think_to);
                    $(SELECTORS.CALC_CHANCE_FROM).val(that.defaultOptions.calc_chance_from);
                    $(SELECTORS.CALC_CHANCE_TO).val(that.defaultOptions.calc_chance_to);
                    $(SELECTORS.PRICE_FROM).val(that.defaultOptions.price_from);
                    $(SELECTORS.PRICE_TO).val(that.defaultOptions.price_to);
                    $(SELECTORS.PLAYERS_FROM).val(that.defaultOptions.players_from);

                    $(SELECTORS.SPECIAL).attr('checked', !!that.defaultOptions.special);

                    $(SELECTORS.MANUFACTURERS_CHECKED).each(function(i, el) {$(el).removeAttr('checked');});
                    $(SELECTORS.AGE_CHECKED).each(function(i, el) {$(el).removeAttr('checked');});
                    $(SELECTORS.TIME_CHECKED).each(function(i, el) {$(el).removeAttr('checked');});

                    that.setSearchOption($(SELECTORS.EASY_DIFFICULT_FROM));
                    that.setSearchOption($(SELECTORS.EASY_DIFFICULT_TO));
                    that.setSearchOption($(SELECTORS.DO_THINK_FROM));
                    that.setSearchOption($(SELECTORS.DO_THINK_TO));
                    that.setSearchOption($(SELECTORS.CALC_CHANCE_FROM));
                    that.setSearchOption($(SELECTORS.CALC_CHANCE_TO));
                    that.setSearchOption($(SELECTORS.SPECIAL));

                    $(SELECTORS.EASY_DIFFICULT_SLIDER).get(0).noUiSlider.set([that.defaultOptions.easy_difficult_from, that.defaultOptions.easy_difficult_to]);
                    $(SELECTORS.DO_THINK_SLIDER).get(0).noUiSlider.set([that.defaultOptions.do_think_from, that.defaultOptions.do_think_to]);
                    $(SELECTORS.CALC_CHANCE_SLIDER).get(0).noUiSlider.set([that.defaultOptions.calc_chance_from, that.defaultOptions.calc_chance_to]);

                    // that.showSearchBalloon($(SELECTORS.RESET_FILTER));
                    that.contentSearch();

                    $(this).addClass('inactive');
                })
                .on('keyup', this.searchFieldsSelector, function(e){
                    var $target = $(e.currentTarget);
                    if (!e.ctrlKey && $target.attr('type') !== 'number') {
                        that.onChangeSearchOption(e);
                    }
                })
                .on('input', this.searchFieldsSelector, function(e){
                    var $target = $(e.currentTarget);
                    if ($target.attr('type') === 'number') {
                        that.onChangeSearchOption(e);
                    }
                })
                .on('change', this.searchFieldsSelector, function(e){
                    var $target = $(e.currentTarget);
                    if ($target.attr('type') === 'checkbox' || $target.prop('tagName') === 'SELECT') {
                        that.onChangeSearchOption(e);
                    }
                })
                .on('keyup', SELECTORS.KEYWORD, function(e) {
                    var emptyKeyword = !$(e.target).val();

                    if (emptyKeyword) {
                        $(SELECTORS.SEARCH_FORM).find('.sortBy').val('rating').trigger('change', false);
                    }
                })
                .on('change', SELECTORS.AGE, function(e){
                    that.onChangeSearchAge(e);
                })
                .on('change', SELECTORS.TIME, function(e){
                    that.onChangeSearchTime(e);
                })
                .on('change', SELECTORS.MANUFACTURERS, function(e){
                    that.onChangeSearchManufacturers(e);
                })
                .on('change', SELECTORS.PRICE_FROM, function(){
                    var $min = $(SELECTORS.PRICE_FROM).val()>>0,
                        $max = $(SELECTORS.PRICE_TO).val()>>0;

                    if ($min > $max) {
                        $min = $max;
                        $(SELECTORS.PRICE_FROM).val($min);
                    }
                })
                .on('change', SELECTORS.PRICE_TO, function(){
                    var $min = $(SELECTORS.PRICE_FROM).val()>>0,
                        $max = $(SELECTORS.PRICE_TO).val()>>0;

                    if($min > $max) {
                        $max = $min;
                        $(SELECTORS.PRICE_TO).val($max);
                    }
                    //$(SELECTORS.PRICE_RANGE).slider('values', 1 ,$max);
                })
                .on('keydown', SELECTORS.SEARCH_FORM_FIELDS, function(e) {
                    if (e.keyCode === 13) {
                        e.preventDefault();
                        that.contentSearch();
                    }
                });

            $.fn.valChange = function (val) {
                var $this    = $(this),
                    oldValue = $this.val();

                if (oldValue !== val) {
                    $this.val(val);
                    $this.trigger('change');
                }
            };
        }
    };

    new LeftSearch();
});