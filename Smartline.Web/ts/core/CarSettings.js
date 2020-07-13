var Core;
(function (Core) {
    var CarSettings = /** @class */ (function () {
        function CarSettings() {
            var _this = this;
            this.cancelEdit = function (item, value, startValue) {
                _this.Odometer.setOdometerValue(value || 0);
            };
            this.maskSaving = function () {
                var wnd = this.getWindow();
                wnd.body.mask('Сохранение данных...');
            };
            this.showExecuteMask = function () {
                var wnd = this.getWindow();
                wnd.body.mask('Отправка данных...');
            };
            this._odometer = new Odometer();
            this._virtualTracker = new VirtualTracker();
            this._gpsReceivedEventHandler = new Core.EventHandler(function (sender, e) { return _this.onGpsReceived(sender, e); });
        }
        Object.defineProperty(CarSettings.prototype, "Record", {
            get: function () {
                return this._record;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(CarSettings.prototype, "Odometer", {
            get: function () {
                return this._odometer;
            },
            enumerable: true,
            configurable: true
        });
        CarSettings.prototype.show = function (record) {
            this.showWindow();
            this.mask();
            this.load(record.data.TrackerId);
        };
        CarSettings.prototype.hide = function () {
            this.unsubscribeFromChanges();
        };
        CarSettings.prototype.relayClick = function (btn, id) {
            var data = $App.RepositoryManager.Repositories[this._record.id].Record.data;
            var obj = data['Relay' + id], state = data.s['Relay' + id];
            this.showExecuteMask();
            try {
                if ($App.ServerCommunication.State == Core.ConnectionState.connected) {
                    var func = state === true ? 'turnOffRelay' : 'turnOnRelay';
                    var trackerId = this._record.id, hub = $App.ServerCommunication.Hub;
                    hub.server[func](trackerId, obj.id).done(function () {
                        Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                    }).fail(function (error) {
                        Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
                    });
                }
                else {
                    var func = state === true ? 'TurnOffRelay' : 'TurnOnRelay';
                    DR[func]($App.SettingsManager.Settings.UserId, this._record.id, obj.id);
                    Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                }
            }
            catch (e) {
                Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
            }
            finally {
                this.hideExecuteMask();
            }
        };
        CarSettings.prototype.sensorAlarmClick = function () {
            this.showExecuteMask();
            try {
                if ($App.ServerCommunication.State == Core.ConnectionState.connected) {
                    var trackerId = this._record.id, hub = $App.ServerCommunication.Hub;
                    hub.server.turnOffAlarming(trackerId).done(function () {
                        Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                    }).fail(function (error) {
                        Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
                    });
                }
                else {
                    DR.TurnOffAlarmingSensor($App.SettingsManager.Settings.UserId, this._record.id);
                    Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                }
            }
            catch (e) {
                Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
            }
            finally {
                this.hideExecuteMask();
            }
        };
        CarSettings.prototype.startEdit = function (item, boundEl, value, fn) {
            var _this = this;
            if (value == 'Загрузка данных...') {
                return false;
            }
            setTimeout(function () {
                _this._odometer.setOdometerValue(_this._odometer.getTrackerOdometer().odometer);
            }, 50);
            return true;
        };
        CarSettings.prototype.editVirtualTracker = function () {
            this._virtualTracker.editVirtualTracker(this._record);
        };
        CarSettings.prototype.saveVirtual = function () {
            this._virtualTracker.saveVirtual();
        };
        CarSettings.prototype.clearVirtual = function () {
            this._virtualTracker.clearVirtual();
        };
        CarSettings.prototype.load = function (trackerId) {
            var _this = this;
            this._odometer.setOdometerValue(null);
            DR.GetTracker(trackerId, $App.SettingsManager.Settings.UserId, {
                success: function (tracker) {
                    _this._record = tracker;
                    _this.display(tracker);
                    _this._odometer.loadOdometer(tracker);
                    _this.subscribeForChanges();
                    _this.unmask();
                },
                failure: function (e) {
                    _this.unmask();
                }
            });
        };
        CarSettings.prototype.save = function () {
            var _this = this;
            this.maskSaving();
            CarSettingsStore.writeData(this);
            DR.UpdateTracker(this._record, this._odometer.getOdometerValue(), $App.SettingsManager.Settings.UserId, {
                success: function (rec) {
                    _this.unmaskSaving();
                    _this.display(rec);
                    Ext.Msg.notify('Сохранение данных', 'Данные успешно сохранены!');
                },
                failure: function (e) {
                    _this.unmaskSaving();
                    Ext.Msg.alert('Ошибка сохранения', 'Произошла ошибка во время сохранения. Повторите попытку.');
                }
            });
        };
        CarSettings.prototype.unmaskSaving = function () {
            this.hideExecuteMask();
        };
        CarSettings.prototype.hideExecuteMask = function () {
            var wnd = this.getWindow();
            wnd.body.unmask();
        };
        CarSettings.prototype.display = function (tracker) {
            var lblId = this.getTrackerIdLabel(), txtName = this.getTrackerNameTxt(), txtDescription = this.getTrackerDescriptionTxt(), fldConsumption = this.getTrackerConsumptionField(), chkHideInEvos = this.getTrackerEvosChk(), fldColor = this.getTrackerColorField();
            lblId.el.setHTML('<b>Идентификатор трекера:</b> ' + tracker.trackerid);
            txtName.setValue(tracker.name);
            txtDescription.setValue(tracker.description);
            fldConsumption.setValue(tracker.consumption || 0);
            chkHideInEvos.setValue(tracker.hevos);
            fldColor.setValue('#' + tracker.color);
            this.applyVirtualData();
            this.displaySensorsData(tracker);
            this.displaySensors(tracker);
            this.loadCarImages();
        };
        CarSettings.prototype.displaySensorsData = function (tracker) {
            this.displayRelayData(tracker.r, '');
            this.displayRelayData(tracker.r1, 1);
            this.displayRelayData(tracker.r2, 2);
            this.displaySensorData(tracker.s1, 1);
            this.displaySensorData(tracker.s2, 2);
        };
        CarSettings.prototype.displaySensors = function (tracker) {
            var repo = $App.RepositoryManager.Repositories[tracker.id];
            repo.updateSensors(tracker);
            this.updateRelays(repo);
        };
        CarSettings.prototype.updateRelays = function (repo) {
            if (this.getWindow().hidden === true) {
                return;
            }
            if (!this._record || repo.Record.data.Id != this._record.id) {
                return;
            }
            this.displayRelayState(repo, '');
            this.displayRelayState(repo, 1);
            this.displayRelayState(repo, 2);
            this.displaySensorState(repo, 1);
            this.displaySensorState(repo, 2);
        };
        CarSettings.prototype.displayRelayData = function (relay, id) {
            var txt = App['txtRelay' + id];
            txt.setValue(relay.n);
        };
        CarSettings.prototype.displayRelayState = function (repo, id) {
            var btn = App['btnRelay' + id], c;
            if (repo.IsConnected === false || repo.Record.data.s == undefined) {
                c = 'grey';
            }
            else if (repo.Record.data.s['Relay' + id] === false || repo.Record.data.s['r' + id] === false) {
                c = 'red';
                btn.setText('ВКЛ');
            }
            else {
                c = 'greenyellow';
                btn.setText('ВЫКЛ');
            }
            btn.el.dom.style.backgroundColor = c;
            btn.setDisabled(c == 'grey');
        };
        CarSettings.prototype.displaySensorData = function (sensor, id) {
            var txt = App['txtSensor' + id];
            txt.setValue(sensor.n);
            if (id == 2) {
                App.chkSensor2.setValue(sensor.sos);
            }
        };
        CarSettings.prototype.displaySensorState = function (repo, id) {
            var btn = App['btnSensor' + id], c, checkAlarming = (repo.Record.data['Sensor' + id] || repo.Record.data['s' + id]).sos === true;
            //App.chkSensor2.setValue(checkAlarming);
            if (checkAlarming === true) {
                this.displayAlarmingSensorState(repo);
                return;
            }
            if (repo.IsConnected === false || repo.Record.data.s == undefined) {
                c = 'grey';
            }
            else if (repo.Record.data.s['Sensor' + id] === false || repo.Record.data.s['s' + id] === false) {
                c = 'red';
            }
            else {
                c = 'greenyellow';
            }
            btn.el.dom.style.backgroundColor = c;
            btn.setDisabled(c == 'grey');
            this.offAlarmButtonState();
        };
        CarSettings.prototype.displayAlarmingSensorState = function (repo) {
            var btn = App['btnSensor2'], c;
            if (repo.isSosAlarming) {
                c = repo.alarmingColor;
            }
            else {
                c = 'orange';
            }
            btn.el.dom.style.backgroundColor = c;
            if (repo.isSosAlarming === true) {
                this.onAlarmButtonState();
            }
            else {
                this.offAlarmButtonState();
            }
        };
        CarSettings.prototype.loadCarImages = function () {
            var _this = this;
            DR.LoadCarIcons({
                success: function (r) {
                    _this.showCarImages(r);
                },
                failure: function () { }
            });
        };
        CarSettings.prototype.showCarImages = function (images) {
            var store = App.CarImagesStore, combo = App.CarImagesCombobox;
            store.removeAll();
            store.loadData(images);
            combo.setValue($App.RepositoryManager.Repositories[this.Record.id].Image.name);
        };
        CarSettings.prototype.changeSensorAlarmCheckbox = function () {
            App.btnSensor2.setText(App.chkSensor2.getValue() ? 'SOS' : '');
        };
        CarSettings.prototype.onAlarmButtonState = function () {
            App.PanelSensor2.setWidth(80);
            App.lblSensor2.hide();
            App.btnSensor2Off.show();
        };
        CarSettings.prototype.offAlarmButtonState = function () {
            App.PanelSensor2.setWidth(70);
            App.lblSensor2.show();
            App.btnSensor2Off.hide();
        };
        CarSettings.prototype.applyVirtualData = function () {
            var btnVirtual = this.getVirtualTrackerButton();
            btnVirtual.setText('<b>Виртуальный трекер:</b> ' + (this._record.v_trackerid && this._record.v_name ? '[' + this._record.v_trackerid + ']' + this._record.v_name : 'нет'));
        };
        CarSettings.prototype.mouseEnterSensor2 = function () {
            var bounds = App.PanelSensor2.body.dom.getBoundingClientRect();
            this._maskSensorAlarmDiv = document.createElement('div');
            $(this._maskSensorAlarmDiv).css({
                position: "absolute",
                marginLeft: -3, marginTop: -3,
                top: bounds.top, left: bounds.left,
                width: bounds.width, height: bounds.height,
                zIndex: 100000
            }).css({
                borderStyle: 'double',
                borderColor: '#00b2ff'
            });
            document.body.appendChild(this._maskSensorAlarmDiv);
        };
        CarSettings.prototype.mouseLeaveSensor2 = function () {
            if (this._maskSensorAlarmDiv) {
                document.body.removeChild(this._maskSensorAlarmDiv);
            }
        };
        CarSettings.prototype.subscribeForChanges = function () {
            $App.RepositoryManager.GpsProvider.GpsReceived.subscribe(this._gpsReceivedEventHandler);
        };
        CarSettings.prototype.unsubscribeFromChanges = function () {
            $App.RepositoryManager.GpsProvider.GpsReceived.unsubscribe(this._gpsReceivedEventHandler);
        };
        CarSettings.prototype.onGpsReceived = function (sender, e) {
            var _this = this;
            e.Items.forEach(function (item) {
                if (item.TrackerId == _this._record.id) {
                    _this.displaySensors(_this._record);
                }
            });
        };
        CarSettings.prototype.getWindow = function () {
            return App.wndCarSettings;
        };
        CarSettings.prototype.showWindow = function () {
            var wnd = this.getWindow();
            wnd.show();
        };
        CarSettings.prototype.mask = function () {
            var wnd = this.getWindow();
            wnd.body.mask('Загрузка данных...');
        };
        CarSettings.prototype.unmask = function () {
            var wnd = this.getWindow();
            wnd.body.unmask();
        };
        CarSettings.prototype.getTrackerIdLabel = function () {
            return App.lblTrackerId;
        };
        CarSettings.prototype.getTrackerNameTxt = function () {
            return App.txtName;
        };
        CarSettings.prototype.getTrackerDescriptionTxt = function () {
            return App.txtDescription;
        };
        CarSettings.prototype.getTrackerConsumptionField = function () {
            return App.fldConsumption;
        };
        CarSettings.prototype.getTrackerEvosChk = function () {
            return App.chkHideInEvos;
        };
        CarSettings.prototype.getTrackerColorField = function () {
            return App.fldColor;
        };
        CarSettings.prototype.getVirtualTrackerButton = function () {
            return App.btnVirtualTracker;
        };
        return CarSettings;
    }());
    Core.CarSettings = CarSettings;
    var Odometer = /** @class */ (function () {
        function Odometer() {
        }
        Odometer.prototype.loadOdometer = function (record) {
            var _this = this;
            this._record = record;
            var fldOdometer = this.getTrackerOdometer();
            fldOdometer.setValue(null);
            DR.LoadOdometer(this._record.id, this._record.trackerid, {
                success: function (obj) {
                    _this.setOdometerValue((obj && obj.o) || 0);
                },
                failure: function (e) { }
            });
        };
        Odometer.prototype.setOdometerValue = function (value) {
            this.getTrackerOdometer().setValue(value);
            this.getTrackerOdometer().odometer = value;
            if (value == undefined) {
                this.getTrackerOdometerLbl().setText('Загрузка данных...');
                return;
            }
            var km = value / 1000;
            this.getTrackerOdometerLbl().setText(km.toFixed(1) + ' км');
        };
        Odometer.prototype.getOdometerValue = function () {
            return this.getTrackerOdometer().getValue();
        };
        Odometer.prototype.getTrackerOdometer = function () {
            return App.fldOdometer;
        };
        Odometer.prototype.getTrackerOdometerLbl = function () {
            return App.lblOdometer;
        };
        return Odometer;
    }());
    Core.Odometer = Odometer;
    var CarSettingsStore = /** @class */ (function () {
        function CarSettingsStore() {
        }
        CarSettingsStore.writeData = function (carSettings) {
            var txtName = carSettings.getTrackerNameTxt(), txtDescription = carSettings.getTrackerDescriptionTxt(), fldConsumption = carSettings.getTrackerConsumptionField(), chkHideInEvos = carSettings.getTrackerEvosChk(), fldColor = carSettings.getTrackerColorField();
            carSettings.Record.name = txtName.getValue();
            carSettings.Record.description = txtDescription.getValue();
            carSettings.Record.consumption = fldConsumption.getValue();
            carSettings.Record.hevos = chkHideInEvos.getValue();
            carSettings.Record.color = fldColor.getValue();
            carSettings.Record.odometer = carSettings.Odometer.getOdometerValue();
            CarSettingsStore.writeImage(carSettings);
            CarSettingsStore.writeRelays(carSettings);
        };
        CarSettingsStore.writeImage = function (carSettings) {
            var store = App.CarImagesStore, combo = App.CarImagesCombobox, repo = $App.RepositoryManager.Repositories[carSettings.Record.id];
            carSettings.Record.image = store.getById(combo.getValue()).data;
            repo.updateImage(carSettings.Record.image);
        };
        CarSettingsStore.writeRelays = function (carSettings) {
            CarSettingsStore.writeRelay(carSettings, '');
            CarSettingsStore.writeRelay(carSettings, 1);
            CarSettingsStore.writeRelay(carSettings, 2);
            CarSettingsStore.writeSensor(carSettings, 1);
            CarSettingsStore.writeSensor(carSettings, 2);
        };
        CarSettingsStore.writeRelay = function (carSettings, id) {
            var txt = App['txtRelay' + id];
            carSettings.Record['r' + id].n = txt.getValue();
        };
        CarSettingsStore.writeSensor = function (carSettings, id) {
            var txt = App['txtSensor' + id];
            carSettings.Record['s' + id].n = txt.getValue();
            if (id == 2) {
                carSettings.Record['s' + id].sos = App.chkSensor2.getValue();
            }
        };
        return CarSettingsStore;
    }());
    var VirtualTracker = /** @class */ (function () {
        function VirtualTracker() {
        }
        VirtualTracker.prototype.editVirtualTracker = function (record) {
            var wnd = this.getWndVirtualTracker();
            wnd.record = record;
            var isNew = record.v_trackerid == 0;
            this.gettxtVirtualName().setValue(record.v_name);
            this.getlblVirtualId().setText('ID трекера: ' + record.v_trackerid);
            if (isNew) {
                this.getbtnDeleteVirtual().hide();
            }
            else {
                this.getbtnDeleteVirtual().show();
            }
            wnd.show();
        };
        VirtualTracker.prototype.saveVirtual = function () {
            var wnd = this.getWndVirtualTracker();
            if (wnd.record.v_trackerid != 0) {
                wnd.record.v_name = this.gettxtVirtualName().getValue();
                wnd.hide();
                return;
            }
            wnd.body.mask('Применение...');
            var me = this;
            DR.GetNextTrackerId({
                success: function (r) {
                    wnd.record.v_name = me.gettxtVirtualName().getValue();
                    wnd.record.v_trackerid = r;
                    wnd.body.unmask();
                    wnd.hide();
                }
            });
        };
        VirtualTracker.prototype.clearVirtual = function () {
            var wnd = this.getWndVirtualTracker();
            wnd.record.v_name = '';
            wnd.record.v_trackerid = 0;
            wnd.hide();
        };
        VirtualTracker.prototype.getWndVirtualTracker = function () {
            return App.windowVirtualTracker;
        };
        VirtualTracker.prototype.getbtnDeleteVirtual = function () {
            return App.btnDeleteVirtual;
        };
        VirtualTracker.prototype.getbtnSaveVirtual = function () {
            return App.btnSaveVirtual;
        };
        VirtualTracker.prototype.gettxtVirtualName = function () {
            return App.txtVirtualName;
        };
        VirtualTracker.prototype.getlblVirtualId = function () {
            return App.lblVirtualId;
        };
        return VirtualTracker;
    }());
})(Core || (Core = {}));
