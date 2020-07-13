var $gud;
var DR;
(function ($gud) {
    var TrackWorker = function () {
        var currentItem;
        var cache = (function () {
            var cache = [];
            var itemsAreEqual = function (item1, item2) {
                return item1.df.toLocaleDateString() === item2.df.toLocaleDateString() &&
                       item1.dt.toLocaleDateString() === item2.dt.toLocaleDateString() &&
                       item1.tf.toLocaleTimeString() === item2.tf.toLocaleTimeString() &&
                       item1.tt.toLocaleTimeString() === item2.tt.toLocaleTimeString();
            }

            var get = function (filter) {
                var cachedItem;

                if (typeof filter === "number") {
                    return cache[filter];
                }
                if (!filter || !filter.id || !cache[filter.id]) { return null; }
                cachedItem = cache[filter.id];
                return itemsAreEqual(cachedItem.filter, filter) ? cachedItem : null;
            };

            var set = function (item) {
                cache[item.filter.id] = item;
            };

            var remove = function (item) {
                if (typeof item === "number") {
                    delete cache[item];
                }
                if (item.id) {
                    delete cache[item.id];
                }
            };

            return {
                set: set,
                get: get,
                remove: remove
            };
        })();
        this.show = function (filter) {
            if (currentItem) {
                this.hide();
            }
            var item = cache.get(filter), that = this;
            if (item) {
                this.processShow(item);
                return;
            }
            item = { filter: filter };
            if (!item.loaded) {
                $gud.trackWorker.loader.load(filter, function (result) {
                    if (result.success === true) {
                        item.loaded = true;
                        item.source = result.value;
                        cache.set(item);
                        that.processShow(item);
                    } else {
                        item.loaded = false;
                    }
                });
            }
        };
        this.processShow = function (item) {
            if (!item.built) {
                $gud.trackBuilder.build(item);
            }
            $gud.trackView.show(item);
            currentItem = item;
        }
        this.hide = function (id) {
            var item = cache.get(id || (currentItem && currentItem.filter.id) || -1);
            if (!item || !item.built) { return; }
            $gud.trackView.hide(item);
            currentItem = null;
        };
        this.delete = function (id) {
            var item = cache.get(id);
            if (!item || !item.built) { return; }
            $gud.trackView.remove(item);
            currentItem = null;
        }

    };
    var loader = (function (dr) {
        return {
            load: function (item, callback) {
                if (!item || !item.id || !callback) { return; }
                var that = this;
                dr.LoadTrackNew(item, {
                    success: function (obj) {
                        callback({ value: obj, success: true });
                    },
                    failure: function (error) {
                        callback({ value: obj, success: false, error: error });
                    }
                });
            }
        };
    })(DR || (DR = {}));
    $gud.trackWorker = new TrackWorker();
    $gud.trackWorker.loader = loader;

})($gud || ($gud = {}));