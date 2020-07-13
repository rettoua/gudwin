module Controllers {
    declare var $App: Core.Application;
    declare var Ext;
    declare var DR: IGpsLoader;

    interface IGpsLoader {
        GetRepositories(values: Array<any>, callback: Core.LoaderCallback);
        LoadUnreachableTrackers(values: Array<any>, callback: Core.LoaderCallback);
    }

    export class GpsReceivedEventArgs extends Core.EventArgs {
        private _items: Array<Models.Gps>;

        public get Items(): Array<Models.Gps> {
            return this._items;
        }

        constructor(items: Array<Models.Gps>) {
            super();
            this._items = items;
        }
    }

    export class GpsProvider extends Core.Updatable {
        private _timerLoadFromSite: number;
        private _timerLoadFromServer: number;
        private _timeoutLoadFromSite: number = 4000;
        private _timeoutLoadFromServer: number = 1000;
        private _gpsReceived: Core.Delegate<GpsReceivedEventArgs>;
        private _gpsLoadEventHandler: Core.EventHandler<Core.GpsAddedEventArgs>;
        private _gpsBuffer: Array<Models.Gps>;

        private get IsUseSignalR(): boolean {
            return $App.ServerCommunication.State == Core.ConnectionState.connected;
        }

        public get GpsReceived(): Core.Delegate<GpsReceivedEventArgs> {
            return this._gpsReceived;
        }

        constructor() {
            super();
            this._gpsReceived = new Core.Delegate<GpsReceivedEventArgs>();
            this._gpsLoadEventHandler = new Core.EventHandler<Core.GpsAddedEventArgs>((s, e) => this.OnGpsAdded(s, e));
            this._gpsBuffer = new Array<Models.Gps>();
        }

        public Start(): void {
            this.InitializeTimer();
            $App.ServerCommunication.GpsAdded.subscribe(this._gpsLoadEventHandler);
        }

        public LoadUnreachableData(): void {
            this.LoadUnreachableRepositories();
        }

        private InitializeTimer(): void {
            this._timerLoadFromSite = setInterval(Ext.Function.bind(this.LoadRepositories, this), this._timeoutLoadFromSite);
            this._timerLoadFromServer = setInterval(Ext.Function.bind(this.RefreshRepository, this), this._timeoutLoadFromServer);
        }

        private GetRepositoriesForRefresh(): Array<any> {
            return $App.RepositoryManager.GetRepositoriesForRefresh();
        }

        private GetUnreachableRepositoriesForRefresh(): Array<any> {
            return $App.RepositoryManager.GetUnreachableRepositories();
        }

        private SuccessCallback(items: Array<any>): void {
            this.EndUpdate();
            if (items.length == 0) {
                return;
            }
            var newItems: Array<Models.Gps> = [];
            items.forEach(value=> { newItems.push(new Models.Gps(value)); });
            this.OnGpsReceived(newItems);
        }

        private FailureCallback(error: any): void {
            this.EndUpdate();
        }

        private LoadRepositories(): void {
            if (this.IsUseSignalR === true || this.IsUpdating) {
                return;
            }
            var repos: Array<any> = this.GetRepositoriesForRefresh();
            if (repos.length == 0) {
                return;
            }
            this.BeginUpdate();
            DR.GetRepositories(repos, { success: (values: any) => { this.SuccessCallback(values); }, failure: (error: string) => { this.FailureCallback(error); } });
        }

        private LoadUnreachableRepositories(): void {
            var repos: Array<any> = this.GetUnreachableRepositoriesForRefresh();
            if (repos.length == 0) {
                return;
            }
            this.BeginUpdate();
            DR.LoadUnreachableTrackers(repos, { success: (values: any) => { this.SuccessCallback(values); }, failure: (error: string) => { this.FailureCallback(error); } });
        }

        private RefreshRepository(): void {
            if (this.IsUseSignalR === false) {
                return;
            }
            var buffer = this._gpsBuffer;
            this._gpsBuffer = new Array<Models.Gps>();
            this.OnGpsReceived(buffer);
        }

        private OnGpsAdded(sender: any, e: Core.GpsAddedEventArgs): void {
            this._gpsBuffer.push(e.Gps);
        }

        private OnGpsReceived(items: Array<Models.Gps>): void {
            this._gpsReceived.raise(this, new GpsReceivedEventArgs(items));
        }
    }

}