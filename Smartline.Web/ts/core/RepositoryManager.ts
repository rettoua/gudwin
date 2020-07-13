module Core {
    declare var $App: Application;
    declare var App;
    declare var Ext;
    declare var L;

    export interface IRecord {
        internalId: number;
        data: any;
        get(name: string): any;
        set(name: string, value: any): void;
    }

    export interface RepositoryArray extends Array<Repository> {
        [index: number]: Repository
    }

    export class RepositoryManager {
        private _gpsProvider: Controllers.GpsProvider;
        private _gpsReceivedEventHandler: Core.EventHandler<Controllers.GpsReceivedEventArgs>;
        private _repositories: RepositoryArray = [];
        private _view: Views.ViewRepositoryManager;
        private _grid: any;
        private _sosHelper: SosHelper;

        public get Repositories(): RepositoryArray {
            return this._repositories;
        }

        public get IsShowHotTracking(): boolean {
            return App.chkShowTrack.pressed === true;
        }

        public get View(): Views.ViewRepositoryManager {
            return this._view;
        }

        public get GpsProvider(): Controllers.GpsProvider {
            return this._gpsProvider;
        }

        constructor() {
            this._gpsProvider = new Controllers.GpsProvider();
            this._view = new Views.ViewRepositoryManager();
            this._sosHelper = new SosHelper(this);
            this._gpsReceivedEventHandler = new Core.EventHandler<Controllers.GpsReceivedEventArgs>((sender, e) => this.OnGpsReceived(sender, e));
            this._gpsProvider.GpsReceived.subscribe(this._gpsReceivedEventHandler);
        }

        public LoadRecords(grid: any): void {
            this._grid = grid;
            this.InitializeRepositories(this._grid.store.getAllRange());
            this._gpsProvider.Start();
            this._gpsProvider.LoadUnreachableData();
            this.UpdatePrependText();
        }

        public GetRepositoriesForRefresh(): Array<any> {
            var values = [];
            for (var k in this._repositories) {
                var r = this._repositories[k];
                var obj: any = {};
                obj.id = k;
                obj.permit = r.CanRefresh !== true;
                obj.date = r.LastTime;
                obj.hottrack = this.IsShowHotTracking;
                values.push(obj);
            }
            return values;
        }

        public GetUnreachableRepositories(): Array<Repository> {
            var values = [];
            for (var k in this._repositories) {
                var r = this._repositories[k];
                if (r.Record.get('Speed') != -1) {
                    continue;
                }
                values.push(k);
            }
            return values;
        }

        private InitializeRepositories(records: Array<IRecord>): void {
            records.forEach(iRecord=> {
                this._repositories[iRecord.internalId] = new Repository(iRecord);
            });
        }

        private OnGpsReceived(sender: any, e: Controllers.GpsReceivedEventArgs): void {
            this.UpdateRepositories(e.Items);
        }

        private BeginHibernateGrid(): void {
            this._grid.store.suspendEvents();
        }

        private EndHibernateGrid(): void {
            this._grid.store.resumeEvents();
            this.RefreshGrid();
            this._view.FitToBounds();
        }

        public RefreshGrid(): void {
            this._grid.view.refresh(false);
        }

        private UpdateRepositories(items: Array<Models.Gps>): void {
            this.BeginHibernateGrid();
            items.forEach(gps=> {
                var repo = this._repositories[gps.TrackerId];
                repo.Update(gps);
            });
            this.EndHibernateGrid();
        }

        public StartAllTracking(): void {
            for (var repo in this._repositories) {
                if (this._repositories[repo].IsTracked) {
                    continue;
                }
                if (!this._repositories[repo].IsActive) {
                    continue;
                }
                this._repositories[repo].StartTracking();
            }
        }

        public StopAllTracking(): void {
            for (var repo in this._repositories) {
                if (!this._repositories[repo].IsTracked) {
                    continue;
                }
                this._repositories[repo].StopTracking();
            }
        }

        public Centralize(item: any, record: IRecord): void {
            this._repositories[record.internalId].Centralize();
        }

        public UpdateShowTracking(): void {
            for (var repo in this._repositories) {
                if (!this._repositories[repo].IsTracked) { continue; }
                if ($App.SettingsManager.Settings.ShowTracking) {
                    this._repositories[repo].Tracking.Show();
                } else {
                    this._repositories[repo].Tracking.Hide();
                }
            }
        }

        public ApplyCarsFilter(): void {
            this._grid.store.filterBy(this.FilterString);
            this.UpdatePrependText();
        }

        private FilterString(record: IRecord): string {
            var value = record.get('Name') ? record.get('Name').toLowerCase() : '',
                textFilterValue = (App.txtFilterValue.getValue() + '').toLowerCase();

            return (value.indexOf(textFilterValue) > -1 || value == '') && $App.RepositoryManager.GetFilterFunctionByType()(record);
        }

        public GetFilterFunctionByType(): any {
            var selectItem = App.btnFilterCarByState.activeItem.id;
            switch (selectItem) {
                case 'cmiAll':
                    return this.FilterCmiAll;
                case 'cmiActive':
                    return this.FilterCmiActive;
                case 'cmiInactive':
                    return this.FilterCmiInactive;
                case 'cmiRun':
                    return this.FilterCmiRun;
                case 'cmiStop':
                    return this.FilterCmiStop;
                default:
                    return this.FilterCmiAll;
            }
        }

        private FilterCmiAll(record) {
            return true;
        }

        private FilterCmiActive(record: IRecord) {
            return $App.RepositoryManager.Repositories[record.internalId].IsActive === true;
        }

        private FilterCmiInactive(record: IRecord) {
            return !$App.RepositoryManager.Repositories[record.internalId].IsActive;
        }

        private FilterCmiRun(record: IRecord) {
            return $App.RepositoryManager.Repositories[record.internalId].IsActive === true && $App.RepositoryManager.Repositories[record.internalId].IsRun === true;
        }

        private FilterCmiStop(record: IRecord) {
            return $App.RepositoryManager.Repositories[record.internalId].IsActive === true && $App.RepositoryManager.Repositories[record.internalId].IsRun === false;
        }

        private GetGridRowsCount(): number {
            return App.gridPanelCars.getRowsValues().length;
        }

        private UpdatePrependText(): void {
            var cmb = App.btnFilterCarByState,
                text = cmb.activeItem.text,
                count = this.GetGridRowsCount();
            cmb.setText(Ext.String.format('Авто(<b>{0}</b>): {1}', count, text));
        }
    }

    export class Repository {
        private _record: IRecord;
        private _isTracked: boolean = false;
        private _isActive: boolean;
        private _hasValue: boolean;
        private _connected: boolean;
        private _isRun: boolean;
        private _stopTimeMinutes: number;
        private _tracking: Views.Tracking;        

        public get Record(): IRecord {
            return this._record;
        }
        
        public get Record2(): IRecord {
            return this._record;
        }

        public get IsTracked(): boolean {
            return this._isTracked;
        }

        public set IsTracked(value: boolean) {
            this._isTracked = value;
        }

        public get CanRefresh(): boolean {
            return this.IsTracked;
        }

        public get LastTime(): Date {
            return this._record.get('EndSendTime') || this._record.get('LastSendTime');
        }

        public get IsActive(): boolean {
            return this._isActive;
        }

        public get HasValue(): boolean {
            return this._hasValue;
        }

        public get IsConnected(): boolean {
            return this._connected;
        }

        public get IsRun(): boolean {
            return this._isRun;
        }

        public get StopTimeMinutes(): number {
            return this._stopTimeMinutes;
        }

        public get Location(): any {
            return new L.LatLng(this._record.get('Latitude'), this._record.get('Longitude'));
        }

        public get Id(): number {
            return this._record.internalId;
        }

        public get Tracking(): Views.Tracking {
            return this._tracking;
        }

        public get Image(): CarImage {
            return new CarImage(this._record);
        }

        public get isSosAlarming(): boolean {
            var d = this._record.data;
            if (d.Sensor2 && d.Sensor2.sos === true && d.s && d.s.s2 === false) {//it's strange but if second sensor has 'false' as a value this means that sensor is in alarming state
                return true;
            }
            return false;
        }

        public get alarmingColor(): string {
            if (!this.isSosAlarming) { return ''; }
            return this._record.data.lastSos ? '#f5ff00' : '#fb7070';
        }

        constructor(record: IRecord) {
            this._record = record;
            this.UpdateState();
        }

        public updateSensors(newState: any): void {
            this._record.set('Relay', newState.r);
            this._record.set('Relay1', newState.r1);
            this._record.set('Relay2', newState.r2);
            this._record.set('Sensor1', newState.s1);
            this._record.set('Sensor2', newState.s2);
        }

        public updateImage(image: any) {
            this._record.set('Image', image);
        }

        public Update(gps: Models.Gps): void {
            this._record.set('LastSendTime', gps.SendTime);
            //if (gps.EndTime && gps.EndTime != null) {
            this._record.set('EndSendTime', gps.EndTime);
            //}
            this._record.set('Speed', gps.Speed);
            this._record.set('i', gps.GpsSignal);
            if (gps.Latitude) {
                this._record.set('Latitude', gps.Latitude);
            }
            if (gps.Longitude) {
                this._record.set('Longitude', gps.Longitude);
            }
            this._record.set('Battery', gps.Battery);
            this._record.set('s', { r: gps.Sensors.Relay, r1: gps.Sensors.Relay1, r2: gps.Sensors.Relay2, s1: gps.Sensors.Sensor1, s2: gps.Sensors.Sensor2 });
            this.UpdateState();
            this.UpdateTracking(gps);
        }

        private UpdateState(): void {
            this._isActive = false;
            this._connected = true;
            this._hasValue = true;
            this._isRun = false;

            var speed = this._record.get('Speed'),
                sendTime: Date = this.LastTime;
            this._hasValue = !!(sendTime);
            if (speed == -1) {
                this._hasValue = false;
                this._connected = false;
                return;
            } else if (this._hasValue) {
                this._stopTimeMinutes = Math.round((Date.now() - +sendTime) / 60000);
                if (this._stopTimeMinutes > 10) {
                    this._connected = false;
                    return;
                }
            }

            if (speed > 0) {
                this._isActive = true;
                this._isRun = true;
            } else if (speed == 0) {
                this._isActive = true;
            }
        }

        private UpdateTracking(gps: Models.Gps): void {
            if (this.IsTracked) {
                this._tracking.AddPoint(gps);
            }
        }

        public StartTracking(): void {
            if (this.IsTracked === true) {
                return;
            }
            this.IsTracked = true;
            this._tracking = new Views.Tracking(this);
            this._tracking.Start();
            $App.RepositoryManager.RefreshGrid();
        }

        public StopTracking(): void {
            if (this.IsTracked === false) {
                return;
            }
            this.IsTracked = false;
            this._tracking.Stop();
            delete this._tracking;
            $App.RepositoryManager.RefreshGrid();
        }

        public GetInfoWindowContent(): string {
            return Ext.String.format('<table><tr><td class="info-window-label">Авто:</td><td class="info-window-value">{0}</td></tr><tr><td class="info-window-label">Скорость:</td><td class="info-window-value">{1} км/ч</td></tr><tr><td class="info-window-label">Отправка:</td><td class="info-window-value">{2}</td></tr><tr><td class="info-window-label">Координаты:</td><td class="info-window-value">{3} х {4}</td></tr></table>',
                this._record.get('Name'), this._record.get('Speed'), Ext.Date.format(this._record.get('LastSendTime'), 'd-m-Y H:i:s'),
                this._record.get('Latitude'), this._record.get('Longitude'));
        }

        public Centralize(): void {
            if (!this.IsTracked) { return; }
            $App.Map.Map.panTo(this.Location, { duration: 1 });
        }
    }

    class SosHelper {
        private _intervalId: number;

        constructor(private _repositoryManager: RepositoryManager) {
            this.initTimer();
        }

        private initTimer(): void {
            this._intervalId = window.setInterval(() => this.doUpdate(), 1000);
        }

        private doUpdate(): void {
            if (this.isRefreshRequired() === true) {
                this._repositoryManager.RefreshGrid();

            }
        }

        private isRefreshRequired(): boolean {
            var isRequired: boolean = false;
            this._repositoryManager.Repositories.forEach(repo => {
                if (repo.isSosAlarming) {
                    isRequired = true;
                    if (repo.IsTracked) {
                        repo.Tracking.pointer.recalc();
                    }
                    if (this.isRepoEditing(repo)) {
                        $App.SettingsManager.CarSettings.displaySensorState(repo, 2);
                    }
                }
            });
            return isRequired;
        }

        private isRepoEditing(repo: Repository): boolean {
            var carSettings = $App.SettingsManager.CarSettings;
            if (!carSettings.Record) { return false; }
            return (carSettings.Record.id || carSettings.Record.Id) == repo.Record.get('Id');
        }
    }

    export class CarImage {
        private _carImage: any;

        public get name(): string {
            return this._carImage.name;
        }

        public get isDirectionRequired(): boolean {
            return this._carImage.isrecalc;
        }

        constructor(private record: IRecord) {
            this._carImage = this.record.get('Image');
        }
    }
}