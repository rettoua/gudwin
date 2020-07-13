
module Core {
    declare var $;

    export enum ConnectionState { connected = 1, connecting = 0, disconnected= 4, reconnecting= 2 }

    export class GpsAddedEventArgs extends EventArgs {

        public get Gps(): Models.Gps {
            return this.gps;
        }

        constructor(private gps: Models.Gps) {
            super();
        }

    }

    export class ConnectionStateEventArgs extends EventArgs {

        public get State(): ConnectionState {
            return this.state;
        }

        constructor(private state: ConnectionState) {
            super();
        }

    }

    export class ServerCommunication {
        private _mapHub: any;
        private _gpsAdd: Delegate<GpsAddedEventArgs>;
        private _connectionStateChanged: Delegate<ConnectionStateEventArgs>;

        public get State(): ConnectionState {
            return $.connection.newState;
        }

        public get GpsAdded(): Delegate<GpsAddedEventArgs> {
            return this._gpsAdd;
        }

        public get StateChanged(): Delegate<ConnectionStateEventArgs> {
            return this._connectionStateChanged;
        }

        public get Hub(): any {
            return this._mapHub;
        }

        constructor(private userId: number, private ip: string) {
            this._gpsAdd = new Delegate<GpsAddedEventArgs>();
            this._connectionStateChanged = new Delegate<ConnectionStateEventArgs>();
        }

        public Start(): void {
            this.SetConnectionSettings();
            this.Connect();
        }

        private OnGpsAdded(source: any): void {
            this._gpsAdd.raise(this, new GpsAddedEventArgs(new Models.Gps(eval('(' + source + ')'))));
        }

        private OnConnectionStateChanged(state: ConnectionState): void {
            this._connectionStateChanged.raise(this, new ConnectionStateEventArgs(state));
        }

        private Connect(): void {
            this._mapHub = $.connection.mapHub;
            if (!this._mapHub) {
                setTimeout(() => { this.Start(); }, 3000); // Restart connection after 3 seconds.
                return;
            }
            this._mapHub.state.userId = this.userId;
            this._mapHub.client.addGps = (s) => this.OnGpsAdded(s);
            this.AttachEventHandlers();
            $.connection.hub.start().done(() => { });
        }

        private AttachEventHandlers(): void {
            $.connection.hub.stateChanged(e=> {
                if (e.newState == 1) {
                    this._mapHub.server.itsMe();;
                }
                $.connection.newState = e.newState;
                this.OnConnectionStateChanged(e.newState);
            });

            $.connection.hub.disconnected(() => {
                setTimeout(() => {
                    $.connection.hub.start();
                }, 5000); // Restart connection after 5 seconds.
            });
        }

        private SetConnectionSettings(): void {
            $.getScript("http://" + this.ip + ":8081/signalr/hubs").fail(() => { });
            $.connection.url = "http://" + this.ip + ":8081/";
            $.connection.hub.url = "http://" + this.ip + ":8081/signalr";
        }

    }

}