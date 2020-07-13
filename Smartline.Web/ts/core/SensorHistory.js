var Core;
(function (Core) {
    var SensorHistory = /** @class */ (function () {
        function SensorHistory() {
            this._isCarLoaded = false;
        }
        SensorHistory.prototype.showWindow = function () {
            this.loadCars();
            App.pnlSensorHistory.show();
        };
        SensorHistory.prototype.loadCars = function () {
            if (this._isCarLoaded) {
                return;
            }
            var combo = App.cmbCarsSensorHistory;
            var firstIndex = 0, repos = $App.RepositoryManager.Repositories;
            for (var i in repos) {
                combo.addItem(repos[i].Record.get('Name'), repos[i].Record.internalId);
                if (firstIndex == 0)
                    firstIndex = repos[i].Record.internalId;
            }
            combo.setValue(firstIndex);
            App.fldSensorHistoryDateFrom.setValue(new Date());
            App.fldSensorHistoryDateTo.setValue(new Date());
            this._isCarLoaded = true;
        };
        return SensorHistory;
    }());
    Core.SensorHistory = SensorHistory;
})(Core || (Core = {}));
