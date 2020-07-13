var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var Controllers;
(function (Controllers) {
    var GpsReceivedEventArgs = /** @class */ (function (_super) {
        __extends(GpsReceivedEventArgs, _super);
        function GpsReceivedEventArgs(items) {
            var _this = _super.call(this) || this;
            _this._items = items;
            return _this;
        }
        Object.defineProperty(GpsReceivedEventArgs.prototype, "Items", {
            get: function () {
                return this._items;
            },
            enumerable: true,
            configurable: true
        });
        return GpsReceivedEventArgs;
    }(Core.EventArgs));
    Controllers.GpsReceivedEventArgs = GpsReceivedEventArgs;
    var GpsProvider = /** @class */ (function (_super) {
        __extends(GpsProvider, _super);
        function GpsProvider() {
            var _this = _super.call(this) || this;
            _this._timeoutLoadFromSite = 4000;
            _this._timeoutLoadFromServer = 1000;
            _this._gpsReceived = new Core.Delegate();
            _this._gpsLoadEventHandler = new Core.EventHandler(function (s, e) { return _this.OnGpsAdded(s, e); });
            _this._gpsBuffer = new Array();
            return _this;
        }
        Object.defineProperty(GpsProvider.prototype, "IsUseSignalR", {
            get: function () {
                return $App.ServerCommunication.State == Core.ConnectionState.connected;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(GpsProvider.prototype, "GpsReceived", {
            get: function () {
                return this._gpsReceived;
            },
            enumerable: true,
            configurable: true
        });
        GpsProvider.prototype.Start = function () {
            this.InitializeTimer();
            $App.ServerCommunication.GpsAdded.subscribe(this._gpsLoadEventHandler);
        };
        GpsProvider.prototype.LoadUnreachableData = function () {
            this.LoadUnreachableRepositories();
        };
        GpsProvider.prototype.InitializeTimer = function () {
            this._timerLoadFromSite = setInterval(Ext.Function.bind(this.LoadRepositories, this), this._timeoutLoadFromSite);
            this._timerLoadFromServer = setInterval(Ext.Function.bind(this.RefreshRepository, this), this._timeoutLoadFromServer);
        };
        GpsProvider.prototype.GetRepositoriesForRefresh = function () {
            return $App.RepositoryManager.GetRepositoriesForRefresh();
        };
        GpsProvider.prototype.GetUnreachableRepositoriesForRefresh = function () {
            return $App.RepositoryManager.GetUnreachableRepositories();
        };
        GpsProvider.prototype.SuccessCallback = function (items) {
            this.EndUpdate();
            if (items.length == 0) {
                return;
            }
            var newItems = [];
            items.forEach(function (value) { newItems.push(new Models.Gps(value)); });
            this.OnGpsReceived(newItems);
        };
        GpsProvider.prototype.FailureCallback = function (error) {
            this.EndUpdate();
        };
        GpsProvider.prototype.LoadRepositories = function () {
            var _this = this;
            if (this.IsUseSignalR === true || this.IsUpdating) {
                return;
            }
            var repos = this.GetRepositoriesForRefresh();
            if (repos.length == 0) {
                return;
            }
            this.BeginUpdate();
            DR.GetRepositories(repos, { success: function (values) { _this.SuccessCallback(values); }, failure: function (error) { _this.FailureCallback(error); } });
        };
        GpsProvider.prototype.LoadUnreachableRepositories = function () {
            var _this = this;
            var repos = this.GetUnreachableRepositoriesForRefresh();
            if (repos.length == 0) {
                return;
            }
            this.BeginUpdate();
            DR.LoadUnreachableTrackers(repos, { success: function (values) { _this.SuccessCallback(values); }, failure: function (error) { _this.FailureCallback(error); } });
        };
        GpsProvider.prototype.RefreshRepository = function () {
            if (this.IsUseSignalR === false) {
                return;
            }
            var buffer = this._gpsBuffer;
            this._gpsBuffer = new Array();
            this.OnGpsReceived(buffer);
        };
        GpsProvider.prototype.OnGpsAdded = function (sender, e) {
            this._gpsBuffer.push(e.Gps);
        };
        GpsProvider.prototype.OnGpsReceived = function (items) {
            this._gpsReceived.raise(this, new GpsReceivedEventArgs(items));
        };
        return GpsProvider;
    }(Core.Updatable));
    Controllers.GpsProvider = GpsProvider;
})(Controllers || (Controllers = {}));
