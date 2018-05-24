(function (factory) {
    "use strict";
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as an anonymous module.
        define(['jquery', 'vex' ], factory);
    } else {
        // Browser globals
        factory(jQuery);
    }
}(function ($, vex) {

    var Faq = function (options, $el) {
        this.options = options;
        this.start = 0;
        this.tag = 0;
        this.$el = $el;
        this.$moreButton = null;
        this.spinner = null;
        this.$posts = null;
        this.type = options['type'];
        this.id = options['id'];
        this.not_registered_allowed = false;
        this.$form = null;
        this.init();
    };

    Faq.prototype.init = function () {
        var initSpinner = $(this.options.spinner);
        this.$el.append(initSpinner);
        this.$spinner = initSpinner;
        this.load();
    };

    Faq.prototype.initFaq = function () {
        var self = this;

        self.$posts = self.$el.find(self.options['posts']);
        // self.$spinner = self.$el.find('.loader');
        self.$moreButton = self.$el.find('.show-all');

        self.$moreButton.on('click', function (e) {
            e.preventDefault();
            var count = $(this).data('count');
            self.load(count);
        });

        self.$el
            .on('click', '.comments__item__rating__btn:not(.js-openLoginModal)', function(e) {
                e.preventDefault();

                var data = {
                    comment_id: $(this).data('comment'),
                    rating:     $(this).hasClass('minus') ? -1 : 1
                };

                $.ajax({
                    url: self.options['url_add_rating'],
                    type: 'post',
                    data: JSON.stringify(data),
                    dataType: 'json',
                    cache: false,
                    beforeSend: function () {
                        APP.showGlobalLoader();
                    },
                    complete: function () {
                        APP.hideGlobalLoader();
                    },
                    success: function (json) {
                        if ('error' in json) {
                            vex.dialog.open({
                                input: '<section>' + json['error'] + '</section>',
                                buttons: false,
                                className: 'vex-small',
                                showCloseButton: true
                            });
                        } else {
                            if ('success' in json && json['success'] == true) {
                                $('.value[data-comment="' + data.comment_id + '"]').text(json['rating']);
                            }
                        }
                    }
                });
            })
            .on('click', '.comments__item__header__tags span, .comments__header__tags a', function (e) {
                e.preventDefault();
                var isActive = $(this).hasClass('active');

                self.tag = isActive ? 0 : $(this).data('tag');
                self.start = 0;
                self.load();
            });

        self.$el.on('click', '.comments__more', function(e) {
            e.preventDefault();
            var $this = $(this);
            // self.$spinner.show();
            APP.showGlobalLoader();
            window.setTimeout( function() {
                $this.closest('.comment-block').find('li.hidden').removeClass('hidden');
                $this.closest('.comments__item__link').remove();
                // self.$spinner.hide();
                APP.hideGlobalLoader();
            }, 100);
        });

        // Написать отзыв - открываем модальное окно
        self.$el.on('click', '.faq-add-modal:not(.js-openLoginModal)', function(e) {
            e.preventDefault();
            var $faqAddModal = vex.dialog.open({
                input: $('#faq-modal').html(),
                buttons: false,
                className: 'vex-small',
                showCloseButton: true,
                afterOpen: function() {
                    var $el = $(this.rootEl),
                        $form = $el.find('.faq-form');

                    $el.find('.add-question').click(function () {
                        $(this).addClass('disabled');
                        self.addFaq($form, function() {$faqAddModal.close();});
                    });
                }
            });
        });

        // Комментировать - открываем модальное окно
        self.$el.on('click', '.answer:not(.js-openLoginModal)', function (e) {
            e.preventDefault();

            var parentId = $(this).data('comment');
            var $faqCommentModal = vex.dialog.open({
                input: $('#faq-comment-modal').html(),
                buttons: false,
                className: 'vex-small',
                showCloseButton: true,
                afterOpen: function() {
                    var $el = $(this.rootEl),
                        $form = $el.find('.faq-comment-form');
                    $form.data('parent_comment_id', parentId);

                    $el.find('.add-answer').click(function (e) {
                        e.preventDefault();
                        $(this).addClass('disabled');
                        self.addFaq($form, function() {$faqCommentModal.close();});
                    });
                }
            });
        });

    };

    Faq.prototype.addFaq = function (form, success) {
        var self = this,
            textArea = form.find('textarea'),
            pid = form.data('parent_comment_id'),
            parent_id = typeof pid !== 'undefined' ? parseInt(pid) : null,
            data = {
                product_id: self.id,
                parent_id: parent_id,
                text: textArea.val(),
                tags: {}
            };

        if (form.find('.comments__add__tags a.active').length) {
            form.find('.comments__add__tags a.active').each(function () {
                data.tags[$(this).index()] = $(this).data('tag');
            });
        }

        $.ajax({
            url: self.options['url_add'],
            type: 'post',
            data: JSON.stringify(data),
            cache: false,
            beforeSend: function () {
                form.find('.error').text('');
            },
            complete: function () {
                form.find('.btn').removeClass('disabled');
            },
            success: function (json) {
                if ('errors' in json) {
                    for (var key in json['errors']) {
                        if (key === 'params') {
                            $.error(errors['params']);
                        } else {
                            form.find('.error[data-field=' + key + ']').text(json['errors'][key]);
                        }
                    }
                } else {
                    if ('success' in json && json['success'] == true) {
                        var $note = form.find('.note').removeClass('hidden');

                        APP.flashMessage($note, 'Ваше сообщение отправлено, после проверки модератором оно появится на странице', function () {
                            success && $.isFunction(success) && success();
                        });
                    }
                }
            }
        });
    };

    Faq.prototype.load = function (limit) {
        var self = this;
        $.ajax({
            url: self.options['url'],
            type: 'POST',
            data: JSON.stringify({
                type: self.type,
                id: self.id,
                start: self.start,
                limit: limit,
                tag: self.tag
            }),
            contentType: 'application/json',
            dataType: 'json',
            cache: false,
            beforeSend: function () {
                // self.$spinner.show();
                APP.showGlobalLoader();
                if (self.$moreButton) {
                    self.$moreButton.hide();
                }
            },
            success: function (json) {
                if ('success' in json) {
                    // self.$spinner.hide();
                    APP.hideGlobalLoader();
                    if (json['success'] === true) {
                        var limit_init = json['limit_init'];

                        if (self.options.tab && 'count' in json && json.count) {
                            var $tab = $(self.options.tab);
                            if ($tab.length) {
                                $tab.text($tab.text() + ' (' + json.count + ')');
                            }
                        }
                        if ('rendered' in json) {
                            if (!self.start) {
                                var $newEl = $(json['rendered']);
                                $newEl.hide();
                                self.$el.replaceWith($newEl);
                                self.$el = $newEl;
                                self.initFaq();
                                $newEl.fadeIn(500, function () {
                                    self.checkHash();
                                });
                            } else {
                                self.$posts.append(json['rendered']);
                                self.checkHash();
                            }
                        }

                        // если json['start'] > 0, значит нажали "Показать еще"
                        if ('start' in json) {
                            self.start = json['start'] > 0
                                ? parseInt(json['start']) + parseInt(json['limit'])
                                : parseInt(limit_init);
                        }

                        // limit_init - кол-во штук при загрузке товара
                        // limit - кол-во штук, когда нажимаем "Показать еще"
                        if ('has_more' in json) {
                            self.$moreButton.toggle(json['has_more']);
                            var total_comment = json['total_comment'],
                                total = json['start'] > 0
                                    ? total_comment - json['start'] - json['limit']
                                    : total_comment - limit_init,
                                limit = json['limit'] > total ? total : json['limit'],

                                btnText = 'Показать ещё ' + limit +' из '+ total;
                            self.$moreButton.text(btnText);
                        }

                        if ('not_registered_allowed' in json) {
                            self.not_registered_allowed = json['not_registered_allowed'];
                        }
                    } else {
                        $.error(json['error']);
                    }
                }
            }
        });

    };

    Faq.prototype.checkHash = function () {
        var self = this,
            hash = window.location.hash;
        if (hash.indexOf('comment') === 1) {
            var $els = $(hash);
            if ($els.length > 0) {
                $(document).scrollTop($els.offset().top);
            }
        }
    };

    var methods = {
        getObject: function () {
            var $el = $(this),
                faq = $el.data('faq');
            return faq;
        }
    };

    $.fn.faq = function (params) {
        if ('faq') {
            this.each(function (idx, el) {
                if (methods[params]) {
                    return methods[params].apply(this, Array.prototype.slice.call(args, 1));
                } else if (typeof params === 'object' || !params) {
                    var $el = $(el),
                        opts = $.extend({}, {
                            url: '/?route=lib/faq/feed',
                            url_add: '/?route=lib/faq/addFaq',
                            url_add_rating: '/?route=lib/review/addReviewRating',
                            posts: 'div.post',
                            minFaqLength: 5
                        }, params),
                        faq = new Faq(opts, $el);

                    // CTRL + N
                    $(document).keydown(function (e) {
                        if (e.ctrlKey && e.keyCode === 78) {
                            $("html, body").animate({scrollTop: $(document).height() + 2000}, 'fast');
                            $el.trigger('faq:load');
                        }
                    });

                    $el.on('faq:load', function (e) {
                        faq.load();
                    });

                    $.data(el, 'faq', faq);
                } else {
                    $.error('unknown param');
                }
            });
        }

    }
}));