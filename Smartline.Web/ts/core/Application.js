var Core;
(function (Core) {
    var Application = /** @class */ (function () {
        function Application() {
            this._repositoryManager = new Core.RepositoryManager();
            this._sensorHistory = new Core.SensorHistory();
        }
        Object.defineProperty(Application.prototype, "SettingsManager", {
            get: function () {
                return this._settingsManager;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Application.prototype, "RepositoryManager", {
            get: function () {
                return this._repositoryManager;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Application.prototype, "ServerCommunication", {
            get: function () {
                return this._serverCommunication;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Application.prototype, "Map", {
            get: function () {
                return this._map;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Application.prototype, "SensorHistory", {
            get: function () {
                return this._sensorHistory;
            },
            enumerable: true,
            configurable: true
        });
        Application.prototype.LoadSettings = function () {
            this._settingsManager = new Core.SettingsManager();
            this._settingsManager.ApplySettings(globalUserSettings);
        };
        Application.prototype.Start = function () {
            if (!this._serverCommunication) {
                this._serverCommunication = new Core.ServerCommunication(this._settingsManager.Settings.UserId, signalrUrl);
            }
            this._serverCommunication.Start();
        };
        Application.prototype.CreateMap = function (element) {
            this._map = new Core.Map(element);
        };
        return Application;
    }());
    Core.Application = Application;
})(Core || (Core = {}));
