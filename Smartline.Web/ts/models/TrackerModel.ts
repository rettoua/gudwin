module models {
    export class Tracker {
        uid: string;
        trackerId: number;
        name: string;
        description: string;
        speed: number;
        sendTime: Date;
        lastTime: Date;
        latitude: number;
        longitude: number;


        virtualName: string;
        virtualId: number;
        color: string;
        hevos: boolean;
        consumption: string;

        constructor(source: any) {
            this.setData(source);
        }
        setData(source: any): void {
            if (typeof source == "undefined" || source == null) { return; }
            this.uid = source.Id;
            this.trackerId = source.TrackerId;
            this.name = source.Name;
            this.description = source.Description;
            //todo: set all necessary stuff
        }
        update(source: any): void {
                        
        }
    }
} 