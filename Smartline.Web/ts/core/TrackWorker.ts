module Core {
    export class TrackWorker {
        private trackItemsCache: TrackItem[] = [];
        private currentTrackItem: TrackItem;

        public show(trackerUid: number): void {
            this.trackItemsCache[trackerUid] && this.doShow(this.trackItemsCache[trackerUid]);
        }

        private doShow(item: TrackItem): void {
            TrackView.display(item);
        }

        public hide(): void {
            if (this.currentTrackItem) {

            }
        }

        private doHide(): void {

        }
    }

    class TrackItem {
        public created: boolean;

        constructor(private source: any[]) { }
    }

    class TrackView {
        public static display(item: TrackItem): void {
            if (item.created) {

            }
            this.processDissplay(item);
        }

        private createView(item: TrackItem): void {

        }

        private static processDissplay(item: TrackItem): void {

        }

        public static hide(item: TrackItem): void {

        }

        public static remove(item: TrackItem): void {

        }
    }
}