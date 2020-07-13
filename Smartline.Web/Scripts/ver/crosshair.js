//var Crosshair2 = L.Class.extend({
//     initialize: function () {

//    },

//    onAdd: function (map) {
 
//    },

//    onRemove: function (map) {
      
//    },
//});

var Crosshair1 = L.Class.extend({
    shouldRecalc: true,
    initialize: function (repo) {
        // save position of the layer or any options from the constructor
        this.repository = repo;
        this.p1 = this.repository.Location;
        this.p2 = this.p1;
    },

    onAdd: function (map) {
        this._map = map;

        var panes = map.getPanes();
        this.repositoryTitle = new RepositoryTitleDiv(panes.overlayPane, this.repository, this);
        this.repositoryTitle.initialize();

        this.imgM = document.createElement('img');
        this.imgM.setAttribute('src', 'Resources/car_top.png');
        this.imgM.style.visibility = 'hidden';
        this.imgM.style.position = "absolute";
        this.img = this.imgM;

        this.imgP = document.createElement('img');
        this.imgP.setAttribute('src', 'Resources/car_top_stop.png');
        this.imgP.style.visibility = 'hidden';
        this.imgP.style.position = "absolute";

        this.canvas = document.createElement('canvas');
        this.canvas.style.width = 40 + 'px';
        this.canvas.style.height = 40 + 'px';
        this.canvas.style.position = "absolute";
        this.canvas.style.zIndex = 30;

        panes.overlayPane.appendChild(this.imgP);
        panes.overlayPane.appendChild(this.imgM);
        panes.overlayPane.appendChild(this.canvas);

        // add a viewreset event listener for updating layer's position, do the latter
        map.on('viewreset', this._reset, this);
        this._reset();
        setTimeout(Ext.Function.bind(this.recalc, this), 300);
    },

    onRemove: function (map) {
        map.getPanes().overlayPane.removeChild(this.imgP);
        map.getPanes().overlayPane.removeChild(this.imgM);
        map.getPanes().overlayPane.removeChild(this.canvas);
        this.repositoryTitle.dispose();
        $App.Map.Map.off('viewreset', this._reset, this);
    },

    _reset: function () {
        var sw = this._map.latLngToLayerPoint(this.p2);
        var div = this.canvas;
        div.style.left = (sw.x - 20) + 'px';
        div.style.top = (sw.y - 20) + 'px';
        this.repositoryTitle.setLocation(sw);
    },

    recalc: function (p, d) {
        this.shouldRecalc = true;
        this.d = d;
        if (!p) {
            p = this.p2;
        }
        this.p1 = this.p2;
        this.p2 = p;

        if (!this.p1 || !this.p2) {
            return;
        }

        if (this.p1.lat == this.p2.lat && this.p1.lng == this.p2.lng && this.a) {
            this.shouldRecalc = false;
        }
        if (this.shouldRecalc && (!this.a || (this.d && !this.d.f))) {
            this.a = this.calcDirection(this.p1.lat, this.p1.lng, this.p2.lat, this.p2.lng);
        }
        if (isNaN(this.a)) {
            this.a = 1;
        }
        this.repositoryTitle.update(this.d);
        var cContext = this.canvas.getContext('2d');
        this.canvas.setAttribute('width', 40);
        this.canvas.setAttribute('height', 40);
        cContext.translate(20, 20);
        cContext.rotate(this.a * Math.PI / 180);
        cContext.drawImage(this.img, -20, -20);
        this._reset();
    },

    calcDirection: function (Lat1, Long1, Lat2, Long2) {
        var x1, y1, y2, result;

        x1 = Math.sin((-Long2 + Long1) * Math.PI / 180);
        y1 = Math.cos(Lat2 * Math.PI / 180) * Math.tan(Lat1 * Math.PI / 180);
        y2 = Math.sin(Lat2 * Math.PI / 180) * Math.cos((-Long2 + Long1) * Math.PI / 180);
        result = Math.atan(x1 / (y1 - y2)) * 180 / Math.PI;
        if (result < 0)
            result = 360 + result;

        if (Long2 < Long1 && (Long2 > (Long1 - 180))) {
            if (result < 180)
                result = result + 180;
            if (result > 180)
                result = result - 180;
        } else if (Lat2 > Lat1) {
            result = result + 180;
        }

        return result;
    },

    centralize: function () {
        if (this.p2) {
            this._map.panTo(this.p2, { duration: 1 });
        }
    }
});

var RepositoryTitleDiv = function (p, r, c) {
    this.parent = p;
    this.crosshair = c;
    this.repository = r;
    this.marker = null;
    this.infoWindow = null;
    this.initialize = function () {
        this.data = document.createElement('div');
        this.data.style.position = "absolute";
        this.data.style.opacity = 0.5;
        this.data.onmouseenter = this.removeOpacity;
        this.data.onmouseleave = this.setOpacity;
        var table = Ext.String.format(' <table class="repository-title"><tr><td id="td_name{0}"></td></tr><tr><td id="td_time{0}"></td></tr></table>', this.repository.Id);
        this.data.innerHTML = table;
        this.parent.appendChild(this.data);
        this.name = document.getElementById('td_name' + this.repository.Id);
        this.time = document.getElementById('td_time' + this.repository.Id);
        this.name.innerHTML = this.repository.Record.get('Name');
        this.time.innerHTML = Ext.Date.format(this.repository.Record.get('EndSendTime') ? this.repository.Record.get('EndSendTime') : this.repository.Record.get('LastSendTime'), 'H:i:s');

        this.imgGps = document.createElement('img');
        this.imgGps.setAttribute('src', 'Resources/no-signal.png');
        this.imgGps.style.position = "absolute";
        this.imgGps.style.visibility = 'hidden';
        this.parent.appendChild(this.imgGps);

        this.createMarker();
    };
    this.createMarker = function () {
        var myIcon = L.icon({
            iconUrl: 'Resources/car_marker.png',
            iconAnchor: [15, 15]
        });
        this.marker = L.marker(this.repository.Location, {
            opacity: 0.0,
            icon: myIcon
        }).addTo($App.Map.Map);
        this.popup = this.marker.bindPopup(this.repository.GetInfoWindowContent())._popup;
    };
    this.update = function () {
        this.updateMarkerContent();
    };
    this.updateMarkerContent = function () {
        this.popup.setContent(this.repository.GetInfoWindowContent());
    };
    this.setLocation = function (sw) {
        this.data.style.left = (sw.x - (this.data.offsetWidth / 2)) + 'px';
        this.data.style.top = (sw.y + 20) + 'px';
        this.imgGps.style.left = (sw.x - 13) + 'px';
        this.imgGps.style.top = (sw.y - 50) + 'px';
        this.marker.setLatLng(this.repository.Location);
        this.updateMarkerContent();
    };
    this.update = function (d) {
        if (!d) return;
        var st = common.parseISO8601(d.f ? d.f : d.a);
        this.time.innerHTML = Ext.Date.format(st, 'H:i:s');
        if (d.f || !d.d) {
            this.crosshair.img = this.crosshair.imgP;
        } else {
            this.crosshair.img = this.crosshair.imgM;
        }
        this.updateGpsSignal(d);
    };
    this.updateGpsSignal = function (d) {
        if (d.i) {
            this.imgGps.style.visibility = d.i.a === true ? 'hidden' : 'visible';
        }
    };
    this.dispose = function () {
        $App.Map.Map.removeLayer(this.marker);
        this.parent.removeChild(this.data);
        this.parent.removeChild(this.imgGps);
    };
    this.setOpacity = function (t) {
        t.srcElement.style.opacity = 0.5;
    };
    this.removeOpacity = function (t) {
        t.srcElement.style.opacity = 1;
    };
};