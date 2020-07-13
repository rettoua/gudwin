L.TrackDecorator = L.Polyline.extend({
    options: {
        trackOptions: {
            showDots: false,
            gradientOfSpeed: false
        }
    },

    initialize: function (paths, options) {
        this._paths = paths.source;
        this._record = paths.record;
        var pnt = [];
        for (var i = 0; i < this._paths.length; i++) {
            if (i == this._paths.length - 1) {
                break;
            }
            var l = new L.LatLng(this._paths[i].b, this._paths[i].c);
            l.description = this._paths[i];
            pnt.push(l);
        }
        L.Polyline.prototype.initialize.call(this, pnt, options);
        L.Util.setOptions(this, options);
        this._map = null;
    },

    redraw: function () {
        this._initPath();
    },

    onAdd: function (map) {
        map.constructor.prototype.latLngToLayerPoint = this.latLngToLayerPoint;
        L.Polyline.prototype.onAdd.call(this, map);
    },

    onRemove: function (t) {
        if (this.markers) {
            for (var i = 0; i < this.markers.length; i++) {
                //this._unbindMarkerPopup(this.markers[i]);
                $App.Map.Map.removeLayer(this.markers[i]);
            }
        }
        L.Polyline.prototype.onRemove.call(this, t);        
    },

    latLngToLayerPoint: function (t) {
        var e = this.project(L.latLng(t))._round();
        var r = e._subtract(this.getPixelOrigin());
        if (!r.description) {
            var z;
        }
        r.description = t.description;
        return r;
    },

    _pointToString: function (p) {
        return p.x + ',' + p.y + ' ';
    },

    _updatePath: function () {
        this._map && (this._clipPoints(), this._simplifyPoints());
        var g = $(this._container),
        defs = $(this._defs);
        g.children().remove();
        defs.children().remove();
        ////if (this.buildWithDelay) {
        //var me = this;
        //setTimeout(function () { me._updatePathWitDelay(); }, 1);
        ////    this.buildWithDelay = false;
        ////}
        ////else {
        this._updatePathWitDelay();
        //}
    },

    _updatePathWitDelay: function () {
        if (this.markers) {
            for (var i = 0; i < this.markers.length; i++) {
                //this._unbindMarkerPopup(this.markers[i]);
                $App.Map.Map.removeLayer(this.markers[i]);
            }
        }

        this.markers = [];

        for (var k = 0; k < this._parts.length; k++) {
            var p = this._parts[k];
            for (var i = 0; i < p.length; i++) {
                if (i == p.length - 1) {
                    break;
                }
                var point = p[i],
                    color = this._getColor(point.description);
                var innerCircle = this._createElement("circle");
                innerCircle.setAttribute("cx", point.x);
                innerCircle.setAttribute("cy", point.y);
                innerCircle.setAttribute("r", 1);
                innerCircle.setAttribute("fill", color);

                var outerCircle = this._createElement("circle");
                outerCircle.setAttribute("cx", point.x);
                outerCircle.setAttribute("cy", point.y);
                outerCircle.setAttribute("r", 3);
                outerCircle.setAttribute("fill", 'white');
                outerCircle.setAttribute("stroke", color);
                outerCircle.setAttribute("stroke-width", "2");

                this._container.appendChild(outerCircle);
                this._container.appendChild(innerCircle);
                if (point.description) {
                    var m = this._createPointMarker(point.description);
                    m.addTo($App.Map.Map);
                    this.markers.push(m);
                }
            }
        }
    },

    _getColor: function (item) {
        return Views.ViewRepositoryManager.GetTrackingPointColor(item, this._record);
    },

    _createPointMarker: function (item) {
        var m = L.marker(new L.LatLng(item.b, item.c), {
            icon: L.icon({
                iconUrl: "Resources/empty_marker.png",
                iconAnchor: [4, 4],
                popupAnchor: [-0, -9]
            })
        }).bindPopup(this._createPointPopupByOption(item));;
        this._bindMarkerPopup(m);
        return m;
    },

    _bindMarkerPopup: function (marker) {
        marker.on('mouseover', function (e) {
            this.openPopup();
        });
        marker.on('mouseout', function (e) {
            this.closePopup();
        });
    },

    _unbindMarkerPopup: function (marker) {
        marker.off('mouseover', function (e) {
            this.openPopup();
        });
        marker.off('mouseout', function (e) {
            this.closePopup();
        });
    },



    _createPointPopupByOption: function (item) {
        var f = '<b>Время:</b> ' + Ext.Date.format(common.parseISO8601(item.a), 'd-m-Y H:i:s') + ' <br /> <b>Скорость:</b> ' + item.d + ' км/час <br /><b>Координаты:</b> ' + item.b + ' x ' + item.c;
        return f;
    }
});

L.trackDecorator = function (paths, options) {
    return new L.TrackDecorator(paths, options);
};
