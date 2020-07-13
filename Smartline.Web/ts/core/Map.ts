/// <reference path="Application.ts" />
module Core {
    declare var L; //leaflet
    declare var App; //extnet
    declare var $App: Application;  //own

    export class Map {
        private _zoomDefault: number = 13;
        private _map: any;

        private get LatitudeDefault(): number {
            return $App.SettingsManager.Settings.Latitude ? $App.SettingsManager.Settings.Latitude : 50.450126;
        }

        private get LongitudeDefault(): number {
            return $App.SettingsManager.Settings.Longitude ? $App.SettingsManager.Settings.Longitude : 30.523925;
        }

        public get Map(): any {
            return this._map;
        }

        constructor(contanerName: string) {
            this.CreateMap(contanerName);
        }

        private CreateMap(containerName: string): void {
            this._map = L.map(containerName, { zoomControl: true }).setView([this.LatitudeDefault, this.LongitudeDefault], this._zoomDefault);
            this.InitializeLayers();
        }

        //todo: refactor
        private InitializeLayers(): void {
            var osm = new L.TileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png');
            var ggl = new L.Google();
            var gglSattelite = new L.Google();
            ggl._type = 'ROADMAP';
            this._map.addLayer(osm);
            this._map.addControl(this.getTrackHistoryControl());            
            this._map.addControl(new L.Control.Layers({ 'OSM': osm, 'Google Карта Дороги': ggl, 'Google Карта Спутник': gglSattelite }, {}));
        }

        private getTrackHistoryControl(): any {
            var trackControl = L.Control.extend({
                options: {
                    position: 'topright'
                },

                onAdd: map=> {
                    var container = L.DomUtil.create('div', 'leaflet-control-layers track-control');
                    container.title = 'История передвижения';
                    container.onclick = () => {
                        App.windowTrack.show();
                    };
                    return container;
                }
            });
            return new trackControl();
        }        
    }
}