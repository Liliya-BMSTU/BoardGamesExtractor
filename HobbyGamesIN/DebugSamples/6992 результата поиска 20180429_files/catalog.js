define(['jquery', 'modules/lists', 'modules/search.left'], (function ($, lists) {
    var view = $('.product-content'),
        container = view.find('.product-container'),
        select_sort = $('.sort-view select.sortBy');

    select_sort.on('change', function (e, trigger) {
        if (trigger !== false) {
            var sel = $(e.target), val = sel.val(), split = val.split(' '),
                sort = split[0], order = split.length > 1 ? split[1] : null;
            view.lists('changeState', {sort: sort, order: order}, true);
        }
    });

    $('.switch-btns').on('click', 'a', function (e) {
        var btn = $(this),
            display = btn.hasClass('view__line')
                ? 'line'
                : 'grid',
            active = btn.hasClass('view__selected');
        if (active) return false;
        $('.switch-btns a').toggleClass('view__selected');
        view.lists('changeDisplay', display);
    });

    view.lists({
        state: window.searchState,
        url: window.config.url.productsFeed,
        mode: 'replace',
        spinnerTemplate: '',
        moreButtonTemplate: null,
        stateLine: true,
        saveState: true,
        afterLoad: function (state, answer, prototype) {
            var results = state.results,
                display = state.getDisplay();
            state = state.state;

            // sort
            var sort = 'sort' in state
                    ? state.sort.trim().toLowerCase()
                    : 'rating',
                order = 'order' in state
                    ? ' ' + state.order.trim().toUpperCase()
                    : 'ASC',
                sort_val = sort + order;

            select_sort.val(sort_val).trigger('change', false);

            //display
            var is_grid = display === 'grid';
            container.toggleClass('product-list', !is_grid);

            $('.switch-btns a')
                .removeClass('view__selected')
                .filter('.view__' + display).addClass('view__selected');

            // Balloons total
            var $balloon = $('.balloon__content'),
                $balloon_mobile = $('.category-left__result__product');
            $balloon.find('span').text(results.total);
            $balloon_mobile.find('span').text(results.total);
        }
    });

    window.onpopstate = function (e) {
        if (e.state) {
            console.log('STATE: ', e.state);
            view.lists('changeState', e.state, false);
        }
    };

    var $leftFilter = $('.category-left'),
        $showLeftFilterBtn = $('#js-showFilter'),
        toggleLeftFilter = function(e) {
            e.preventDefault();
            $('body').toggleClass('is-filter');
            $leftFilter.toggleClass('is-visible');
        };

    $showLeftFilterBtn.click(toggleLeftFilter);
    $leftFilter.find('.category-left__result__close').click(toggleLeftFilter);
    $(document).keyup(function(e) {
        if ($leftFilter.hasClass('is-visible') && e.keyCode === 27) {
            toggleLeftFilter(e);
        }
    });
}));