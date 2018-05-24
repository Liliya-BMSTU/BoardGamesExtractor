define(['jquery', 'sprintf', 'modules/state'], function($, _s, State) {
    var Lists = function(options, $el) {
        //console.log('constructor');
        this.options = $.extend({}, {
            url: null,
            mode: 'replace',
            stateLine: false,
            state: null,
            perPage: 20,
            saveState: false,
            spinnerTemplate: '<div class="loader"></div>',
            moreButtonTemplate: '<a class="btn btn-lg">Показать ещё</a>',
            controlsWrapperTemplate: '<div class="btn-block center">',
            pagesWrapperTemplate: '<div class="paginate"></div>',
            stateLineTemplate: '<div class="state"></div>',
            displayChangeCallback: function($el, display) {
                var subj = $el.closest('.view__grid,.view__list');
                switch (display) {
                    case 'line':
                        subj.removeClass('view__grid').addClass('view__list');
                        break;
                    default:
                        subj.removeClass('view__list').addClass('view__grid');

                }
            }

        }, options);
        this.$el = $el;
        this.working = false;
        this.display = options.display;
        this.prevState = {};
        this.init();

    };
    Lists.prototype.init = function() {
        this.url = this.options.url;
        this.mode = this.options.mode;
        this.state = new State(this.options.state);
        this.spinner = this.options.spinnerTemplate === null
            ? null
            : $(this.options.spinnerTemplate);
        this.moreButton = this.options.moreButtonTemplate === null
            ? null
            : $(this.options.moreButtonTemplate);
        this.controlsWrapper = this.options.controlsWrapperTemplate === null
            ? null
            : $(this.options.controlsWrapperTemplate);
        this.addControls();

        this.stateLineWrapper = this.options.stateLineTemplate === null
            ? null
            : $(this.options.stateLineTemplate);

        if (this.state) {
            window.history.replaceState(this.state, '', window.location.href);
        }
    };

    Lists.prototype.addControls = function() {
        //console.log('addControls');
        var self = this;
        self.$el.after(this.controlsWrapper);
        self.controlsWrapper.append(this.moreButton);
        self.controlsWrapper.append(this.spinner);
        if(self.moreButton) {
            self.moreButton.click(function() {
                self.nextPage();
            });
        }

        self.initPagination();
        if (!this.options.stateLine)
            self.getPage({saveState: true, scroll: false});
    };

    Lists.prototype.initPagination = function() {
        //console.log('initPagination');
        var self = this,
            $pages = $('.pagination'),
            $location = this.$el.find('.location'),
            $limit = this.$el.find('.limit');

        if (!this.state.results.total) {
            return;
        }

        if (this.state.getTotalPages() === 1) {
            initLocationQuantityEvents();
            return;
        }

        function initPagesEvents() {
            var $first = $pages.find('.first'),
                $prev = $pages.find('.prev'),
                $next = $pages.find('.next'),
                $last = $pages.find('.last');

            $first.length && $first.click(function(e) {
                e.preventDefault();
                self.setPage(1);
            });
            $prev.length && $prev.click(function(e) {
                e.preventDefault();
                self.prevPage();
            });
            $next.length && $next.click(function(e) {
                e.preventDefault();
                self.nextPage();
            });
            $last.length && $last.click(function(e) {
                e.preventDefault();
                self.lastPage();
            });
            $pages.find('a:not(.first,.prev,.next,.last)').click(function(e) {
                e.preventDefault();
                self.setPage($(this).text());
            });
        }

        function initLocationQuantityEvents() {
            var $linkAll = $location.find('.all'),
                $linkLocal = $location.find('.local');

            $linkLocal.click(function(e) {
                e.preventDefault();
                self.state.setLocationQuantity(true);
                self.getPage();
            });

            $linkAll.click(function(e) {
                e.preventDefault();
                self.state.setLocationQuantity(false);
                self.getPage();
            });
        }

        function initLimitEvents() {
            $limit.find('a').click(function(e) {
                e.preventDefault();
                self.state.setResultsPerPage($(this).data('step'));

                var countPages = Math.ceil(self.state.results.total / self.state.getResultsPerPage());
                if (self.state.getPage() > countPages) {
                    self.state.setPage(countPages);
                }
                self.getPage({scroll: true});
            });
        }

        function initStateLineWrapper() {
            var beforeCb = self.options.beforeLoad,
                afterCb = self.options.afterLoad;

            self.options.beforeLoad = function() {
                self.stateLineWrapper.find('span').fadeTo(150, 0.1);

                beforeCb && beforeCb();
            };
            self.options.afterLoad = function(state, answer, prototype) {
                //state = state.state;
                var results = state.results,
                    text = results.total > 0
                        ? _s.sprintf('Показано c <span>%d</span> по <span>%d</span> из <span>%d</span> (всего страниц: <span>%d</span>)', results.start, results.end, results.total, results.total_pages)
                        : 'Ничего не найдено.';

                self.stateLineWrapper.find('span').fadeTo(150, 1);
                self.stateLineWrapper.html(text);

                afterCb && afterCb(state, answer, prototype);
            }
        }

        initPagesEvents();
        initLocationQuantityEvents();
        initLimitEvents();
        self.options.stateLine && initStateLineWrapper();
    };

    Lists.prototype.changeDisplay = function(display) {
        this.display = display;
        if (typeof this.options.displayChangeCallback === 'function') {
            this.options.displayChangeCallback(this.$el, display);
        }
        this.state.getState().page = 1;
        this.$el.html('');
        this.getPage();
    };

    Lists.prototype.getPage = function(options) {
        var self = this;

        options = $.extend({}, {
            callback: $.noop,
            saveState: true,
            first: false,
            scroll: false
        }, options);

        if (!self.working) {
            if (typeof self.options.beforeLoad === 'function') {
                self.options.beforeLoad();
            }

            self.working = true;
            //self.moreButton.hide();
            self.spinner.show();

            var state = self.state.getState();
            state.page = state.page || 1;

            self.prevState = state;

            $.ajax({
                url: self.options.url,
                beforeSend: function() {
                    window.APP.showGlobalLoader();
                },
                complete: function() {
                    window.APP.hideGlobalLoader();
                },
                type: 'POST',
                data: JSON.stringify({
                    saveState: self.options.saveState,
                    state: state,
                    display: self.display,
                    url: window.location.origin + window.location.pathname
                }),
                contentType : 'application/json',
                dataType: 'json',
                cache: false,
                success: function(json) {
                    if ('state' in json) {
                        var rendered = 'rendered' in json ? json['rendered'] : '';
                        self.state.update(json);

                        if (self.options.mode === 'add') {
                            self.$el.append(rendered);
                        } else {
                            self.$el.html(rendered);
                        }
                        self.spinner.hide();
                        /*if (self.state.hasMore()) {
                            self.moreButton.show()
                        }*/

                        options.callback();

                        if (typeof self.options.afterLoad === 'function') {
                            self.options.afterLoad(self.state, json, self);

                            if (options.scroll) {
                                $('html, body').animate({scrollTop: 0}, 'slow');
                            }

                            if (self.options.saveState && options.first) {
                                console.log('replaceState', state, window.location.href);
                                window.history.replaceState(state, '', window.location.href);
                            }

                            if (options.saveState) {
                                console.log('pushState', state, json.link);
                                window.history.pushState(state, '', json.link);
                            }
                        }

                        if (self.options.stateLine == false && options.saveState) {
                            console.log('pushState', state, json.link);
                            window.history.pushState(state, '', json.link);
                        }
                        self.initPagination();
                        self.$el.find('.to-cart').removeClass('disabled');
                        self.working = false;
                    } else {
                        self.spinner.hide();
                        window.APP.hideGlobalLoader();
                        $.error('bad server answer');
                    }

                }
            });
        } else {
            $.error('WAIT');
        }
    };

    Lists.prototype.nextPage = function(callback) {
        var self = this;
        if (!self.working) {
            self.prevState = $.extend({}, self.state.getState());
            var hasPages = self.state.incPage();
            console.log('lists.nextPage hasPages', hasPages);
            if (hasPages) {
                self.getPage({callback: callback, scroll: true});
            } else {
                $.error('no more pages');
                self.spinner.hide();
                window.APP.hideGlobalLoader();
            }
        } else {
            $.error('WAIT');
        }
    };

    Lists.prototype.prevPage = function(callback) {
        var self = this;
        if (!self.working) {
            self.prevState = $.extend({}, self.state.getState());
            var hasPages = self.state.decPage();
            console.log('lists.prevPage hasPages', hasPages);
            if (hasPages) {
                self.getPage({callback: callback, scroll: true});
            } else {
                $.error('this is first page');
                self.spinner.hide();
                window.APP.hideGlobalLoader();
            }
        } else {
            $.error('WAIT');
        }
    };

    Lists.prototype.lastPage = function(callback) {
        var self = this;
        if (!self.working) {
            self.prevState = $.extend({}, self.state.getState());
            var hasPages = self.state.lastPage();
            console.log('lists.lastPage hasPages', hasPages);
            if (hasPages) {
                self.getPage({callback: callback, scroll: true});
            } else {
                $.error('this is last page');
                self.spinner.hide();
                window.APP.hideGlobalLoader();
            }
        } else {
            $.error('WAIT');
        }
    };

    Lists.prototype.setPage = function(page, callback) {
        var self = this;
        if (!self.working) {
            var totalPages = self.state.getTotalPages();
            console.log('lists.setPage totalPages', totalPages);
            if (page > 0 && page <= totalPages) {
                self.prevState = $.extend({}, self.state.getState());
                self.state.setPage(page);
                self.getPage({callback: callback, scroll: true});
            } else {
                $.error('invalid page');
                self.spinner.hide();
                window.APP.hideGlobalLoader();
            }
        } else {
            $.error('WAIT');
        }
    };

    var methods = {
        changeState: function(params, saveState) {
            saveState = typeof saveState !== 'undefined' ? saveState : true;
            var $el = $(this),
                lists = $el.data('lists'),
                state = lists.state.getState();

            $el.html('');
            lists.prevState = $.extend({}, state);
            $.extend(state, {sort: '', page: 1}, params);
            lists.getPage.apply(lists, [{saveState: saveState, scroll: true}]);
        },
        changeDisplay: function(params) {
            var display = params === 'grid' ? 'grid' : 'line',
                $el = $(this),
                lists = $el.data('lists');
            lists.changeDisplay.call(lists, display);
        },
        getObject: function() {
            var $el = $(this),
                lists = $el.data('lists');
            return lists;
        }
    };


    $.fn.lists = function(params) {
        var args = arguments;
        this.each(function(idx, el) {
            if ( methods[params] ) {
                return methods[ params ].apply( this, Array.prototype.slice.call( args, 1 ));
            } else if ( typeof params === 'object' || ! params ) {
                // init
                var $el = $(el),
                    lists = new Lists(params, $el);

                $el.on('lists:nextPage', function(e) {
                    lists.nextPage();
                });
                // CTRL + N
                $(document).keydown(function(e){ if (e.ctrlKey && e.keyCode === 78) {
                    $("html, body").animate({ scrollTop: $(document).height()  + 2000}, 'fast');
                    $el.trigger('lists:nextPage');
                }});

                $.data(el, 'lists', lists);
            } else {
                $.error( 'unknown param' );
            }
        });
    }
});