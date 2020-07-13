var $gud;
var $App;
(function ($gud) {
    var builder = function () {


        function processMoving(item) {
            var polyline = [];
            //this.allPoints = this.allPoints.concat(item.Items);
            for (var i = 0; i < item.Items.length; i++) {
                polyline.push(new L.LatLng(item.Items[i].b, item.Items[i].c));
            }
            return L.polyline(polyline, {
                //color: Views.ViewRepositoryManager.GetColorByIndex(this.array[j].speed, this.record),
                opacity: 1,
                weight: getWeight(),
                lineJoin: 'round'
            });
        };

        function processParking(item) {
            //this.markers.push(this.createMarkerNew(item));
        };

        function process(items) {
            var polylines = [],
                markers = [];
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                if (item.TrackItemEnum === "Parking") {
                    markers.push(processParking(item));
                } else {
                    polylines.push(processMoving(item));
                }
            }
            return {
                polylines: polylines,
                markers: markers
            }
        }

        var build = function (item) {
            var source = item.source,
                polylines = [];

            if (!source || source.length === 0) {
                item.built = false;
                return;
            }
            item.el = process(source.TrackItems);
            item.built = true;
        };;

        return {
            build: build
        };
    };
    $gud.trackBuilder = builder();
})($gud || ($gud = {}));

(function ($gud) {
    var view = function () {
        var show = function (item) {
            if (!item.el) { return; }
            var poly = item.el.polylines;
            for (var i = 0; i < poly.length; i++) {
                poly[i].addTo($App.Map.Map);
            }
        };

        var hide = function (item) {
            if (!item || !item.el) { return; }
            var poly = item.el.polylines;
            for (var i = 0; i < poly.length; i++) {
                poly[i].addTo($App.Map.Map);
                $App.Map.Map.removeLayer(poly[i]);
            }
        };

        var remove = function (item) {
            hide(item);
            delete item.el;
        };

        return {
            show: show,
            hide: hide,
            remove: remove
        };
    };
    $gud.trackView = view();
})($gud || ($gud = {}));