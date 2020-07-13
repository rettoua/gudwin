module Core {
    declare var L;
    declare var globalUserSettings;
    declare var signalrUrl: string;
    declare var App;

    export interface LoaderCallback {
        success(value: any): void;
        failure(error: string): void;
    }

    export class Application {
        private _map: Map;
        private _serverCommunication: ServerCommunication;
        private _settingsManager: SettingsManager;
        private _repositoryManager: RepositoryManager;
        private _sensorHistory: SensorHistory;

        public get SettingsManager(): SettingsManager {
            return this._settingsManager;
        }

        public get RepositoryManager(): RepositoryManager {
            return this._repositoryManager;
        }

        public get ServerCommunication(): ServerCommunication {
            return this._serverCommunication;
        }

        public get Map(): Map {
            return this._map;
        }

        public get SensorHistory(): SensorHistory {
            return this._sensorHistory;
        }

        constructor() {
            this._repositoryManager = new RepositoryManager();
            this._sensorHistory = new SensorHistory();
        }

        public LoadSettings(): void {
            this._settingsManager = new SettingsManager();
            this._settingsManager.ApplySettings(globalUserSettings);
        }

        public Start(): void {
            if (!this._serverCommunication) {
                this._serverCommunication = new ServerCommunication(this._settingsManager.Settings.UserId, signalrUrl);
            }
            this._serverCommunication.Start();
        }

        public CreateMap(element: string): void {
            this._map = new Map(element);
        }
    }
}
