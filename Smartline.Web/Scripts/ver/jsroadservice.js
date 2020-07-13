var imgObj = null;
var road;

function initialize() {
    road = new RoadService(document.getElementById("TabPanel1-body"));
    road.initialize();
};

function markerclick(uid) {
    road.setById(uid);
};

function carpathclick(uid) {
    road.centralizeFlyCar(uid);
};

function addclick(uid) {
    road.additional(uid);
};

function insert(grid) {
    grid.addRecord({});
    grid.getSelectionModel().selectRow(1);
    grid.getView().focusRow(1);
};

function dateFilterChanged() {
    if (dtFromFilter.getValue() != '' && dtToFilter.getValue() != '') {
        btnShowPath.setDisabled(false);
    } else {
        btnShowPath.setDisabled(true);
    }
    CompositeFieldTime.setVisible(dtFromFilter.value == dtToFilter.value);
};

var RoadService = function (div) {
    this.div = div;
    this.cars = {};
    this.flyPanel = null;
    this.initTimer = null;
    var directionsDisplay;
    var map;

    this.initialize = function () {
        directionsDisplay = new google.maps.DirectionsRenderer();
        var myOptions = {
            center: new google.maps.LatLng(50.450126, 30.523925),
            zoom: 12,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        map = new google.maps.Map(div,
            myOptions);

        directionsDisplay.setMap(map);        
//        this.initTimer = setInterval(this.setContainer.createDelegate(this), 1000);        
    },

    this.setContainer = function () {
        var store = cars_store;
        if (!store.isLoaded) return;
        store.each(function (r) {
            var el = document.getElementById('car-list-' + r.id);
            if (el) {
                if (this.cars[r.id] == null) {
                    this.hideLoadWindow();
                }
                else return;
                var newCar = new Car(r, this, el);
                this.cars[newCar.__id] = newCar;
            }
        }, this);

        if (store.totalLength == 0) {
            this.hideLoadWindow();
        }
    },

    this.hideLoadWindow = function () {
        clearInterval(this.initTimer);
        this.initTimer = 0;
        wndwLoading.setVisible(false);
    },

    this.getMap = function () {
        return map;
    };

    this.centerById = function (uid, record) {
        if (this.cars[uid] != undefined) {
            map.setCenter(this.cars[uid].getLastPoint());
            return true;
        }
        else {
            map.setCenter(new google.maps.LatLng(record.data.Latitude, record.data.Longitude));
        }
        return false;
    },

    this.addCar = function (id, node) {
        var car = new Car(id, map, node);
        car.addEvent("onload", onloadcar);
        car.init();
    },

    this.updateCarPath = function (id) {
        if (this.cars[id] != undefined) {
            var car = this.cars[id];
            if (!car.__el.runActive) {
                car.run();
                NodeStl.activePath(car.__el);
            } else {
                car.stop();
                NodeStl.deActivePath(car.__el);
            }
        }
    },

    this.setById = function (uid) {
        var result = this.cars[uid];
        if (!result.__el.selected) {
            this.flyPanel.add(result);
            NodeStl.select(result.__el);
        } else {
            NodeStl.deSelect(result.__el);
            this.flyPanel.removeFlyCar(result.__id);
        }
    };

    this.centralizeFlyCar = function (carid) {
        this.flyPanel.setCenter(carid);
    };

    function onloadcar(parameters) {
        var newCar = parameters.target;
        if (Ext.isDefined(newCar)) {
            var carMarker = newCar.getMarker();
            carMarker.setMap(map);
            cars[newCar.id] = newCar;
        }
    };

    this.getMap = function () {
        return map;
    };

    this.additional = function (uid) {
//        wndCar.show();
//        return;
        var result = this.cars[uid];
        if (!result.__el.additional) {
            var addCar = this.getAddCar();
            if (addCar) {
                NodeStl.addCollapse(addCar);
            }
            NodeStl.addExpand(result);
        }
        else {
            NodeStl.addCollapse(result);
        }
    };

    this.getAddCar = function () {
        for (var c in this.cars) {
            if (this.cars[c].__el.additional) {
                return this.cars[c];
            }
        }
        return null;
    };

    this.showPath = function () {
        var addCar = this.getAddCar();
        var from = dtFromFilter;
        var to = dtToFilter;

        var tFrom = '', tTo = '';
        if (from.value == to.value) {
            tFrom = timeFromFilter.value == undefined ? '' : timeFromFilter.value;
            tTo = timeToFilter.value == undefined ? '' : timeToFilter.value;
        }

        addCar.showPath(from.value, to.value, tFrom, tTo);
    };

    this.changeMarkerPathVisibility = function () {
        var addCar = this.getAddCar();
        addCar.setPathVisibility();
    };

    this.changeTimePathVisibility = function () {
        var addCar = this.getAddCar();
        addCar.setTimeVisibility();
    };
};  