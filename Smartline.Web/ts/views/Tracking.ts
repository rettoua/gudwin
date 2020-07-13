module Views {
    declare var $App: Core.Application;
    declare var L;
    declare var App;
    declare var Ext;
    declare var common;
    declare var $;

    export class Tracking {
        private _repository: Core.Repository;
        private _polyline: any;
        private _pointer: Crosshair;

        private get IsShowHotTracking(): boolean {
            return $App.SettingsManager.Settings.ShowTracking;
        }

        public get pointer(): Crosshair {
            return this._pointer;
        }

        constructor(repository: Core.Repository) {
            this._repository = repository;
        }

        public Start(): void {
            this.CreatePolyline();
            this.CreatePointer();
        }

        public Stop(): void {
            this.Delete();
        }

        public Show(): void {
            if (this._polyline) {
                this._polyline.addTo($App.Map.Map);
            }
        }

        public Hide(): void {
            if (this._polyline) {
                this._polyline._latlngs = [];
                $App.Map.Map.removeLayer(this._polyline);
            }
        }

        public AddPoint(gps: Models.Gps): void {
            if (!gps.Latitude || !gps.Longitude) { return; }
            var latLng = new L.LatLng(gps.Latitude, gps.Longitude);
            if (this.IsShowHotTracking === true) {
                if ($App.SettingsManager.Settings.PointsInPath && this._polyline._latlngs.length >= $App.SettingsManager.Settings.PointsInPath) {
                    this._polyline._latlngs.shift();
                }
                this._polyline.addLatLng(latLng);
            }
            this._pointer.recalc();
        }

        private CreatePolyline(): void {
            this._polyline = L.polyline([], {
                color: this._repository.Record.get('Color'),
                opacity: 1.0,
                weight: $App.SettingsManager.Settings.Weight,
                lineJoin: 'round'
            }).addTo($App.Map.Map);
        }

        private CreatePointer(): void {
            this.Center(this._repository.Location);
            this._pointer = new Crosshair(this._repository);

        }

        private Delete(): void {
            $App.Map.Map.removeLayer(this._polyline);
            $App.Map.Map.removeLayer(this._pointer.Pointer);
            delete this._polyline;
        }

        private Center(point: any): void {
            $App.Map.Map.panTo(point, { duration: 1 });
        }
    }

    export interface CrosshairMap {
        shouldRecalc: boolean;
        initialize(repo: Core.Repository): void;
        onAdd(map: any): void;
        onRemove(map: any): void;
        _reset(): void;
        recalc(p: any, d: any): void;
        calcDirection(lat1: number, long1: number, lat2: number, long2: number): number;
        centralize(): void;
    }

    export class Crosshair {
        private _repository: Core.Repository;
        private _crosshair: CrosshairMap;
        private _p1: any;
        private _imageCarRun: HTMLImageElement;
        private _imageCarStop: HTMLImageElement;
        private _imageSos: HTMLImageElement;
        private _imageSosLite: HTMLImageElement;
        private _canvas: HTMLCanvasElement;
        private _title: CrosshairTitle;
        private _angle: number;

        constructor(repository: Core.Repository) {
            this._repository = repository;
            this.CreateCrosshair();
        }

        public get Pointer(): CrosshairMap {
            return this._crosshair;
        }

        private CreateCrosshair(): void {
            var crosshair = L.Class.extend({

                shouldRecalc: true,
                initialize: () => { this.initialize(); },
                onAdd: (map) => { this.onAdd(map); },
                onRemove: (map) => { this.onRemove(map); },
                _reset: () => { this._reset(); },
                centralize: () => { this.centralize(); },
                recalc: () => { this.recalc(); },
                calcDirection: (lat1, long1, lat2, long2) => { this.calcDirection(lat1, long1, lat2, long2); },

            });
            this._crosshair = new crosshair();
            $App.Map.Map.addLayer(this._crosshair);
        }

        public initialize(): void {
            // save position of the layer or any options from the constructor            
            this._p1 = this._repository.Location;
        }

        public onAdd(map: any): void {

            var panes = map.getPanes();
            this._title = new CrosshairTitle(panes.overlayPane, this._repository);

            this._imageCarRun = document.createElement('img');
            this._imageCarRun.setAttribute('src', Ext.String.format('Resources/car_{0}.png', this.getImageName()));
            this._imageCarRun.style.visibility = 'hidden';
            this._imageCarRun.style.position = "absolute";

            this._imageCarStop = document.createElement('img');
            this._imageCarStop.setAttribute('src', Ext.String.format('Resources/car_{0}_stop.png', this.getImageName()));
            this._imageCarStop.style.visibility = 'hidden';
            this._imageCarStop.style.position = "absolute";

            this._canvas = document.createElement('canvas');
            this._canvas.style.width = 40 + 'px';
            this._canvas.style.height = 40 + 'px';
            this._canvas.style.position = "absolute";
            this._canvas.style.zIndex = '30';

            panes.overlayPane.appendChild(this._imageCarRun);
            panes.overlayPane.appendChild(this._imageCarStop);
            panes.overlayPane.appendChild(this._canvas);

            // add a viewreset event listener for updating layer's position, do the latter
            map.on('viewreset', this._reset, this);
            this._reset();
            setTimeout(Ext.Function.bind(this.recalc, this), 300);
        }

        public onRemove(map: any): void {
            map.getPanes().overlayPane.removeChild(this._imageCarRun);
            map.getPanes().overlayPane.removeChild(this._imageCarStop);
            map.getPanes().overlayPane.removeChild(this._canvas);
            if (this._imageSos) {
                map.getPanes().overlayPane.removeChild(this._imageSos);
            }
            if (this._imageSosLite) {
                map.getPanes().overlayPane.removeChild(this._imageSosLite);
            }
            this._title.Dispose();
            $App.Map.Map.off('viewreset', this._reset, this);
        }

        public _reset(): void {
            var sw = $App.Map.Map.latLngToLayerPoint(this._repository.Location);
            var div = this._canvas;
            div.style.left = (sw.x - 20) + 'px';
            div.style.top = (sw.y - 20) + 'px';
            this._title.SetLocation(sw);
        }

        public recalc(): void {
            var shouldRecalc = true;

            if (this._p1.lat == this._repository.Location.lat && this._p1.lng == this._repository.Location.lng && this._angle) {
                shouldRecalc = false;
            }

            if (shouldRecalc) {
                this._angle = this._repository.Image.isDirectionRequired ?
                this.calcDirection(this._p1.lat, this._p1.lng, this._repository.Location.lat, this._repository.Location.lng) : 0;
            }
            if (isNaN(this._angle)) {
                this._angle = 1;
            }

            this._title.Update();
            var img = this.getImage();

            var cContext = this._canvas.getContext('2d');
            this._canvas.setAttribute('width', '40');
            this._canvas.setAttribute('height', '40');
            cContext.translate(20, 20);
            cContext.rotate(this._angle * Math.PI / 180);
            cContext.drawImage(img, -20, -20);
            this._reset();
            this._p1 = this._repository.Location;
        }

        private getImage(): any {
            if (this._repository.isSosAlarming) {
                if (!this._imageSos) {
                    this.createSosImage();
                }
                if (!this._imageSosLite) {
                    this.createSosLiteImage();
                }
                return this._repository.Record.data.lastSos ? this._imageSos : this._imageSosLite;
            }
            return this._repository.IsRun ? this._imageCarRun : this._imageCarStop;
        }

        private getImageName(): string {
            return this._repository.Image.name;
        }

        private createSosImage(): void {
            this._imageSos = document.createElement('img');
            this._imageSos.setAttribute('src', Ext.String.format('Resources/car_{0}_sos.png', this.getImageName()));
            this._imageSos.style.visibility = 'hidden';
            this._imageSos.style.position = "absolute";
            $App.Map.Map.getPanes().overlayPane.appendChild(this._imageSos);
        }

        private createSosLiteImage(): void {
            this._imageSosLite = document.createElement('img');
            this._imageSosLite.setAttribute('src', Ext.String.format('Resources/car_{0}_sos_lite.png', this.getImageName()));
            this._imageSosLite.style.visibility = 'hidden';
            this._imageSosLite.style.position = "absolute";
            $App.Map.Map.getPanes().overlayPane.appendChild(this._imageSosLite);
        }

        public calcDirection(lat1, long1, lat2, long2): number {
            var x1, y1, y2, result;

            x1 = Math.sin((-long2 + long1) * Math.PI / 180);
            y1 = Math.cos(lat2 * Math.PI / 180) * Math.tan(lat1 * Math.PI / 180);
            y2 = Math.sin(lat2 * Math.PI / 180) * Math.cos((-long2 + long1) * Math.PI / 180);
            result = Math.atan(x1 / (y1 - y2)) * 180 / Math.PI;
            if (result < 0)
                result = 360 + result;

            if (long2 < long1 && (long2 > (long1 - 180))) {
                if (result < 180)
                    result = result + 180;
                if (result > 180)
                    result = result - 180;
            } else if (lat2 > lat1) {
                result = result + 180;
            }

            return result;
        }

        public centralize(): void {
            if (this._repository.Location) {
                $App.Map.Map.panTo(this._repository.Location, { duration: 1 });
            }
        }
    }

    class CrosshairTitle {
        private _parent: HTMLDivElement;
        private _repository: Core.Repository;
        private _mainDiv: HTMLDivElement;
        private _timeDiv: HTMLElement;
        private _imageGpsSignal: HTMLImageElement;
        private _marker: any;
        private _popup: any;

        constructor(parent: any, repository: Core.Repository) {
            this._parent = parent;
            this._repository = repository;
            this.CreateElements();
        }

        private CreateElements(): void {
            this._mainDiv = document.createElement('div');
            this._mainDiv.style.position = "absolute";
            this._mainDiv.style.opacity = '0.5';
            this._mainDiv.onmouseenter = this.RemoveOpacity;
            this._mainDiv.onmouseleave = this.SetOpacity;
            var table = Ext.String.format(' <table class="repository-title"><tr><td id="td_name{0}"></td></tr><tr><td id="td_time{0}"></td></tr></table>', this._repository.Id);
            //var table = Ext.String.format(' <table class="repository-title"><tr><td id="td_name{0}"></td></tr><tr><td id="td_time{0}"></td></tr></table>', this._repository.Id);
            this._mainDiv.innerHTML = table;
            this._parent.appendChild(this._mainDiv);
            var nameDiv: HTMLElement = document.getElementById('td_name' + this._repository.Id);
            this._timeDiv = document.getElementById('td_time' + this._repository.Id);
            nameDiv.innerHTML = this._repository.Record.get('Name');
            this._timeDiv.innerHTML = Ext.Date.format(this._repository.Record.get('EndSendTime') ? this._repository.Record.get('EndSendTime') : this._repository.Record.get('LastSendTime'), 'H:i:s');

            this._imageGpsSignal = document.createElement('img');
            this._imageGpsSignal.setAttribute('src', 'Resources/no-signal.png');
            this._imageGpsSignal.style.position = "absolute";
            this._imageGpsSignal.style.visibility = 'hidden';
            this._parent.appendChild(this._imageGpsSignal);
            this.CreateMarker();
        }

        private CreateMarker(): void {
            var myIcon = L.icon({
                iconUrl: 'Resources/car_marker.png',
                iconAnchor: [15, 15]
            });
            this._marker = L.marker(this._repository.Location, {
                opacity: 0.0,
                icon: myIcon
            }).addTo($App.Map.Map);
            this._popup = this._marker.bindPopup(this._repository.GetInfoWindowContent())._popup;
        }

        private UpdateMarkerContent() {
            this._popup.setContent(this._repository.GetInfoWindowContent());
        }

        public SetLocation(sw: any): void {
            this._mainDiv.style.left = (sw.x - (this._mainDiv.offsetWidth / 2)) + 'px';
            this._mainDiv.style.top = (sw.y + 20) + 'px';
            this._imageGpsSignal.style.left = (sw.x - 13) + 'px';
            this._imageGpsSignal.style.top = (sw.y - 50) + 'px';
            this._marker.setLatLng(this._repository.Location);
            this.UpdateMarkerContent();
        }

        public Update(): void {
            $(this._timeDiv).text(Ext.Date.format(this._repository.LastTime, 'H:i:s'));
            if (this._repository.isSosAlarming) {
                $(this._mainDiv).find('table').removeClass().addClass(this._repository.Record.data.lastSos ? 'repository-title-sos' : 'repository-title-sos-lite');
            } else {
                $(this._mainDiv).find('table').removeClass().addClass('repository-title');
            }
            this.UpdateGpsSignal();
        }

        private UpdateGpsSignal(): void {
            var hasGpsSignal: any = this._repository.Record.get('i');
            if (hasGpsSignal) {
                this._imageGpsSignal.style.visibility = hasGpsSignal.Active === true ? 'hidden' : 'visible';
            }
        }

        public Dispose(): void {
            $App.Map.Map.removeLayer(this._marker);
            this._parent.removeChild(this._mainDiv);
            this._parent.removeChild(this._imageGpsSignal);
        }

        private SetOpacity(t: any): void {
            t.srcElement.style.opacity = 0.5;
        }

        private RemoveOpacity(t: any): void {
            t.srcElement.style.opacity = 1;
        }

    }

}