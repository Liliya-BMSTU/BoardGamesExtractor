define([], function() {
    function State(state) {
        this.state = state;
        this.catalog = null;
        this.results = {};
        this._hasResults = false;
        this.display = null;

        if ('results' in state) {
            this._hasResults = true;
            this.results = state.results;
        }
    }
    State.prototype = {
        update: function(answer) {
            //if ('link' in answer) {
            //    console.log('link', answer.link);
            //}
            if ('state' in answer) {
                this.state = answer.state;
                if (!this.state.page) {
                    this.state.page = 1;
                }
                if (!this.state.results_per_page) {
                    this.state.results_per_page = 20;
                }
                if ('results' in this.state) {
                    this.results = this.state.results;
                    this._hasResults = true;
                    delete this.state.results;
                } else {
                    this._hasResults = false;
                    this.results = {};
                }
            }
            if ('display' in answer) {
                //console.log('display in answer');
                this.display = answer.display;
            }

        },
        getState: function () {
            return this.state;
        },
        setState: function (state) {
            this.state = state;
        },
        hasResults: function () {
            return this._hasResults;
        },
        getResults: function () {
            return this.results;
        },
        hasMore: function () {
            var total_pages = this.getTotalPages(),
                page = this.getPage();
            return !this.hasResults() || total_pages > page;
        },
        getPage: function () {
            if (!this.state.page) {
                this.state.page = 1;
            }
            return this.state.page;
        },
        setPage: function (page) {
            var total_pages = this.getTotalPages;
            if (total_pages !== false && page > total_pages) {
                page = total_pages;
            }
            this.state.page = page;
        },
        getLocationQuantity: function () {
            if (!this.state.location) {
                this.state.location = false;
            }
            return this.state.location;
        },
        setLocationQuantity: function (location) {
            this.state.location = location;
        },
        getResultsPerPage: function() {
            return this.state.results_per_page;
        },
        setResultsPerPage: function(limit) {
            this.state.results_per_page = limit;
        },
        incPage: function () {
            var page = this.getPage();
            console.log('state incPage page', page, 'totalPages', this.getTotalPages(), 'hasResults', this.hasResults() );
            if (this.hasResults() && this.getTotalPages() <= page) {
                return false;
            }
            if (this.hasResults() && this.getTotalPages() >= page) {
                this.state.page += 1;
            }
            return true;
        },
        decPage: function () {
            var page = this.getPage();
            console.log('state decPage page', page, 'totalPages', this.getTotalPages(), 'hasResults', this.hasResults() );
            if (this.hasResults() && page <= 1 ) {
                return false;
            }
            if (this.hasResults() && page > 1) {
                this.state.page -= 1;
            }
            return true;
        },
        lastPage: function () {
            var page = this.getPage(),
                lastPage = this.getTotalPages();

            if (this.hasResults() && page === lastPage ) {
                return false;
            }
            if (this.hasResults() && page < lastPage) {
                this.state.page = lastPage;
            }
            return true;
        },
        getTotalPages: function () {
            return (this.hasResults() && 'total_pages' in this.results) ? this.results['total_pages'] : false;
        },
        getHit: function() {
            return 'hit' in this.state && this.state['hit'] === true;
        },
        getNew: function() {
            return 'new' in this.state && this.state['new'] === true;
        },
        getSpecial: function() {
            return 'special' in this.state && (this.state['special'] === true || this.state['sale'] === true)
        },
        getAlive: function() {
            return 'alive' in this.state && this.state['alive'] === true
        },
        getPriceFrom: function() {
            return 'price_from' in this.state ? this.state['price_from'] : 0;
        },
        getPriceTo: function() {
            var sp = 'price_to' in this.state ? this.state['price_to'] : 0;
            if (sp > 0) {
                return sp;
            }
            if (this.catalog !== null) {
                return catalog.getMaxPrice();
            }
            return 0;
        },
        getSort: function() {
            return 'sort' in this.state ? this.state.sort : 'rating';
        },
        getDisplay: function() {
            return this.display !== null ? this.display : null;
        },
        getKeyword: function() {
            return 'keyword' in this.state? this.state.keyword : '';
        }
    }
    return State;
});
