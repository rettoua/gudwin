L.Track = L.LayerGroup.extend({
    initialize: function (source, options) {
        this._source = source;
        L.LayerGroup.prototype.initialize.call(this, options);
        L.Util.setOptions(this, options);
        this._map = null;
    },

    buildPolylines: function () {

    },

    redraw: function () {
        this._initPath();
    },

    onAdd: function (map) {
        map.constructor.prototype.latLngToLayerPoint = this.latLngToLayerPoint;
        L.Polyline.prototype.onAdd.call(this, map);
    },
});
L.track = function (paths, options) {
    return new L.Track(paths, options);
};