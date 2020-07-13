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
var Core;
(function (Core) {
    var ConnectionState;
    (function (ConnectionState) {
        ConnectionState[ConnectionState["connected"] = 1] = "connected";
        ConnectionState[ConnectionState["connecting"] = 0] = "connecting";
        ConnectionState[ConnectionState["disconnected"] = 4] = "disconnected";
        ConnectionState[ConnectionState["reconnecting"] = 2] = "reconnecting";
    })(ConnectionState = Core.ConnectionState || (Core.ConnectionState = {}));
    var GpsAddedEventArgs = /** @class */ (function (_super) {
        __extends(GpsAddedEventArgs, _super);
        function GpsAddedEventArgs(gps) {
            var _this = _super.call(this) || this;
            _this.gps = gps;
            return _this;
        }
        Object.defineProperty(GpsAddedEventArgs.prototype, "Gps", {
            get: function () {
                return this.gps;
            },
            enumerable: true,
            configurable: true
        });
        return GpsAddedEventArgs;
    }(Core.EventArgs));
    Core.GpsAddedEventArgs = GpsAddedEventArgs;
    var ConnectionStateEventArgs = /** @class */ (function (_super) {
        __extends(ConnectionStateEventArgs, _super);
        function ConnectionStateEventArgs(state) {
            var _this = _super.call(this) || this;
            _this.state = state;
            return _this;
        }
        Object.defineProperty(ConnectionStateEventArgs.prototype, "State", {
            get: function () {
                return this.state;
            },
            enumerable: true,
            configurable: true
        });
        return ConnectionStateEventArgs;
    }(Core.EventArgs));
    Core.ConnectionStateEventArgs = ConnectionStateEventArgs;
    var ServerCommunication = /** @class */ (function () {
        function ServerCommunication(userId, ip) {
            this.userId = userId;
            this.ip = ip;
            this._gpsAdd = new Core.Delegate();
            this._connectionStateChanged = new Core.Delegate();
        }
        Object.defineProperty(ServerCommunication.prototype, "State", {
            get: function () {
                return $.connection.newState;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ServerCommunication.prototype, "GpsAdded", {
            get: function () {
                return this._gpsAdd;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ServerCommunication.prototype, "StateChanged", {
            get: function () {
                return this._connectionStateChanged;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ServerCommunication.prototype, "Hub", {
            get: function () {
                return this._mapHub;
            },
            enumerable: true,
            configurable: true
        });
        ServerCommunication.prototype.Start = function () {
            this.SetConnectionSettings();
            this.Connect();
        };
        ServerCommunication.prototype.OnGpsAdded = function (source) {
            this._gpsAdd.raise(this, new GpsAddedEventArgs(new Models.Gps(eval('(' + source + ')'))));
        };
        ServerCommunication.prototype.OnConnectionStateChanged = function (state) {
            this._connectionStateChanged.raise(this, new ConnectionStateEventArgs(state));
        };
        ServerCommunication.prototype.Connect = function () {
            var _this = this;
            this._mapHub = $.connection.mapHub;
            if (!this._mapHub) {
                setTimeout(function () { _this.Start(); }, 3000); // Restart connection after 3 seconds.
                return;
            }
            this._mapHub.state.userId = this.userId;
            this._mapHub.client.addGps = function (s) { return _this.OnGpsAdded(s); };
            this.AttachEventHandlers();
            $.connection.hub.start().done(function () { });
        };
        ServerCommunication.prototype.AttachEventHandlers = function () {
            var _this = this;
            $.connection.hub.stateChanged(function (e) {
                if (e.newState == 1) {
                    _this._mapHub.server.itsMe();
                    ;
                }
                $.connection.newState = e.newState;
                _this.OnConnectionStateChanged(e.newState);
            });
            $.connection.hub.disconnected(function () {
                setTimeout(function () {
                    $.connection.hub.start();
                }, 5000); // Restart connection after 5 seconds.
            });
        };
        ServerCommunication.prototype.SetConnectionSettings = function () {
            $.getScript("http://" + this.ip + ":8081/signalr/hubs").fail(function () { });
            $.connection.url = "http://" + this.ip + ":8081/";
            $.connection.hub.url = "http://" + this.ip + ":8081/signalr";
        };
        return ServerCommunication;
    }());
    Core.ServerCommunication = ServerCommunication;
})(Core || (Core = {}));
