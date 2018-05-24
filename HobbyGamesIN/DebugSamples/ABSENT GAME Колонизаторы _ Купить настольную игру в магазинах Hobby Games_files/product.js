(function (factory) {
    "use strict";
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as an anonymous module.
        define(['jquery', 'vex', 'pages/faq', 'pages/reviews', 'spoiler', 
            'fancybox', 'bxSlider', 'owlCarousel', 'scrollspy', 'rating', 'trunk', 'lightgallery', 'lg-thumbnail'], factory);
    } else {
        // Browser globals
        factory(jQuery);
    }
}(function ($, vex) {
    $('.desc-text.collapsed').each(function(i, el) {
        var $content = $(el);
        if ($content.height() === parseInt($content.css('max-height'))) {
            $content.next('.desc-more').on('click', function(e) {
                e.preventDefault();
                $content.removeClass('collapsed');
                $(this).remove();
            }).show();
        }
    });

    $('.product-info__main__menu a, .product-info__fix-nav__menu a').click(function () {
        var fix = 0,
            offset = 15,
            $fixed = $('.product-info__fix-nav');

        if ($fixed.length > 0) {
            fix = $fixed.height() + offset;
        }

        $('html,body').animate({
            scrollTop: $($(this).attr('href')).offset().top - fix
        }, 500);

        return false;
    });

    // Scroll from head rating to block reviews
    $('.product-info__main__head .rating').on('change', function () {
        $('a[href="#reviews"]').click();
    });

    $(".spoiler").spoiler();

    $('.product-info__main').lightGallery({
        selector: 'a.lightGallery',
        thumbnail: true,
        animateThumb: true
    });

    var bx = $('.product-info__thumbs').bxSlider({
		pager: false,
        minSlides: 1,
        maxSlides: 6,
        moveSlides: 1,
        slideWidth: 70,
        slideMargin: 10,
        speed: 300,
        controls: false
	});

    var owl = $('.product-info .owl-carousel');

    owl.on('initialized.owl.carousel', function(e) {
        $('.product-info__images').lightGallery({
            selector: '.owl-item:not(".cloned") a',
            thumbnail: true,
            animateThumb: true
        });

        //раньше в блоке справа были только Рекомендованные, теперь добавились теги, поэтому marginTop не нужен
        //setTimeout(function () {
        //    var top = $('.product-info__images').height() - ($('.product-info__cart').height() + $('.product-info__params').height() - 50);
        //    if (top > 0) {
        //        $('.product-info__related').css({
        //            'marginTop': -top
        //        });
        //    }
        //}, 1000);
    });

    owl.owlCarousel({
        items: 1,
        navText: ['', ''],
        navClass: ['owl-prev icon-arrow-left main-background', 'owl-next icon-arrow-right main-background'],
        center: true,
        loop: $('.image-item').length > 1,
        margin: 10,
        mouseDrag: false,
        thumbs: true,
        nav: true
    });
    owl.trigger('to.owl.carousel', 0);

    owl.on('changed.owl.carousel', function(e) {
        if (e.namespace && e.property.name === 'position') {
            bx.goToSlide(e.page.index);
            $('.product-info__thumbs a').removeClass('active');
            $('.product-info__thumbs a[data-index="' + e.page.index + '"]').addClass('active');
        }
    });

    $('.product-info__thumbs > a').click(function() {
        $('.product-info__thumbs a').removeClass('active');
        $(this).addClass('active');
        owl.trigger('to.owl.carousel', [$(this).data('index')]);
    });

    if ($(window).width() <= 767) {
        $('#block-tabs1-tab1').owlCarousel({
            items: 1,
            navText: ['', ''],
            navClass: ['owl-prev icon-arrow-left main-background', 'owl-next icon-arrow-right main-background'],
            loop: true,
            nav: true
        });
    }

    $(window).scroll(function () {
        if ($(this).scrollTop () > $('.product-info__background').height() - $('.product-info__fix-nav').height()) {
            $('body').addClass('show-fix-nav');
        } else {
            $('body').removeClass('show-fix-nav');
        }
    });

    $('body').scrollspy({
        target: '.product-info__fix-nav__menu',
        offset: 100
    });

    $('#package img').each(function() {
        $(this).wrap('<a class="lightGallery" href="' + $(this).attr('src') + '"></a>');
    });

    $('#package').lightGallery({
        selector: 'a.lightGallery',
        thumbnail: true,
        animateThumb: true
    });

    var product_id = $('body').data('id');

    $('#reviews').reviews({
        id: product_id
    });

    $('#faq').faq({
        id: product_id
    });

    $.fn.tabs = function() {
        var selector = this;
        
        this.each(function() {
            var obj = $(this); 
            
            $(obj.attr('href')).hide();
            
            obj.click(function() {
                $(selector).removeClass('active');
                
                $(this).addClass('active');
                
                $($(this).attr('href')).fadeIn();
                
                $(selector).not(this).each(function(i, element) {
                    $($(element).attr('href')).hide();
                });
                
                return false;
            });
        });
    
        $(this).show();
        
        $(this).first().click();
    };

    if ($('.product-info__params__item').length <= 2) {
        $('.tags').css('margin-bottom', '-5px');
    }

    if ($('.product-info__params__item .symbols').is(":empty")) {
        $('.product-info__params__item .symbols').remove();
    }

    $('.product-info__related .product-item-mini a.name span').trunk8({
        lines: 3
    });

    $(window).resize(function (event) {
        $('.product-info__related .product-item-mini a.name span').trunk8({
            lines: 3
        });
        OrganizedTags();
    });

    //оптимизируем теги товара по принципу тетриса
    // п.с. Если не можешь написать хорошо, тогда напиши комментарии
    var OrganizedTags = function() {
        var tags_object = {},
            tags_widths = [],
            mainBlock = $('.hidden-md-up').is(':visible') ? $('.hidden-md-up')
                : $('.hidden-sm-down'),
            tagsBlock = mainBlock.find('.tags'),
            widthBlock = mainBlock.find('.product-info__params__item').width();

        mainBlock.find('.tags a').each(function() {
            var $this = $(this);
            // если элементы одинаковые по ширине и одинаковый индекс - нехорошо
            // в таком случае добавляем буквы к ключу массива
            if (tags_object[$this.outerWidth()]) {
                var width = $this.outerWidth() + Math.random().toString(36).replace(/[^a-z]+/g, '').substr(0,2);
                tags_object[width] = $this;
                tags_widths.push(width);
            } else {
                tags_object[$this.outerWidth()] = $this;
                tags_widths.push($this.outerWidth());
            }
        });
        tags_widths.sort(function(a,b){return parseInt(a) < parseInt(b);});

        mainBlock.find('.tags').html('');
        for (var key in tags_widths) {
            var elem = tags_widths[key],
                elem_int = parseInt(elem),
                min_elem = tags_widths[tags_widths.length-1],
                min2_elem = tags_widths[tags_widths.length-2];

            if (elem_int < widthBlock) {
                tagsBlock.append(tags_object[elem]);
                //проверяем, какой оптимальнее из двух последних элементов вставить
                if (getSum(elem_int,min_elem,widthBlock) > getSum(elem_int,min2_elem,widthBlock)) {
                    if (elem_int != parseInt(min_elem) && StandTag(elem,min_elem,widthBlock))
                        tags_widths.splice(tags_widths.length-3, 1);
                } else {
                    if (StandTag(elem,min_elem,widthBlock))
                        tags_widths.pop();
                }
            }
        }
        // проверяем, поместятся ли еще один блок
        // при рассчете ширины учитываем 10 - у каждого тега margin-right: 5px
        function StandTag (elem1, elem2, widthBlock) {
            if (parseInt(elem1) + parseInt(elem2) + 10 <= widthBlock) {
                tagsBlock.append(tags_object[elem2]);
                return true;
            } else return false;

        }

        // проверяем, поместятся ли два блока
        // при рассчете ширины учитываем 10 - у каждого тега margin-right: 5px
        function getSum(elem1, elem2,widthBlock) {
            var elem1 = parseInt(elem1),
                elem2 = parseInt(elem2);

            return widthBlock - elem1 + elem2 + 10 > 0
                ? widthBlock - elem1 + elem2 + 10
                : -1;
        }
    }

    OrganizedTags();
    $('#block-tabs1 a').tabs();

    /* APRIL EVENT */
    var $templateApril = $('#js-callAprilModalTemplate'),
        $rules = $('.april-rules'),
        $rules_dialog = $('#js-callAprilRulesModalTemplate');
    if ($templateApril.length) {
        var $modal = $('.april-modal');
        $modal.on('click', function(e) {
            e.preventDefault();
            var $april_dialog = vex.dialog.open({
                input: $templateApril.html(),
                buttons: false,
                showCloseButton: false,
                closeAllOnPopState: true,
                className: 'april2018',
                afterOpen: function() {
                    var $rules = $('.april-rules');
                    $rules.on('click', function(e) {
                        $april_dialog.close();
                        e.preventDefault();
                        if (!$('.april-rules__inner').length) {
                            vex.dialog.open({
                                input: $rules_dialog.html(),
                                buttons: false,
                                showCloseButton: true,
                                closeAllOnPopState: true,
                                className: 'april-rules'
                            });
                        }
                    });

                    var $share_april = $('.share-april');
                    $share_april.on('click', function(e) {
                        $april_dialog.close();
                        var url = $(this).attr('href');
                        e.preventDefault();
                        window.open(url,'','toolbar=0,status=0,width=626,height=436,top='+((screen.height-470)/2)+',left='+((screen.width-860)/2)+',');
                    });
                }
            });
        });

        $rules.on('click', function(e) {
            e.preventDefault();
            if (!$('.april-rules__inner').length) {
                vex.dialog.open({
                    input: $rules_dialog.html(),
                    buttons: false,
                    showCloseButton: true,
                    closeAllOnPopState: true,
                    className: 'april-rules'
                });
            }
        });
    }
}));