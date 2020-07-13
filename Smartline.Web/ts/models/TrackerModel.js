var models;
(function (models) {
    var Tracker = /** @class */ (function () {
        function Tracker(source) {
            this.setData(source);
        }
        Tracker.prototype.setData = function (source) {
            if (typeof source == "undefined" || source == null) {
                return;
            }
            this.uid = source.Id;
            this.trackerId = source.TrackerId;
            this.name = source.Name;
            this.description = source.Description;
            //todo: set all necessary stuff
        };
        Tracker.prototype.update = function (source) {
        };
        return Tracker;
    }());
    models.Tracker = Tracker;
})(models || (models = {}));
