var RPI;
var TM;
var MAP;
var ActualTime;
var Relay;
var $App;

var main_start = function () {
    $App = new Core.Application();
    $App.LoadSettings();
    $App.CreateMap('TabPanel1-body');
    $App.Start();
    parent.$App = $App;
};

var SetActualTime = function (object) {
    if (object.Date) {
        ActualTime = common.parseISO8601(object.Date);
    }
};
var getWeight = function () {
    return 2;
    if (globalUserSettings && globalUserSettings.weight) {
        return Math.min(globalUserSettings.weight, 3);
    }
    return 3;
};
var GetActualTime = function () {
    return ActualTime ? ActualTime : new Date();
};

var init_repo_manager = function (grid) {
    $App.RepositoryManager.LoadRecords(grid);
    //RPI = new repo(grid);
    //RPI.initialize();
    TM = new TrackingManager(grid);
    TM.initialize();
};

var repo = function (g) {
    this.gp = g;
    this.repositories = {};
    this.initTimer = null;

    this.initialize = function () {
        this.createRepositories();
        this.initTimer = setInterval(Ext.Function.bind(this.refreshRepository, this), 4000);
        this.loadUnreachableTrackers();
    },
    this.getRepository = function (k) {
        return this.repositories[k];
    };
    this.renderRelay = function (value, meta, record, index) {
        if (!value || value.a === false) {
            return '<img src="Resources/un_r.png" />';
        }
        if (value.s == undefined) {
            return '<img src="Resources/w_r.png" />';
        }
        if (value.s === true) {
            return '<img src="Resources/on_r.png" />';
        }
        return '<img src="Resources/off_r.png" />';
    };
};

var TrackingManager = function (g) {
    this.tracks = {};
    this.trackSpeed = {};
    this.gp = g;
    this.combo = null;
    this.comboSpeed = null;
    this.currentTrack = null;
    this.currentTrackSpeed = null;
    this.initialize = function () {
        this.initTrackCombo();
    };
    this.initTrackCombo = function () {
        this.combo = App.carsForTracking;
        var r = this.gp.store.getAllRange();
        var firstIndex = 0;
        for (var i = 0; i < r.length; i++) {
            this.tracks[r[i].internalId] = new Track(r[i]);
            this.combo.addItem(r[i].get('Name'), r[i].internalId);
            if (firstIndex == 0)
                firstIndex = r[i].internalId;
        }
        this.combo.setValue(firstIndex);
    };
    this.load = function () {
        if (!App.dfFrom.getValue() || !App.carsForTracking.getValue()
            || !App.dfTo.getValue() || !App.tfFrom.getValue() || !App.tfTo.getValue()) {
            Ext.Msg.alert('Фильтр истории движения', 'Введите все данные для фильтрации');
            return;
        }
        if (App.dfTo.getValue() - App.dfFrom.getValue() < 0) {
            Ext.Msg.alert('Фильтр истории движения', 'Начальная дата должна быть меньше. Выберите повторно диапазон дат.');
            return;
        }
        if (Math.floor((((App.dfTo.getValue() - App.dfFrom.getValue()) / 1000) % 31536000) / 86400) > 2) {
            Ext.Msg.alert('Фильтр истории движения', 'Максимальный период истории движения - 2 дня <br />за любые последние 30 календарных дней.');
            return;
        }
        this.showMask();
        this.currentTrack = this.tracks[App.carsForTracking.getValue()];
        this.loadTrack(App.carsForTracking.getValue(),
                        App.dfFrom.getValue(),
                        App.dfTo.getValue(),
                        App.tfFrom.getValue(),
                        App.tfTo.getValue());
    };
    this.loadTrack = function (track, df, dt, tf, tt) {
        var t = this;
        DR.LoadTrack(track, df, dt, tf, tt, {
            success: function (obj) {
                if (!obj || obj == 'null') {
                    t.resultNovalue();
                    return;
                }
                t.tracks[track].setFilter(df, dt, tf, tt);
                t.dispalyTrack(track, obj);
            },
            failure: function (error) {
                t.errorWhileLoading(error);
            }
        });
    };
    this.resultNovalue = function () {
        this.hideMask();
    };
    this.errorWhileLoading = function (error) {
        this.hideMask();
    };
    this.dispalyTrack = function (track, obj) {
        this.tracks[track].setSource(obj);
        this.tracks[track].display();
    };
    this.showMask = function () {
        App.windowTrack.body.mask('Загрузка маршрута...');
    };
    this.hideMask = function () {
        App.windowTrack.body.unmask();
    };
    this.trackHover = function (id) {
        if (this.currentTrack) {
            var me = this;
            var obj = {
                func: function () {
                    me.currentTrack.hover(id);
                }
            };
            setTimeout(function () { obj.func(); }, 50);
        }
    };
    this.trackLeave = function (id) {
        if (this.currentTrack != null) {
            var me = this;
            var obj = {
                func: function () {
                    me.currentTrack.leave(id);
                }
            };
            setTimeout(function () { obj.func(); }, 50);
        }
    };
    this.trackClick = function (id) {
        if (this.currentTrack) {
            this.currentTrack.click(id);
        }
    };
    this.trackChanged = function () {
        if (this.currentTrack) {
            this.currentTrack.hide();
        }
        this.currentTrack = this.tracks[App.carsForTracking.getValue()];
        if (this.currentTrack)
            this.currentTrack.show();
    };
    this.clear = function () {
        if (this.currentTrack) {
            this.currentTrack.clear();
            this.currentTrack.trackSource = [];
            this.currentTrack = null;
        }
    };
    this.toggleParking = function () {
        if (this.currentTrack) {
            if (this.getToggleState()) {
                this.currentTrack.showMarkers();
            } else {
                this.currentTrack.hideMarkers();
            }
        }
        Ext.get(App.btnTrackerSetting.btnIconEl).toggleCls('inactive');
    };
    this.togglePoints = function () {
        if (this.currentTrack) {
            if (this.getTogglePointState()) {
                this.currentTrack.showPoints();
            } else {
                this.currentTrack.hidePoints();
            }
        }
        Ext.get(App.btnShowPoints.btnIconEl).toggleCls('inactive');
    };
    this.toggleSpeed = function () {
        if (this.currentTrack) {
            if (this.getToggleSpeedState()) {
                this.currentTrack.showSpeed();
            } else {
                this.currentTrack.hideSpeed();
            }
        }
        Ext.get(App.btnShowSpeed.btnIconEl).toggleCls('inactive');
    };
    this.getToggleState = function () {
        return App.btnTrackerSetting.pressed;
    };
    this.getTogglePointState = function () {
        return App.btnShowPoints.pressed;
    };
    this.getToggleSpeedState = function () {
        return App.btnShowSpeed.pressed;
    };
    this.changeParkingLength = function (item, newValue) {
        item.setNote('остановка: ' + newValue + ' мин.');
    };
    this.changeCompleteParkingLength = function () {
        if (this.currentTrack && this.currentTrack.trackSource) {
            this.currentTrack.display();
        }
    };
    this.getParkingLength = function () {
        return App.sliderParkingLength.getValue();
    };
};

var Track = function (record) {
    this.record = record;
    this.trackSource = null;
    this.multiPolylines = [];
    this.summary = {
        speeds: [], distance: 0, ticks: 0
    };
    this.items = [];
    this.filter = {
    };
    this.speedPolyline = [];
    this.markerGroup = null;
    this.polylinesDecorators = [];
    this.markers = [];
    this.pointsGroup = null;
    this.points = [];
    this.setSource = function (s) {
        delete this.trackSource;
        this.trackSource = s;
    };
    this.setFilter = function (df, dt, tf, tt) {
        this.filter = {
            df: df, dt: dt, tf: tf, tt: tt
        };
    };
    this.refreshFilter = function () {
        App.dfFrom.setValue(this.filter.df);
        App.dfTo.setValue(this.filter.dt);
        App.tfFrom.setValue(this.filter.tf);
        App.tfTo.setValue(this.filter.tt);
    };
    this.resetFilter = function () {
        App.dfFrom.setValue(new Date());
        App.dfTo.setValue(new Date());
        App.tfFrom.setValue('0:00');
        App.tfTo.setValue('23:59');
    };
    this.display = function () {
        if (!this.trackSource) {
            return;
        }
        this.clear();
        var singlePolyline = [];

        var option = this.createOptions();
        var prevItem;
        for (var i = 0; i < this.trackSource.length; i++) {
            var item = this.trackSource[i];
            if (item.f) {
                if (!this.canMarkAsParking(item)) {
                    continue;
                }
                option.start = common.parseISO8601(item.a);
                option.end = common.parseISO8601(item.f);
                option.geo = item.g;
                option.item = item;
                this.buildOptionAndSummary(option);
                this.items.push(option);
                if (singlePolyline.length == 0) {
                    singlePolyline.push(new L.LatLng(item.b, item.c));
                    singlePolyline.speed = Views.ViewRepositoryManager.GetSpeedIndex(item, this.record);
                }
                this.array.push(singlePolyline);
                this.markers.push(this.createMarker(option, item));
                singlePolyline = [];
                prevItem = null;
                option = this.createOptions();
            } else {
                if (item.d == 0) {
                    continue;
                }
                if (!option.movingStart) {
                    option.movingStart = common.parseISO8601(item.a);
                }
                var index = Views.ViewRepositoryManager.GetSpeedIndex(item, this.record);
                if (prevItem && prevItem == index) {
                    singlePolyline.push(new L.LatLng(item.b, item.c));
                } else {
                    if (!singlePolyline.speed) {
                        singlePolyline.speed = index;
                    }
                    if (singlePolyline.length == 1) {
                        singlePolyline.push(new L.LatLng(item.b, item.c));
                    } else {
                        if (singlePolyline.length == 0) {
                            singlePolyline.push(new L.LatLng(item.b, item.c));
                            singlePolyline.speed = index;
                        } else {
                            var last = singlePolyline[singlePolyline.length - 1];
                            this.array.push(singlePolyline);
                            singlePolyline = [];
                            singlePolyline.speed = index;
                            singlePolyline.push(last);
                            singlePolyline.push(new L.LatLng(item.b, item.c));
                        }
                    }
                }
                prevItem = index;
                option.speeds.push(item.d);
                option.distance += item.h;
                option.values.push(new L.LatLng(item.b, item.c));
            }
        }
        if (this.array.length == 0 || singlePolyline.length > 0) {
            if (this.trackSource && this.trackSource.length > 0) {
                option.start = common.parseISO8601(this.getFirstCorrectItem().a);
                option.end = common.parseISO8601(this.trackSource[this.trackSource.length - 1].a);
                this.array.push(singlePolyline);
                this.buildOptionAndSummary(option);
                this.items.push(option);
            }
        }
        for (var j = 0; j < this.array.length; j++) {
            var polyline = L.polyline(this.array[j], {
                color: Views.ViewRepositoryManager.GetColorByIndex(this.array[j].speed, this.record),
                opacity: 1,
                weight: getWeight(),
                lineJoin: 'round'
            });
            polyline.c = Views.ViewRepositoryManager.GetColorByIndex(this.array[j].speed, this.record);
            polyline.addTo($App.Map.Map);
            this.multiPolylines.push(polyline);
        }

        var index = -1;
        for (var k = 0; k < this.multiPolylines.length; k++) {
            var layer = this.multiPolylines[k];
            if (!layer._latlngs || layer._latlngs.length <= 1) {
                continue;
            }
            layer.decorator = L.polylineDecorator(layer, {
                patterns: [
                    {
                        opacity: 1, offset: 1, repeat: '70px', symbol: new L.Symbol.arrowHead({
                            pixelSize: 8, headAngle: 35,
                            pathOptions: {
                                fillOpacity: 1,
                                opacity: 1,
                                weight: 2,
                                color: layer.c
                            }
                        })
                    }
                ]
            });
            this.polylinesDecorators.push(layer.decorator);
            layer.decorator.addTo($App.Map.Map);
        }

        this.createBoundsMarker();
        this.markerGroup = L.featureGroup(this.markers);
        if (TM.getToggleState()) {
            this.markerGroup.addTo($App.Map.Map);
        }
        this.showPointGroup();
        //this.bindOptionToLayer();
        this.prepareSummary();
        this.renderTemplate(this.items, this.summary);
        for (var l = 0; l < this.items.length; l++) {
            Core.GeoLocation.updateLocation(this.items[l].item, l);
        }
        TM.hideMask();
    };
    this.getFirstCorrectItem = function () {
        for (var i = 0; i < this.trackSource.length; i++) {
            if (this.trackSource[i].b != 0) {
                return this.trackSource[i];
            }
        }
    };
    this.showPointGroup = function () {
        if (!TM.getTogglePointState()) {
            return;
        }
        this.pointsGroup = L.trackDecorator({ source: this.trackSource, record: this.record }, {});
        this.pointsGroup.addTo($App.Map.Map);
    };
    this.showPolylineDecorators = function () {
        if (this.polylinesDecorators && this.polylinesDecorators.length > 0) {
            for (var i = 0; i < this.polylinesDecorators.length; i++) {
                this.polylinesDecorators[i].addTo($App.Map.Map);
            }
        }
    };
    this.createMarker = function (option, item) {
        var m = L.marker(new L.LatLng(item.b, item.c), {
            icon: L.icon({
                iconUrl: 'Resources/parking.jpg',
                iconAnchor: [12, 30],
                popupAnchor: [0, -30]
            })
        }).bindPopup(this.createPopupByOption(option));
        this.bindMarkerPopup(m);
        return m;
    };
    this.createPointMarker = function (option, item) {
        var m = L.marker(new L.LatLng(item.b, item.c), {
            icon: L.icon({
                iconUrl: 'Resources/dot.png',
                iconAnchor: [4, 5],
                popupAnchor: [-0, -9]
            })
        }).bindPopup(this.createPointPopupByOption(item));;
        this.bindMarkerPopup(m);
        return m;
    };
    this.bindMarkerPopup = function (marker) {
        marker.on('mouseover', function (e) {
            this.openPopup();
        });
        marker.on('mouseout', function (e) {
            this.closePopup();
        });
    };
    this.createBoundsMarker = function () {
        if (!this.array || this.array.length == 0) {
            return;
        }
        var a = this.array[0];
        var s = a[0];
        var optionStart = this.items[0];
        var marker = L.marker(s, {
            icon: L.icon({
                iconUrl: 'Resources/start_end.png',
                iconAnchor: [12, 30],
                popupAnchor: [0, -30]
            })
        }).bindPopup(this.createBoundPopupByOption(optionStart.startReadable, 'Начало движения'));
        this.bindMarkerPopup(marker);
        this.markers.push(marker);

        var e = this.array[this.array.length - 1];
        var end = e[e.length - 1];
        var optionEnd = this.items[this.items.length - 1];
        var marker2 = L.marker(end, {
            icon: L.icon({
                iconUrl: 'Resources/start_end.png',
                iconAnchor: [12, 30],
                popupAnchor: [0, -30]
            })
        }).bindPopup(this.createBoundPopupByOption(optionEnd.endReadable, 'Конец движения'));
        this.bindMarkerPopup(marker2);
        this.markers.push(marker2);
    };
    this.createPopupByOption = function (option) {
        var f = '<b>Остановка:</b> ' + option.time + ' <br /> <b>c</b> ' + option.startReadable + ' <b>до</b> ' + option.endReadable;
        return f;
    };
    this.createPointPopupByOption = function (option) {
        var f = '<b>Время:</b> ' + Ext.Date.format(common.parseISO8601(option.a), 'd-m-Y H:i:s') + ' <br /> <b>Скорость:</b> ' + option.d + ' км/час <br /><b>Координаты:</b> ' + option.b + ' x ' + option.c;
        return f;
    };
    this.createBoundPopupByOption = function (s, w) {
        var f = '<b>' + w + ':</b>  ' + s;
        return f;
    };
    this.updateBar = function (value, pbar, count, callback, v) {
        if (value >= count) {
            pbar.updateProgress(0, 'Загрузка координат. Подождите...');
            callback();
        } else {
            pbar.updateProgress(value / count, 'Отображение координат ' + value + ' из ' + (count - 1) + '...');
            this.poly.addLatLng(new L.LatLng(v.b, v.c));
        }
    };
    this.clear = function () {
        this.clearPolylines();
        this.clearSpeedPolylines();
        this.clearMarkers();
        this.clearPoints();
        this.clearPolylineDecorators();
        this.array = [];
        this.arraySpeed = [];
        delete this.markers;
        this.markers = [];
        this.points = [];
        this.renderTemplate([]);
        this.items = [];
        this.multiPolylines = [];
        this.polylinesDecorators = [];
        this.summary = {
            speeds: [], distance: 0, ticks: 0
        };
        this.markerGroup = null;
        this.pointsGroup = null;
    };
    this.clearPolylines = function () {
        if (this.multiPolylines) {
            for (var i = 0; i < this.multiPolylines.length; i++) {
                $App.Map.Map.removeLayer(this.multiPolylines[i]);
            }
        }
    };
    this.clearSpeedPolylines = function () {
        if (this.speedPolyline) {
            for (var i = 0; i < this.speedPolyline.length; i++) {
                $App.Map.Map.removeLayer(this.speedPolyline[i]);
            }
        }
    };
    this.clearMarkers = function () {
        if (this.markerGroup) {
            $App.Map.Map.removeLayer(this.markerGroup);
        }
    };
    this.clearPoints = function () {
        if (this.pointsGroup) {
            $App.Map.Map.removeLayer(this.pointsGroup);
            this.pointsGroup = null;
        }
    };
    this.clearPolylineDecorators = function () {
        if (this.polylinesDecorators) {
            for (var i = 0; i < this.polylinesDecorators.length; i++) {
                $App.Map.Map.removeLayer(this.polylinesDecorators[i]);
            }
            this.polylinesDecorators = null;
        }
    };
    this.createOptions = function () {
        var options = {
            speeds: [],
            distance: 0,
            values: []
        };
        return options;
    };
    this.buildOptionAndSummary = function (option) {
        option.avgSpeed = this.getAvg(option.speeds);
        option.maxSpeed = this.getMax(option.speeds);
        if (option.movingStart) {
            option.movingStartReadable = Ext.Date.format(option.movingStart, 'H:i:s');
        }
        if (option.end) {
            option.time = common.timeToHumanReadable((option.end - option.start) / 1000);
            this.summary.ticks += option.end - option.start;
            option.startReadable = Ext.Date.format(option.start, 'H:i:s');
            option.endReadable = Ext.Date.format(option.end, 'H:i:s');
        }
        option.distanceReadable = common.distanceReadable(option.distance);
        this.summary.distance += option.distance;
        this.summary.speeds = this.summary.speeds.concat(option.speeds);
    };
    this.prepareSummary = function () {
        this.summary.avgSpeed = this.getAvg(this.summary.speeds);
        this.summary.maxSpeed = this.getMax(this.summary.speeds);
        this.summary.time = common.timeToHumanReadable(this.summary.ticks / 1000);
        this.summary.distanceReadable = common.distanceReadable(this.summary.distance);
    };
    this.getAvg = function (array) {
        if (!array || array.length == 0) return 0;
        return Math.floor(array.reduce(function (a, b) { return a + b; }) / array.length);
    };
    this.getMax = function (array) {
        if (!array || array.length == 0) return 0;
        return Math.max.apply(null, array);
    };
    this.renderTemplate = function (values, summary) {
        for (var i = 0; i < values.length; i++) {
            values[i].id = i;
        }
        App.XTemplateHistory.overwrite(App.pnlTrackTemplate.body, values);
        App.XTemplateSummary.overwrite(App.pnlTrackSummary.body, summary);
    };
    this.canMarkAsParking = function (item) {
        var s = common.parseISO8601(item.a);
        var e = common.parseISO8601(item.f);
        var diff = (e - s) / 1000;
        var secs = 60 * TM.getParkingLength();
        return diff > secs;
    };
    this.hover = function (id) {
        if (this.items[id].polyline) {
            this.items[id].polyline.addTo($App.Map.Map);
            this.items[id].polyline1.addTo($App.Map.Map);

        } else {
            var values = this.items[id] && this.items[id].values;
            if (values && values.length) {
                var polyline = L.polyline(values, {
                    color: "black",
                    opacity: 1,
                    weight: 5,
                    lineJoin: 'round'
                });
                var polyline1 = L.polyline(values, {
                    color: "white",
                    opacity: 1,
                    weight: 4,
                    lineJoin: 'round'
                });
                this.items[id].polyline = polyline;
                this.items[id].polyline1 = polyline1;
                this.items[id].polyline.addTo($App.Map.Map);
                this.items[id].polyline1.addTo($App.Map.Map);
            }
        }
    };
    this.leave = function (id) {
        if (this.items[id].polyline) {
            $App.Map.Map.removeLayer(this.items[id].polyline);
            $App.Map.Map.removeLayer(this.items[id].polyline1);
        }
    };
    this.click = function (id) {
        var values = this.items[id] && this.items[id].values;
        if (values && values.length) {
            $App.Map.Map.panTo(values[0], { duration: 1 });
        }
    };
    this.show = function () {
        if (!this.displayed()) {
            this.resetFilter();
            return;
        }
        if (this.multiPolylines) {
            for (var i = 0; i < this.multiPolylines.length; i++) {
                this.multiPolylines[i].addTo($App.Map.Map);
            }
        }
        if (this.markerGroup && TM.getToggleState()) {
            this.markerGroup.addTo($App.Map.Map);
        }
        this.renderTemplate(this.items, this.summary);
        this.showPointGroup();
        this.showPolylineDecorators();
        this.refreshFilter();
    };
    this.hide = function () {
        if (!this.displayed()) return;
        this.clearPolylines();
        this.clearMarkers();
        this.clearPoints();
        this.clearPolylineDecorators();
        this.renderTemplate([], []);
    };
    this.displayed = function () {
        return this.trackSource && this.multiPolylines;
    };
    this.showMarkers = function () {
        if (this.markers && this.markers.length > 0) {
            this.markerGroup.addTo($App.Map.Map);
        }
    };
    this.hideMarkers = function () {
        if (this.markers && this.markers.length > 0) {
            this.clearMarkers();
        }
    };
    this.showPoints = function () {
        if (!this.pointsGroup) {
            this.showPointGroup();
        }
    };
    this.hidePoints = function () {
        if (this.pointsGroup) {
            this.clearPoints();
        }
    };
    this.showSpeed = function () {
        this.display();
    };
    this.hideSpeed = function () {
        this.display();
    };
};

var initializeRelay = function () {
    if (!Relay) {
        Relay = new relay();
    }
    Relay.showWindow();
    Relay.loadData();
};

var relay = function () {
    this.selectedTracker = null;
    this.showWindow = function () {
        this.getWnd().show();
        //this.showMask();
    };
    this.hideMask = function () {
        var wnd = this.getWnd();
        wnd.body.unmask();
    };
    this.showMask = function () {
        var wnd = this.getWnd();
        wnd.body.mask('Загрузка реле...');
    };
    this.showExecuteMask = function () {
        var wnd = this.getWnd();
        wnd.body.mask('Отправка данных...');
    };
    this.hideExecuteMask = function () {
        var wnd = this.getWnd();
        wnd.body.unmask();
    };
    this.loadData = function () {
        var trackers = this.getActiveTrackers();
        if (trackers.length == 0) {
            this.showNoTrackersPanel();
            this.hideTrackerGridPanel();
        } else {
            this.showTrackerGridPanel();
            this.hideNoTrackersPanel();
            var grid = this.getWndTrackerTrackerRelay();
            grid.store.loadData(trackers);
        }
    };
    this.getActiveTrackers = function () {
        var repos = RPI.repositories,
            recs = [];
        for (var id in repos) {
            var r = repos[id].record;
            if (r.record.ia === true) {
                if (r.data.Relay || r.data.Relay1 || r.data.Relay2 || r.data.Sensor1 || r.data.Sensor2) {
                    recs.push(repos[id].record.data);
                }
            }
        }
        return recs;
    };
    this.showNoTrackersPanel = function () {
        var pnl = this.getWndNoTracker();
        pnl.show();

    };
    this.hideTrackerGridPanel = function () {
        var grid = this.getWndTrackerTrackerRelay();
        grid.hide();
    };
    this.showTrackerGridPanel = function () {
        var grid = this.getWndTrackerTrackerRelay();
        grid.show();
    };
    this.hideNoTrackersPanel = function () {
        var grid = this.getWndNoTracker();
        grid.hide();
    };
    this.getWnd = function () {
        return App.wndRelay;
    };
    this.getWndNoTracker = function () {
        return App.pnlNoActiveTrackers;
    };
    this.getWndTrackerTrackerRelay = function () {
        return App.gridPanelTrackerRelay;
    };
    this.getUnvPanel = function () {
        return App.pnlUnavailableRelay;
    };
    this.relayRenderer = function (value, meta, record, index) {
        var name = value.n;
        if (value && value.a === true) {
            if (value.s === true) {
                meta.style = 'background: #82f879;';
            }
            else if (value.s === false) {
                meta.style = 'background: #ff5757;';
            } else {
                meta.style = 'background: #bbd6fb;';
            }
        } else {
            return '<span style="color: grey;">не подключен</span>';
        }
        return Ext.String.format('<span style="font-width: bold;">{0}</span>', name);
    };
    this.sensorRenderer = function (value, meta, record, index) {
        var name = value.n ? value.n : 'Вход';
        if (value && value.a === true) {
            if (value.s === true) {
                meta.style = 'background: #82f879;';
            }
            else if (value.s === false) {
                meta.style = 'background: #ff5757;';
            } else {
                meta.style = 'background: #bbd6fb;';
            }
        } else {
            return '<span style="color: grey;">не подключен</span>';
        }
        return Ext.String.format('<span style="font-width: bold;">{0}</span>', name);
    };
    this.relayCommandPrepare = function (grid, command, record, row, column, value) {
        if (!value || value.a === false) {
            command.hidden = true;
            return;
        }
        command.hidden = false;
        command.disabled = false;
        if (value.s === true) {
            command.command = 'off';
            command.iconCls = 'icon-decline';
            command.text = 'Выкл';
        } else if (value.s === false) {
            command.command = 'on';
            command.iconCls = 'icon-accept';
            command.text = 'Вкл';
        } else {
            command.command = 'inactive';
            command.disabled = true;
        }
    };
    this.executeRelay = function (column, command, record, cellIndex) {
        var obj = record.data[column.dataIndex];
        if (!RPI.signalRConnected()) {
            Ext.Msg.alert('Ошибка выполения', 'Невозможно выполнить действие, потеряно соединение с сервером. Повторите попытку.');
            return;
        }
        this.showExecuteMask();
        var func = command == 'off' ? 'turnOffRelay' : 'turnOnRelay';
        try {
            var trackerId = record.data.Id;
            var hub = RPI.getHub();
            hub.server[func](trackerId, obj.id).done(function () {
                Relay.hideExecuteMask();
                Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
            }).fail(function (error) {
                Relay.hideExecuteMask();
                Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
            });;
        } catch (e) {
            Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
        }
    };
};