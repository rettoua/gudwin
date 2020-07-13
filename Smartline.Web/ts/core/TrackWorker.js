var Core;
(function (Core) {
    var TrackWorker = /** @class */ (function () {
        function TrackWorker() {
            this.trackItemsCache = [];
        }
        TrackWorker.prototype.show = function (trackerUid) {
            this.trackItemsCache[trackerUid] && this.doShow(this.trackItemsCache[trackerUid]);
        };
        TrackWorker.prototype.doShow = function (item) {
            TrackView.display(item);
        };
        TrackWorker.prototype.hide = function () {
            if (this.currentTrackItem) {
            }
        };
        TrackWorker.prototype.doHide = function () {
        };
        return TrackWorker;
    }());
    Core.TrackWorker = TrackWorker;
    var TrackItem = /** @class */ (function () {
        function TrackItem(source) {
            this.source = source;
        }
        return TrackItem;
    }());
    var TrackView = /** @class */ (function () {
        function TrackView() {
        }
        TrackView.display = function (item) {
            if (item.created) {
            }
            this.processDissplay(item);
        };
        TrackView.prototype.createView = function (item) {
        };
        TrackView.processDissplay = function (item) {
        };
        TrackView.hide = function (item) {
        };
        TrackView.remove = function (item) {
        };
        return TrackView;
    }());
})(Core || (Core = {}));
