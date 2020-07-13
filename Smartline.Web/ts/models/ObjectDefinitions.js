var Models;
(function (Models) {
    var Gps = /** @class */ (function () {
        function Gps(source) {
            this.Parse(source);
        }
        Gps.prototype.Parse = function (source) {
            if (!source) {
                return;
            }
            if (source.a) {
                this.SendTime = common.parseISO8601(source.a);
            }
            if (source.b && source.b != 0) {
                this.Latitude = source.b;
            }
            if (source.c && source.c != 0) {
                this.Longitude = source.c;
            }
            if (source.d) {
                this.Speed = source.d;
            }
            if (source.e) {
                this.TrackerId = source.e;
            }
            if (source.f) {
                this.EndTime = common.parseISO8601(source.f);
            }
            if (source.h) {
                this.Distance = source.h;
            }
            if (source.i) {
                this.GpsSignal = new GpsSignal(source.i);
            }
            if (source.k) {
                this.Battery = source.k;
            }
            if (source.s1) {
                this.Sos1 = source.s1;
            }
            if (source.s2) {
                this.Sos2 = source.s2;
            }
            if (source.s) {
                this.Sensors = new Sensors(source.s);
            }
        };
        return Gps;
    }());
    Models.Gps = Gps;
    var GpsSignal = /** @class */ (function () {
        function GpsSignal(source) {
            this.Parse(source);
        }
        GpsSignal.prototype.Parse = function (source) {
            if (!source) {
                return;
            }
            this.Active = source.a === true;
            if (source.d) {
                this.Date = source.d;
            }
        };
        return GpsSignal;
    }());
    Models.GpsSignal = GpsSignal;
    var Sensors = /** @class */ (function () {
        function Sensors(source) {
            this.Parse(source);
        }
        Sensors.prototype.Parse = function (source) {
            if (!source) {
                return;
            }
            this.Relay = source.r === true;
            this.Relay1 = source.r1 === true;
            this.Relay2 = source.r2 === true;
            this.Sensor1 = source.s1 === true;
            this.Sensor2 = source.s2 === true;
        };
        return Sensors;
    }());
    Models.Sensors = Sensors;
    var WindowSettings = /** @class */ (function () {
        function WindowSettings(source) {
            this.Parse(source);
        }
        WindowSettings.prototype.Parse = function (source) {
            if (!source) {
                return;
            }
            if (source.x) {
                this.X = source.x;
            }
            if (source.y) {
                this.Y = source.y;
            }
            if (source.w) {
                this.Width = source.w;
            }
            if (source.h) {
                this.Height = source.h;
            }
            if (source.collapsed) {
                this.IsCollapsed = source.collapsed;
            }
        };
        return WindowSettings;
    }());
    Models.WindowSettings = WindowSettings;
    var Settings = /** @class */ (function () {
        function Settings(source) {
            this.Parse(source);
        }
        Settings.prototype.Parse = function (source) {
            if (!source) {
                return;
            }
            if (source.i) {
                this.UserId = source.i;
            }
            if (source.t) {
                this.ShowTracking = source.t;
            }
            if (source.lat) {
                this.Latitude = source.lat;
            }
            if (source.long) {
                this.Longitude = source.long;
            }
            if (source.long) {
                this.Longitude = source.long;
            }
            if (source.p) {
                this.PointsInPath = source.p;
            }
            if (source.pan) {
                this.IsPan = source.pan;
            }
            this.Weight = Math.min(source.weight, 3) || 3;
            if (source.wndcars) {
                this.WindowSettings = new WindowSettings(source.wndcars);
            }
            this.SpeedLimits = new SpeedLimits(source.sl);
        };
        return Settings;
    }());
    Models.Settings = Settings;
    var SpeedLimits = /** @class */ (function () {
        function SpeedLimits(source) {
            if (source) {
                this.Parse(source);
            }
            else {
                this.CreateDefaults();
            }
        }
        SpeedLimits.prototype.Parse = function (source) {
            this.Limit1 = source.l1;
            this.Limit1Color = source.l1c;
            this.Limit2 = source.l2;
            this.Limit2Color = source.l2c;
            this.Limit3 = source.l3;
            this.Limit3Color = source.l3c;
        };
        SpeedLimits.prototype.CreateDefaults = function () {
            this.Limit1 = 60;
            this.Limit1Color = "#1900FF";
            this.Limit2 = 80;
            this.Limit2Color = "#059E21";
            this.Limit3 = 150;
            this.Limit3Color = "#FF0000";
        };
        return SpeedLimits;
    }());
    Models.SpeedLimits = SpeedLimits;
})(Models || (Models = {}));
