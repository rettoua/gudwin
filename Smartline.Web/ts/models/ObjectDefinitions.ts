module Models {
    declare var common;
    export class Gps {
        public SendTime: Date;
        public Latitude: number;
        public Longitude: number;
        public Speed: number;
        public TrackerId: number;
        public EndTime: Date;
        public Distance: number;
        public Battery: number;
        public GpsSignal: GpsSignal;
        public Sensors: Sensors;
        public Sos1: boolean;
        public Sos2: boolean;

        constructor(source: any) {
            this.Parse(source);
        }

        private Parse(source: any): void {
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
        }

    }

    export class GpsSignal {
        public Active: boolean;
        public Date: Date;

        constructor(source: any) {
            this.Parse(source);
        }

        private Parse(source: any): void {
            if (!source) {
                return;
            }
            this.Active = source.a === true;
            if (source.d) {
                this.Date = source.d;
            }
        }

    }

    export class Sensors {
        public Relay: boolean;
        public Relay1: boolean;
        public Relay2: boolean;
        public Sensor1: boolean;
        public Sensor2: boolean;

        constructor(source: any) {
            this.Parse(source);
        }

        private Parse(source: any): void {
            if (!source) {
                return;
            }
            this.Relay = source.r === true;
            this.Relay1 = source.r1 === true;
            this.Relay2 = source.r2 === true;
            this.Sensor1 = source.s1 === true;
            this.Sensor2 = source.s2 === true;
        }
    }

    export class WindowSettings {
        public X: number;
        public Y: number;
        public Width: number;
        public Height: number;
        public IsCollapsed: boolean;

        constructor(source: any) {
            this.Parse(source);
        }

        private Parse(source: any): void {
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
        }
    }

    export class Settings {
        public UserId: number;
        public ShowTracking: boolean;
        public Latitude: number;
        public Longitude: number;
        public PointsInPath: number;
        public IsPan: boolean;
        public Weight: number;
        public WindowSettings: WindowSettings;
        public SpeedLimits: SpeedLimits;

        constructor(source: any) {
            this.Parse(source);
        }

        private Parse(source: any): void {
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
        }

    }

    export class SpeedLimits {
        public Limit1: number;
        public Limit1Color: string;

        public Limit2: number;
        public Limit2Color: string;

        public Limit3: number;
        public Limit3Color: string;

        constructor(source: any) {
            if (source) {
                this.Parse(source);
            } else {
                this.CreateDefaults();
            }
        }

        public Parse(source: any) {
            this.Limit1 = source.l1;
            this.Limit1Color = source.l1c;

            this.Limit2 = source.l2;
            this.Limit2Color = source.l2c;

            this.Limit3 = source.l3;
            this.Limit3Color = source.l3c;
        }

        private CreateDefaults(): void {
            this.Limit1 = 60;
            this.Limit1Color = "#1900FF";

            this.Limit2 = 80;
            this.Limit2Color = "#059E21";

            this.Limit3 = 150;
            this.Limit3Color = "#FF0000";
        }
    }
}