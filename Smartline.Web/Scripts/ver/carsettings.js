var carSettings = function () {
    this.record = null;
    this.getWindow = function () {
        return App.wndCarSettings;
    };
    this.getTrackerIdLabel = function () {
        return App.lblTrackerId;
    };
    this.getTrackerNameTxt = function () {
        return App.txtName;
    };
    this.getTrackerDescriptionTxt = function () {
        return App.txtDescription;
    };
    this.getTrackerConsumptionField = function () {
        return App.fldConsumption;
    };
    this.getTrackerEvosChk = function () {
        return App.chkHideInEvos;
    };
    this.getTrackerColorField = function () {
        return App.fldColor;
    };
    this.getVirtualTrackerButton = function () {
        return App.btnVirtualTracker;
    };
    this.getTrackerOdometer = function () {
        return App.fldOdometer;
    };
    this.getTrackerOdometerLbl = function () {
        return App.lblOdometer;
    };
    this.show = function (record) {
        this.showWindow();
        this.mask();
        this.load(record.data.TrackerId);
    };
    this.showWindow = function () {
        var wnd = this.getWindow();
        wnd.show();
    };
    this.mask = function () {
        var wnd = this.getWindow();
        wnd.body.mask('Загрузка данных...');
    };
    this.unmask = function () {
        var wnd = this.getWindow();
        wnd.body.unmask();
    };
    this.load = function (trackerId) {
        this.setOdometerValue(null);
        var me = this;
        DR.GetTracker(trackerId, globalUserSettings.i, {
            success: function (tracker) {
                me.record = tracker;
                me.display(tracker);
                me.loadOdometer();
                me.unmask();
            },
            failure: function (e) {
                me.unmask();
            }
        });
    };
    this.display = function (tracker) {
        var lblId = this.getTrackerIdLabel(),
            txtName = this.getTrackerNameTxt(),
            txtDescription = this.getTrackerDescriptionTxt(),
            fldConsumption = this.getTrackerConsumptionField(),
            chkHideInEvos = this.getTrackerEvosChk(),
            fldColor = this.getTrackerColorField();

        lblId.el.setHTML('<b>ID трекера:</b> ' + tracker.trackerid);
        txtName.setValue(tracker.name);
        txtDescription.setValue(tracker.description);
        fldConsumption.setValue(tracker.consumption || 0);
        chkHideInEvos.setValue(tracker.hevos);
        fldColor.setValue('#' + tracker.color);
        this.applyVirtualData();
        this.displaySensorsData(tracker);
        this.displaySensors(tracker);
    };
    this.displaySensorsData = function (tracker) {
        this.displayRelayData(tracker.r, '');
        this.displayRelayData(tracker.r1, 1);
        this.displayRelayData(tracker.r2, 2);
        this.displaySensorData(tracker.s1, 1);
        this.displaySensorData(tracker.s2, 2);
    };
    this.displaySensors = function (tracker) {
        var repo = $App.RepositoryManager.Repositories[tracker.id];
        this.updateRelays(repo);
    };
    this.updateRelays = function (repo) {
        if (this.getWindow().hidden === true) { return; }
        if (!this.record || repo.Record.data.Id != this.record.id) { return; }
        this.displayRelayState(repo, '');
        this.displayRelayState(repo, 1);
        this.displayRelayState(repo, 2);
        this.displaySensorState(repo, 1);
        this.displaySensorState(repo, 2);
    };
    this.displayRelayData = function (relay, id) {
        var txt = App['txtRelay' + id];
        txt.setValue(relay.n);
    };
    this.displayRelayState = function (repo, id) {
        var btn = App['btnRelay' + id], c;
        if (repo.IsConnected === false || repo.Record.data.s == undefined) {
            c = 'grey';
        } else if (repo.Record.data.s['Relay' + id] === false) {
            c = 'red';
            btn.setText('ВКЛ');
        } else {
            c = 'greenyellow';
            btn.setText('ВЫКЛ');
        }
        btn.el.dom.style.backgroundColor = c;
        btn.setDisabled(c == 'grey');
    };
    this.displaySensorData = function (sensor, id) {
        var txt = App['txtSensor' + id];
        txt.setValue(sensor.n);
    };
    this.displaySensorState = function (repo, id) {
        var btn = App['btnSensor' + id], c;
        if (repo.IsConnected === false || repo.Record.data.s == undefined) {
            c = 'grey';
        } else if (repo.Record.data.s['Sensor' + id] === false) {
            c = 'red';
        } else {
            c = 'greenyellow';
        }
        btn.el.dom.style.backgroundColor = c;
        btn.setDisabled(c == 'grey');
    };
    this.relayClick = function (btn, id) {
        var data = $App.RepositoryManager.Repositories[this.record.id].Record.data;
        var obj = data['Relay' + id], state = data.s['Relay' + id];
        this.showExecuteMask();
        try {
            var z = true;
            z = true;
            if (z === true && $App.ServerCommunication.State == Core.ConnectionState.connected) {
                var func = state === true ? 'turnOffRelay' : 'turnOnRelay';
                var trackerId = this.record.id,
                    hub = $App.ServerCommunication._mapHub,
                    me = this;
                hub.server[func](trackerId, obj.id).done(function () {
                    Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                }).fail(function (error) {
                    Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
                });
            } else {
                var func = state === true ? 'TurnOffRelay' : 'TurnOnRelay';
                DR[func]($App.SettingsManager.Settings.UserId, this.record.id, obj.id);
                Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
            }
        } catch (e) {
            Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
        }
        finally {
            this.hideExecuteMask();
        }
    };
    this.loadOdometer = function () {
        var fldOdometer = this.getTrackerOdometer(),
            me = this;
        fldOdometer.setValue(null);
        DR.LoadOdometer(this.record.id, this.record.trackerid, {
            success: function (obj) {
                me.setOdometerValue((obj && obj.o) || 0);
            },
            failure: function (e) { }
        });
    };
    this.applyVirtualData = function () {
        var btnVirtual = this.getVirtualTrackerButton();
        btnVirtual.setText('<b>Виртуальный трекер:</b> ' + (this.record.v_trackerid && this.record.v_name ? '[' + this.record.v_trackerid + ']' + this.record.v_name : 'нет'));
    };
    this.save = function () {
        this.maskSaving();
        this.writeData();
        var me = this;
        DR.UpdateTracker(this.record, this.getOdometerValue(), globalUserSettings.i, {
            success: function (rec) {
                me.unmaskSaving();
                Ext.Msg.notify('Сохранение данных', 'Данные успешно сохранены!');
            },
            failure: function (e) {
                me.unmaskSaving();
                Ext.Msg.alert('Ошибка сохранения', 'Произошла ошибка во время сохранения. Повторите попытку.');
            }
        });
    };
    this.writeData = function () {
        var txtName = this.getTrackerNameTxt(),
            txtDescription = this.getTrackerDescriptionTxt(),
            fldConsumption = this.getTrackerConsumptionField(),
            chkHideInEvos = this.getTrackerEvosChk(),
            fldColor = this.getTrackerColorField();

        this.record.name = txtName.getValue();
        this.record.description = txtDescription.getValue();
        this.record.consumption = fldConsumption.getValue();
        this.record.hevos = chkHideInEvos.getValue();
        this.record.color = fldColor.getValue();
        this.record.odometer = this.getOdometerValue();
        this.writeRelays();
    };
    this.writeRelays = function () {
        this.writeRelay('');
        this.writeRelay(1);
        this.writeRelay(2);
        this.writeSensor(1);
        this.writeSensor(2);
    };
    this.writeRelay = function (id) {
        var txt = App['txtRelay' + id];
        this.record['r' + id].n = txt.getValue();
    };
    this.writeSensor = function (id) {
        var txt = App['txtSensor' + id];
        this.record['s' + id].n = txt.getValue();
    };
    this.getOdometerValue = function () {
        return this.getTrackerOdometer().getValue();
    };
    this.setOdometerValue = function (value) {
        this.getTrackerOdometer().setValue(value);
        this.getTrackerOdometer().odometer = value;
        if (value == undefined) {
            this.getTrackerOdometerLbl().setText('Загрузка данных...');
            return;
        }
        var km = value / 1000;
        this.getTrackerOdometerLbl().setText(km.toFixed(1) + ' км');
    };
    this.showExecuteMask = function () {
        var wnd = this.getWindow();
        wnd.body.mask('Отправка данных...');
    };
    this.hideExecuteMask = function () {
        var wnd = this.getWindow();
        wnd.body.unmask();
    };
    this.maskSaving = function () {
        var wnd = this.getWindow();
        wnd.body.mask('Сохранение данных...');
    };
    this.unmaskSaving = function () {
        this.hideExecuteMask();
    };
    this.startEdit = function (item, boundEl, value, fn) {
        if (value == 'Загрузка данных...') {
            return false;
        }
        setTimeout(function () {
            carSettings.setOdometerValue(carSettings.getTrackerOdometer().odometer);
        }, 50);
    };
    this.cancelEdit = function (item, value) {
        this.setOdometerValue(value || 0);
    };
};

function editVirtualTracker() {
    var record = carSettings.record;
    var wnd = getWndVirtualTracker();
    wnd.record = record;
    var isNew = record.v_trackerid == 0;
    gettxtVirtualName().setValue(record.v_name);
    getlblVirtualId().setText('Идентификатор трекера: ' + record.v_trackerid);
    if (isNew) {
        getbtnDeleteVirtual().hide();
    } else {
        getbtnDeleteVirtual().show();
    }
    wnd.show();
}

function saveVirtual() {
    var wnd = getWndVirtualTracker();
    if (wnd.record.v_trackerid != 0) {
        wnd.record.v_name = gettxtVirtualName().getValue();
        wnd.hide();
        return;
    }
    wnd.body.mask('Применение...');
    DR.GetNextTrackerId({
        success: function (r) {
            wnd.record.v_name = gettxtVirtualName().getValue();
            wnd.record.v_trackerid = r;
            wnd.body.unmask();
            wnd.hide();
        }
    });
}

function clearVirtual() {
    var wnd = getWndVirtualTracker();
    wnd.record.v_name = '';
    wnd.record.v_trackerid = 0;
    wnd.hide();
}

function getWndVirtualTracker() {
    return App.windowVirtualTracker;
}
function getbtnDeleteVirtual() {
    return App.btnDeleteVirtual;
}
function getbtnSaveVirtual() {
    return App.btnSaveVirtual;
}
function gettxtVirtualName() {
    return App.txtVirtualName;
}
function getlblVirtualId() {
    return App.lblVirtualId;
}
carSettings = new carSettings();