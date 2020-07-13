module Core {
    declare var App: any;
    declare var $App: Core.Application;

    export class SensorHistory {
        private _isCarLoaded: boolean = false;

        public showWindow(): void {
            this.loadCars();
            App.pnlSensorHistory.show();
        }

        private loadCars(): void {
            if (this._isCarLoaded) { return; }
            var combo = App.cmbCarsSensorHistory;
            var firstIndex = 0,
                repos = $App.RepositoryManager.Repositories;
            for (var i in repos) {
                combo.addItem(repos[i].Record.get('Name'), repos[i].Record.internalId);
                if (firstIndex == 0)
                    firstIndex = repos[i].Record.internalId;
            }
            combo.setValue(firstIndex);
            App.fldSensorHistoryDateFrom.setValue(new Date());
            App.fldSensorHistoryDateTo.setValue(new Date());
            this._isCarLoaded = true;
        }
    }
}