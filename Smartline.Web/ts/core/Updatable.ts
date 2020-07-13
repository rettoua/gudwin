module Core {

    export class Updatable {
        private _updateCounter: number=0;

        public get IsUpdating(): boolean {
            return this._updateCounter != 0;
        }

        public BeginUpdate(): void {
            this._updateCounter++;
        }

        public EndUpdate(): void {
            this._updateCounter--;
        }

    }
}