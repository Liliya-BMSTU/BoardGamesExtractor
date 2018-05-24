(function (factory) {
    "use strict";
    if (typeof define === 'function' && define.amd) {
        define(['jquery', 'vex', 'rating'], factory);
    } else {
        factory(jQuery);
    }
}(function ($, vex, rating) {
    var Reviews = function (options, $el) {
        this.options = options;
        this.start = 0;
        this.$el = $el;
        this.$moreButton = null;
        this.spinner = null;
        this.$posts = null;
        this.id = options['id'];
        this.not_registered_allowed = false;
        this.init();
    };

    Reviews.prototype.init = function () {
        var initSpinner = $(this.options.spinner);
        this.$el.append(initSpinner);
        this.$spinner = initSpinner;
        this.load();
    };

    Reviews.prototype.initReviews = function () {
        var self = this;

        self.$posts = self.$el.find(self.options['posts']);
        // self.$spinner = self.$el.find('.loader');
        self.$moreButton = self.$el.find('.show-all');
        
        self.$moreButton.on('click', function (e) {
            e.preventDefault();
            var count = $(this).data('count');
            self.load(count);
        });

        self.$el.on('click', '.comments__more', function(e) {
            e.preventDefault();
            var $this = $(this);
            APP.showGlobalLoader();
            window.setTimeout( function() {
                $this.closest('.comment-block').find('li.hidden').removeClass('hidden');
                $this.closest('.comments__item__link').remove();
                APP.hideGlobalLoader();
            }, 100);
        });

        self.$el.on('click', '.comments__item__rating__btn:not(.js-openLoginModal)', function (e) {
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
        });

        // Написать отзыв - открываем модальное окно
        self.$el.on('click', '.review-add-modal:not(.js-openLoginModal)', function(e) {
            e.preventDefault();
            var $reviewAddModal = vex.dialog.open({
                input: $('#review-modal').html(),
                buttons: false,
                className: 'vex-small',
                showCloseButton: true,
                afterOpen: function() {
                    var $el = $(this.rootEl),
                        $form = $el.find('.review-form');

                    $el.find('.rating').rating();
                    $el.find('.review-add').click(function (e) {
                        $(this).addClass('disabled');
                        self.addReview($form, function() {$reviewAddModal.close();});
                    });
                }
            });
        });

        // Комментировать - открываем модальное окно
        self.$el.on('click', '.review-comment:not(.js-openLoginModal)', function (e) {
            e.preventDefault();

            var parentId = $(this).data('comment');
            var $reviewCommentModal = vex.dialog.open({
                input: $('#review-comment-modal').html(),
                buttons: false,
                className: 'vex-small',
                showCloseButton: true,
                afterOpen: function() {
                    var $el = $(this.rootEl),
                        $form = $el.find('.review-comment-form');
                    $form.data('parent_comment_id', parentId);

                    $el.find('.rating').rating();
                    $el.find('.review-comment-add').click(function (e) {
                        e.preventDefault();
                        $(this).addClass('disabled');
                        self.addReview($form, function() {$reviewCommentModal.close();});
                    });
                }
            });
        });


    };

    Reviews.prototype.addReview = function (form, success) {
        var self = this,
            rating = form.find('input[name="rating"]'),
            textArea = form.find('textarea'),
            pid = form.data('parent_comment_id'),
            parent_id = typeof pid !== 'undefined' ? parseInt(pid) : 0,
            data = {
                product_id: self.id,
                parent_id:  parent_id,
                rating:     rating.val(),
                text:       textArea.val()
            };

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

                        APP.flashMessage($note, 'Ваш отзыв добавлен, после проверки модератором он появится на странице', function () {
                            success && $.isFunction(success) && success();
                        });
                    }
                }
            }
        });
    };

    Reviews.prototype.load = function (limit) {
        var self = this;
        $.ajax({
            url: self.options['url'],
            type: 'POST',
            data: JSON.stringify({
                id: self.id,
                start: self.start,
                limit: limit
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
                                self.initReviews();
                                $newEl.fadeIn(500, function () {
                                    self.checkHash();
                                });
                            } else {
                                self.$posts.append(json['rendered']);
                                self.checkHash();
                            }
                        }

                        if ('start' in json) {
                            self.start = parseInt(json['start']) + parseInt(json['limit']);
                        }

                        if ('has_more' in json) {
                            self.$moreButton.toggle(json['has_more']);
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

    Reviews.prototype.checkHash = function () {
        var self = this,
            hash = window.location.hash;
        if (hash.indexOf('reviews') === 1) {
            var $els = $(hash);
            if ($els.length > 0) {
                $(document).scrollTop($els.offset().top);
            } else {
                self.load();
            }
        }
    };

    var methods = {
        getObject: function () {
            var $el = $(this),
                reviews = $el.data('reviews');
            return reviews;
        }
    };

    $.fn.reviews = function (params) {
        if ('Reviews') {
            this.each(function (idx, el) {
                if (methods[params]) {
                    return methods[params].apply(this, Array.prototype.slice.call(args, 1));
                } else if (typeof params === 'object' || !params) {
                    var $el = $(el),
                        opts = $.extend({}, {
                            url: '/?route=lib/review/feed',
                            url_add: '/?route=lib/review/addReview',
                            url_add_rating: '/?route=lib/review/addReviewRating',
                            posts: 'div.post',
                            minReviewsLength: 5
                        }, params),
                        reviews = new Reviews(opts, $el);

                    // CTRL + N
                    $(document).keydown(function (e) {
                        if (e.ctrlKey && e.keyCode === 78) {
                            $("html, body").animate({scrollTop: $(document).height() + 2000}, 'fast');
                            $el.trigger('reviews:load');
                        }
                    });

                    $el.on('reviews:load', function (e) {
                        reviews.load();
                    });

                    $.data(el, 'Reviews', reviews);
                } else {
                    $.error('unknown param');
                }
            });
        }
    }
}));