/// <reference path="Application.ts" />
var Core;
(function (Core) {
    var Map = /** @class */ (function () {
        function Map(contanerName) {
            this._zoomDefault = 13;
            this.CreateMap(contanerName);
        }
        Object.defineProperty(Map.prototype, "LatitudeDefault", {
            get: function () {
                return $App.SettingsManager.Settings.Latitude ? $App.SettingsManager.Settings.Latitude : 50.450126;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Map.prototype, "LongitudeDefault", {
            get: function () {
                return $App.SettingsManager.Settings.Longitude ? $App.SettingsManager.Settings.Longitude : 30.523925;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Map.prototype, "Map", {
            get: function () {
                return this._map;
            },
            enumerable: true,
            configurable: true
        });
        Map.prototype.CreateMap = function (containerName) {
            this._map = L.map(containerName, { zoomControl: true }).setView([this.LatitudeDefault, this.LongitudeDefault], this._zoomDefault);
            this.InitializeLayers();
        };
        //todo: refactor
        Map.prototype.InitializeLayers = function () {
            var osm = new L.TileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png');
            var ggl = new L.Google();
            var gglSattelite = new L.Google();
            ggl._type = 'ROADMAP';
            this._map.addLayer(osm);
            this._map.addControl(this.getTrackHistoryControl());
            this._map.addControl(new L.Control.Layers({ 'OSM': osm, 'Google Карта Дороги': ggl, 'Google Карта Спутник': gglSattelite }, {}));
        };
        Map.prototype.getTrackHistoryControl = function () {
            var trackControl = L.Control.extend({
                options: {
                    position: 'topright'
                },
                onAdd: function (map) {
                    var container = L.DomUtil.create('div', 'leaflet-control-layers track-control');
                    container.title = 'История передвижения';
                    container.onclick = function () {
                        App.windowTrack.show();
                    };
                    return container;
                }
            });
            return new trackControl();
        };
        return Map;
    }());
    Core.Map = Map;
})(Core || (Core = {}));
