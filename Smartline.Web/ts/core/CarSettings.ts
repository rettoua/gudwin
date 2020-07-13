module Core {
    declare var $App: Application;
    declare var App;//extjs
    declare var DR;//direct methods namespace
    declare var Ext;

    export class CarSettings {
        private _odometer: Odometer;
        private _repository: Repository;
        private _virtualTracker: VirtualTracker;
        private _record: any;
        private _gpsReceivedEventHandler: Core.EventHandler<Controllers.GpsReceivedEventArgs>;
        private _maskSensorAlarmDiv: HTMLDivElement;

        public get Record(): any {
            return this._record;
        }

        public get Odometer(): Odometer {
            return this._odometer;

        }

        constructor() {
            this._odometer = new Odometer();
            this._virtualTracker = new VirtualTracker();
            this._gpsReceivedEventHandler = new Core.EventHandler<Controllers.GpsReceivedEventArgs>((sender, e) => this.onGpsReceived(sender, e));
        }

        public show(record: any): void {
            this.showWindow();
            this.mask();
            this.load(record.data.TrackerId);
        }
        public hide(): void {
            this.unsubscribeFromChanges();
        }
        public relayClick(btn, id): void {
            var data = $App.RepositoryManager.Repositories[this._record.id].Record.data;
            var obj = data['Relay' + id], state = data.s['Relay' + id];
            this.showExecuteMask();
            try {
                if ($App.ServerCommunication.State == Core.ConnectionState.connected) {
                    var func = state === true ? 'turnOffRelay' : 'turnOnRelay';
                    var trackerId = this._record.id,
                        hub = $App.ServerCommunication.Hub;
                    hub.server[func](trackerId, obj.id).done(() => {
                        Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                    }).fail(error => {
                            Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
                        });
                } else {
                    var func = state === true ? 'TurnOffRelay' : 'TurnOnRelay';
                    DR[func]($App.SettingsManager.Settings.UserId, this._record.id, obj.id);
                    Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                }
            } catch (e) {
                Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
            }
            finally {
                this.hideExecuteMask();
            }
        }
        public sensorAlarmClick(): void {
            this.showExecuteMask();
            try {
                if ($App.ServerCommunication.State == Core.ConnectionState.connected) {
                    var trackerId = this._record.id,
                        hub = $App.ServerCommunication.Hub;
                    hub.server.turnOffAlarming(trackerId).done(() => {
                        Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                    }).fail(error => {
                            Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
                        });
                } else {
                    DR.TurnOffAlarmingSensor($App.SettingsManager.Settings.UserId, this._record.id);
                    Ext.Msg.alert('Отправка данных', 'Данные успешно отправлены на трекер!');
                }
            } catch (e) {
                Ext.Msg.alert('Ошибка выполения', 'Ошибка выполнения. Повторите попытку.');
            }
            finally {
                this.hideExecuteMask();
            }
        }
        public startEdit(item, boundEl, value, fn): boolean {
            if (value == 'Загрузка данных...') {
                return false;
            }
            setTimeout(() => {
                this._odometer.setOdometerValue(this._odometer.getTrackerOdometer().odometer);
            }, 50);
            return true;
        }
        public editVirtualTracker(): void {
            this._virtualTracker.editVirtualTracker(this._record);
        }
        public saveVirtual(): void {
            this._virtualTracker.saveVirtual();
        }
        public clearVirtual() {
            this._virtualTracker.clearVirtual();
        }
        private cancelEdit = (item, value, startValue) => {
            this.Odometer.setOdometerValue(value || 0);
        }
        private load(trackerId: number): void {
            this._odometer.setOdometerValue(null);
            DR.GetTracker(trackerId, $App.SettingsManager.Settings.UserId, {
                success: tracker => {
                    this._record = tracker;
                    this.display(tracker);
                    this._odometer.loadOdometer(tracker);
                    this.subscribeForChanges();
                    this.unmask();
                },
                failure: e => {
                    this.unmask();
                }
            });
        }
        private save(): void {
            this.maskSaving();
            CarSettingsStore.writeData(this);
            DR.UpdateTracker(this._record, this._odometer.getOdometerValue(), $App.SettingsManager.Settings.UserId, {
                success: rec => {
                    this.unmaskSaving();
                    this.display(rec);
                    Ext.Msg.notify('Сохранение данных', 'Данные успешно сохранены!');
                },
                failure: e => {
                    this.unmaskSaving();
                    Ext.Msg.alert('Ошибка сохранения', 'Произошла ошибка во время сохранения. Повторите попытку.');
                }
            });
        }
        private maskSaving = function () {
            var wnd = this.getWindow();
            wnd.body.mask('Сохранение данных...');
        }
        private unmaskSaving(): void {
            this.hideExecuteMask();
        }
        private showExecuteMask = function () {
            var wnd = this.getWindow();
            wnd.body.mask('Отправка данных...');
        }
        private hideExecuteMask(): void {
            var wnd = this.getWindow();
            wnd.body.unmask();
        }
        private display(tracker): void {
            var lblId = this.getTrackerIdLabel(),
                txtName = this.getTrackerNameTxt(),
                txtDescription = this.getTrackerDescriptionTxt(),
                fldConsumption = this.getTrackerConsumptionField(),
                chkHideInEvos = this.getTrackerEvosChk(),
                fldColor = this.getTrackerColorField();

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
        }
        private displaySensorsData(tracker: any): void {
            this.displayRelayData(tracker.r, '');
            this.displayRelayData(tracker.r1, 1);
            this.displayRelayData(tracker.r2, 2);
            this.displaySensorData(tracker.s1, 1);
            this.displaySensorData(tracker.s2, 2);
        }
        private displaySensors(tracker): void {
            var repo = $App.RepositoryManager.Repositories[tracker.id];
            repo.updateSensors(tracker);
            this.updateRelays(repo);
        }
        private updateRelays(repo): void {
            if (this.getWindow().hidden === true) { return; }
            if (!this._record || repo.Record.data.Id != this._record.id) { return; }
            this.displayRelayState(repo, '');
            this.displayRelayState(repo, 1);
            this.displayRelayState(repo, 2);
            this.displaySensorState(repo, 1);
            this.displaySensorState(repo, 2);
        }
        private displayRelayData(relay, id): void {
            var txt = App['txtRelay' + id];
            txt.setValue(relay.n);
        }
        private displayRelayState(repo, id): void {
            var btn = App['btnRelay' + id], c;
            if (repo.IsConnected === false || repo.Record.data.s == undefined) {
                c = 'grey';
            } else if (repo.Record.data.s['Relay' + id] === false || repo.Record.data.s['r' + id] === false) {
                c = 'red';
                btn.setText('ВКЛ');
            } else {
                c = 'greenyellow';
                btn.setText('ВЫКЛ');
            }
            btn.el.dom.style.backgroundColor = c;
            btn.setDisabled(c == 'grey');
        }

        private displaySensorData(sensor, id): void {
            var txt = App['txtSensor' + id];
            txt.setValue(sensor.n);
            if (id == 2) {
                App.chkSensor2.setValue(sensor.sos);
            }
        }
        public displaySensorState(repo, id): void {
            var btn = App['btnSensor' + id], c,
                checkAlarming = (repo.Record.data['Sensor' + id] || repo.Record.data['s' + id]).sos === true;
            //App.chkSensor2.setValue(checkAlarming);
            if (checkAlarming === true) {
                this.displayAlarmingSensorState(repo);
                return;
            }
            if (repo.IsConnected === false || repo.Record.data.s == undefined) {
                c = 'grey';
            } else if (repo.Record.data.s['Sensor' + id] === false || repo.Record.data.s['s' + id] === false) {
                c = 'red';
            } else {
                c = 'greenyellow';
            }
            btn.el.dom.style.backgroundColor = c;
            btn.setDisabled(c == 'grey');
            this.offAlarmButtonState();
        }

        public displayAlarmingSensorState(repo): void {
            var btn = App['btnSensor2'], c;
            if (repo.isSosAlarming) {
                c = repo.alarmingColor;
            } else {
                c = 'orange';
            }

            btn.el.dom.style.backgroundColor = c;
            if (repo.isSosAlarming === true) {
                this.onAlarmButtonState();
            } else {
                this.offAlarmButtonState();
            }
        }

        public loadCarImages(): void {
            DR.LoadCarIcons({
                success: (r) => {
                    this.showCarImages(r);
                },
                failure: () => { }
            });
        }

        private showCarImages(images: any): void {
            var store = App.CarImagesStore,
                combo = App.CarImagesCombobox;
            store.removeAll();
            store.loadData(images);
            combo.setValue($App.RepositoryManager.Repositories[this.Record.id].Image.name);
        }

        public changeSensorAlarmCheckbox(): void {
            App.btnSensor2.setText(App.chkSensor2.getValue() ? 'SOS' : '');
        }

        private onAlarmButtonState(): void {
            App.PanelSensor2.setWidth(80);
            App.lblSensor2.hide();
            App.btnSensor2Off.show();
        }

        private offAlarmButtonState(): void {
            App.PanelSensor2.setWidth(70);
            App.lblSensor2.show();
            App.btnSensor2Off.hide();
        }

        private applyVirtualData(): void {
            var btnVirtual = this.getVirtualTrackerButton();
            btnVirtual.setText('<b>Виртуальный трекер:</b> ' + (this._record.v_trackerid && this._record.v_name ? '[' + this._record.v_trackerid + ']' + this._record.v_name : 'нет'));
        }

        public mouseEnterSensor2(): void {
            var bounds = App.PanelSensor2.body.dom.getBoundingClientRect();
            this._maskSensorAlarmDiv = document.createElement('div');
            $(this._maskSensorAlarmDiv).css({
                position: "absolute",
                marginLeft: -3, marginTop: -3,
                top: bounds.top, left: bounds.left,
                width: bounds.width, height: bounds.height,
                zIndex: 100000
            }).css(
                {
                    borderStyle: 'double',
                    borderColor: '#00b2ff'
                });
            document.body.appendChild(this._maskSensorAlarmDiv);
        }

        public mouseLeaveSensor2(): void {
            if (this._maskSensorAlarmDiv) {
                document.body.removeChild(this._maskSensorAlarmDiv);
            }
        }

        private subscribeForChanges(): void {
            $App.RepositoryManager.GpsProvider.GpsReceived.subscribe(this._gpsReceivedEventHandler);
        }

        private unsubscribeFromChanges(): void {
            $App.RepositoryManager.GpsProvider.GpsReceived.unsubscribe(this._gpsReceivedEventHandler);
        }
        private onGpsReceived(sender: any, e: Controllers.GpsReceivedEventArgs): void {
            e.Items.forEach(item=> {
                if (item.TrackerId == this._record.id) {
                    this.displaySensors(this._record);
                }
            });
        }
        private getWindow(): any {
            return App.wndCarSettings;
        }
        private showWindow(): void {
            var wnd = this.getWindow();
            wnd.show();
        }
        private mask(): void {
            var wnd = this.getWindow();
            wnd.body.mask('Загрузка данных...');
        }
        private unmask(): void {
            var wnd = this.getWindow();
            wnd.body.unmask();
        }
        private getTrackerIdLabel(): any {
            return App.lblTrackerId;
        }
        public getTrackerNameTxt(): any {
            return App.txtName;
        }
        public getTrackerDescriptionTxt(): any {
            return App.txtDescription;
        }
        public getTrackerConsumptionField(): any {
            return App.fldConsumption;
        }
        public getTrackerEvosChk(): any {
            return App.chkHideInEvos;
        }
        public getTrackerColorField(): any {
            return App.fldColor;
        }
        private getVirtualTrackerButton(): any {
            return App.btnVirtualTracker;
        }
    }

    export class Odometer {
        private _record: any;

        public loadOdometer(record: any): void {
            this._record = record;
            var fldOdometer = this.getTrackerOdometer();
            fldOdometer.setValue(null);
            DR.LoadOdometer(this._record.id, this._record.trackerid, {
                success: obj => {
                    this.setOdometerValue((obj && obj.o) || 0);
                },
                failure: e => { }
            });
        }
        public setOdometerValue(value): void {
            this.getTrackerOdometer().setValue(value);
            this.getTrackerOdometer().odometer = value;
            if (value == undefined) {
                this.getTrackerOdometerLbl().setText('Загрузка данных...');
                return;
            }
            var km = value / 1000;
            this.getTrackerOdometerLbl().setText(km.toFixed(1) + ' км');
        }
        public getOdometerValue(): any {
            return this.getTrackerOdometer().getValue();
        }
        public getTrackerOdometer(): any {
            return App.fldOdometer;
        }
        private getTrackerOdometerLbl(): any {
            return App.lblOdometer;
        }
    }

    class CarSettingsStore {
        public static writeData(carSettings: CarSettings): void {
            var txtName = carSettings.getTrackerNameTxt(),
                txtDescription = carSettings.getTrackerDescriptionTxt(),
                fldConsumption = carSettings.getTrackerConsumptionField(),
                chkHideInEvos = carSettings.getTrackerEvosChk(),
                fldColor = carSettings.getTrackerColorField();

            carSettings.Record.name = txtName.getValue();
            carSettings.Record.description = txtDescription.getValue();
            carSettings.Record.consumption = fldConsumption.getValue();
            carSettings.Record.hevos = chkHideInEvos.getValue();
            carSettings.Record.color = fldColor.getValue();
            carSettings.Record.odometer = carSettings.Odometer.getOdometerValue();
            CarSettingsStore.writeImage(carSettings);
            CarSettingsStore.writeRelays(carSettings);
        }

        private static writeImage(carSettings: CarSettings): void {
            var store = App.CarImagesStore, combo = App.CarImagesCombobox,
                repo = $App.RepositoryManager.Repositories[carSettings.Record.id];
            carSettings.Record.image = store.getById(combo.getValue()).data;
            repo.updateImage(carSettings.Record.image);
        }

        private static writeRelays(carSettings: CarSettings): void {
            CarSettingsStore.writeRelay(carSettings, '');
            CarSettingsStore.writeRelay(carSettings, 1);
            CarSettingsStore.writeRelay(carSettings, 2);
            CarSettingsStore.writeSensor(carSettings, 1);
            CarSettingsStore.writeSensor(carSettings, 2);
        }
        private static writeRelay(carSettings: CarSettings, id): void {
            var txt = App['txtRelay' + id];
            carSettings.Record['r' + id].n = txt.getValue();
        }
        private static writeSensor(carSettings: CarSettings, id) {
            var txt = App['txtSensor' + id];
            carSettings.Record['s' + id].n = txt.getValue();
            if (id == 2) {
                carSettings.Record['s' + id].sos = App.chkSensor2.getValue();
            }
        }
    }

    class VirtualTracker {

        public editVirtualTracker(record: any): void {
            var wnd = this.getWndVirtualTracker();
            wnd.record = record;
            var isNew = record.v_trackerid == 0;
            this.gettxtVirtualName().setValue(record.v_name);
            this.getlblVirtualId().setText('ID трекера: ' + record.v_trackerid);
            if (isNew) {
                this.getbtnDeleteVirtual().hide();
            } else {
                this.getbtnDeleteVirtual().show();
            }
            wnd.show();
        }
        public saveVirtual(): void {
            var wnd = this.getWndVirtualTracker();
            if (wnd.record.v_trackerid != 0) {
                wnd.record.v_name = this.gettxtVirtualName().getValue();
                wnd.hide();
                return;
            }
            wnd.body.mask('Применение...');
            var me = this;
            DR.GetNextTrackerId({
                success: r=> {
                    wnd.record.v_name = me.gettxtVirtualName().getValue();
                    wnd.record.v_trackerid = r;
                    wnd.body.unmask();
                    wnd.hide();
                }
            });
        }
        public clearVirtual(): void {
            var wnd = this.getWndVirtualTracker();
            wnd.record.v_name = '';
            wnd.record.v_trackerid = 0;
            wnd.hide();
        }
        private getWndVirtualTracker(): any {
            return App.windowVirtualTracker;
        }
        private getbtnDeleteVirtual(): any {
            return App.btnDeleteVirtual;
        }
        private getbtnSaveVirtual(): any {
            return App.btnSaveVirtual;
        }
        private gettxtVirtualName(): any {
            return App.txtVirtualName;
        }
        private getlblVirtualId(): any {
            return App.lblVirtualId;
        }
    }
}