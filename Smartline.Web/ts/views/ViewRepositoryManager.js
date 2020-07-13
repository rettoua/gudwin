var Views;
(function (Views) {
    var ViewRepositoryManager = /** @class */ (function () {
        function ViewRepositoryManager() {
        }
        Object.defineProperty(ViewRepositoryManager.prototype, "ShowPointsInTracking", {
            get: function () {
                return App.btnShowPoints.pressed;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(ViewRepositoryManager.prototype, "ShowSpeedInTracking", {
            get: function () {
                return App.btnShowSpeed.pressed;
            },
            enumerable: true,
            configurable: true
        });
        ViewRepositoryManager.prototype.GetRepository = function (id) {
            return $App.RepositoryManager.Repositories[id];
        };
        ViewRepositoryManager.prototype.RenderName = function (value, meta, record) {
            var repo = this.GetRepository(record.internalId);
            if (!repo) {
                return value;
            }
            return this.RenderNameCore(repo);
        };
        ViewRepositoryManager.prototype.RenderNameCore = function (repo) {
            var gpsSignal = repo.Record.get('i') && repo.Record.get('i').Active === false ? '<img src="Resources/no-signal_16.png" style="padding-right: 4px;height: 11px;"/>' : '';
            if (repo.IsActive) {
                return Ext.String.format("{0}<b>{1}</b>", gpsSignal, repo.Record.get('Name'));
            }
            else {
                return Ext.String.format('{0}<span style="color: grey;"><b>{1}</b></span>', gpsSignal, repo.Record.get('Name'));
            }
        };
        ViewRepositoryManager.prototype.RenderToolbar = function (grid, command, record) {
            var repo = this.GetRepository(record.internalId);
            if (!repo) {
                return;
            }
            this.RenderToolbarCore(repo, command);
        };
        ViewRepositoryManager.prototype.RenderToolbarCore = function (repo, command) {
            if (command.command == 'CarStart') {
                command.hidden = repo.IsTracked || !repo.HasValue;
            }
            if (command.command == 'CarStop') {
                command.hidden = !repo.IsTracked;
            }
            if (command.command == 'Battery') {
                var battery = repo.Record.get('Battery');
                if (!battery || battery == -1) {
                    command.hidden = true;
                    command.hideMode = 'visibility'; //you can try 'display' also 
                }
                else {
                    command.hidden = false;
                    command.iconCls = Ext.String.format('icon-battery s{0}', battery);
                    command.qtext = Ext.String.format('Питание от батареи. Заряд {0}%', battery);
                }
            }
        };
        ViewRepositoryManager.prototype.RenderSpeed = function (value, meta, record) {
            var repo = this.GetRepository(record.internalId);
            if (!repo) {
                return value;
            }
            return this.RenderSpeedCore(repo) || value;
        };
        ViewRepositoryManager.prototype.RenderSpeedCore = function (repo) {
            if (!repo.IsConnected && !repo.HasValue) {
                return this.GreyOut('<span style="font-style: italic;">нет данных</span>');
            }
            if (repo.IsActive && repo.IsRun) {
                return Ext.String.format('<span style="color: green;font-weight: bold;">{0} км/ч</span>', repo.Record.get('Speed'));
            }
            if (repo.IsActive) {
                if (repo.Record.get('EndSendTime') && repo.Record.get('LastSendTime')) {
                    var parking = (repo.Record.get('EndSendTime') - repo.Record.get('LastSendTime')) / 1000;
                    var readabletime = common.timeToHumanReadable(parking);
                    return '<span style="color: blue;">' + readabletime + '</span>';
                }
                return '<span style="color: blue;">Стоит...</span>';
            }
            if (!repo.IsConnected) {
                if (repo.StopTimeMinutes > 10 && repo.StopTimeMinutes < 59) {
                    return this.GreyOut(Ext.String.format("Нет данных <b>{0}</b> мин.", repo.StopTimeMinutes));
                }
                else if (repo.StopTimeMinutes >= 60) {
                    if (Ext.Date.format(repo.LastTime, 'd-m-Y') == Ext.Date.format(Date.now(), 'd-m-Y')) {
                        return this.GreyOut(Ext.Date.format(repo.LastTime, 'H:i:s'));
                    }
                    return this.GreyOut(Ext.Date.format(repo.LastTime, 'd-m-Y H:i:s'));
                }
            }
            return '';
        };
        ViewRepositoryManager.prototype.GreyOut = function (value) {
            return Ext.String.format('<span style="color: grey;">{0}</span>', value);
        };
        ViewRepositoryManager.prototype.ExecuteCommand = function (value, command, record) {
            var repo = this.GetRepository(record.internalId);
            if (!repo) {
                return;
            }
            switch (command) {
                case 'CarStart':
                    {
                        repo.StartTracking();
                    }
                    break;
                case 'CarStop':
                    {
                        repo.StopTracking();
                    }
                    break;
            }
        };
        ViewRepositoryManager.prototype.FitToBounds = function () {
            if (!$App.SettingsManager.Settings.IsPan) {
                return;
            }
            var trackedRepository = [];
            var repositories = $App.RepositoryManager.Repositories;
            for (var k in repositories) {
                if (repositories[k].IsTracked) {
                    trackedRepository.push(repositories[k]);
                }
            }
            if (trackedRepository.length == 0) {
                return;
            }
            if (trackedRepository.length == 1) {
                //if (a_r[0] != this.repository) {
                //    return;
                //}
                if ($App.Map.Map.getBounds().contains(trackedRepository[0].Location) === false) {
                    trackedRepository[0].Centralize();
                }
                return;
            }
            else {
                var bounds = $App.Map.Map.getBounds(), inBounds = true, latLongs = [];
                for (var i = 0; i < trackedRepository.length; i++) {
                    latLongs.push(trackedRepository[i].Location);
                    if (bounds.contains(trackedRepository[i].Location) === false) {
                        inBounds = false;
                    }
                }
                if (!inBounds) {
                    var newBounds = L.latLngBounds(latLongs);
                    $App.Map.Map.fitBounds(newBounds);
                }
            }
        };
        ViewRepositoryManager.prototype.VisualRowClass = function (record) {
            var r = $App.RepositoryManager.Repositories[record.internalId];
            if (r) {
                if (r.isSosAlarming) {
                    if (r.Record.data.lastSos === true) {
                        r.Record.data.lastSos = false;
                        return 'repository-sos';
                    }
                    else {
                        r.Record.data.lastSos = true;
                        return 'repository-sos-lite';
                    }
                }
                if (r.IsTracked) {
                    return 'repository-run';
                }
            }
            return '';
        };
        ViewRepositoryManager.prototype.FromDateChanged = function () {
            if (App.dfFrom.getValue() > App.dfTo.getValue()) {
                App.dfTo.setValue(App.dfFrom.getValue());
            }
            else {
                var diff = App.dfTo.getValue() - App.dfFrom.getValue();
                var days = Math.round(diff / 86400000);
                if (days > 2) {
                    var v = App.dfFrom.getValue();
                    v.setDate(v.getDate() + 2);
                    App.dfTo.setValue(v);
                }
            }
        };
        ViewRepositoryManager.prototype.ToDateChanged = function () {
            if (App.dfTo.getValue() < App.dfFrom.getValue()) {
                App.dfFrom.setValue(App.dfTo.getValue());
            }
            else {
                var diff = App.dfTo.getValue() - App.dfFrom.getValue();
                var days = Math.round(diff / 86400000);
                if (days > 2) {
                    var v = App.dfTo.getValue();
                    v.setDate(v.getDate() - 2);
                    App.dfFrom.setValue(v);
                }
            }
        };
        ViewRepositoryManager.prototype.FromDateChangedSpeed = function () {
            if (App.dfFromSpeed.getValue() > App.dfToSpeed.getValue()) {
                App.dfToSpeed.setValue(App.dfFromSpeed.getValue());
            }
            else {
                var diff = App.dfToSpeed.getValue() - App.dfFromSpeed.getValue();
                var days = Math.round(diff / 86400000);
                if (days > 2) {
                    var v = App.dfFromSpeed.getValue();
                    v.setDate(v.getDate() + 2);
                    App.dfToSpeed.setValue(v);
                }
            }
        };
        ViewRepositoryManager.prototype.ToDateChangedSpeed = function () {
            if (App.dfToSpeed.getValue() < App.dfFromSpeed.getValue()) {
                App.dfFromSpeed.setValue(App.dfToSpeed.getValue());
            }
            else {
                var diff = App.dfToSpeed.getValue() - App.dfFromSpeed.getValue();
                var days = Math.round(diff / 86400000);
                if (days > 2) {
                    var v = App.dfToSpeed.getValue();
                    v.setDate(v.getDate() - 2);
                    App.dfFromSpeed.setValue(v);
                }
            }
        };
        ViewRepositoryManager.GetTrackingPointColor = function (item, record) {
            if (!item || !$App.RepositoryManager.View.ShowSpeedInTracking) {
                return record.get('Color');
            }
            return ViewRepositoryManager.GetColorBySpeed(item.d);
        };
        ViewRepositoryManager.GetSpeedIndex = function (item) {
            if (!item || item.d <= $App.SettingsManager.Settings.SpeedLimits.Limit1) {
                return 1;
            }
            if (item.d <= $App.SettingsManager.Settings.SpeedLimits.Limit2) {
                return 2;
            }
            return 3;
        };
        ViewRepositoryManager.GetColorBySpeed = function (speed) {
            if (speed <= $App.SettingsManager.Settings.SpeedLimits.Limit1) {
                return $App.SettingsManager.Settings.SpeedLimits.Limit1Color;
            }
            if (speed <= $App.SettingsManager.Settings.SpeedLimits.Limit2) {
                return $App.SettingsManager.Settings.SpeedLimits.Limit2Color;
            }
            return $App.SettingsManager.Settings.SpeedLimits.Limit3Color;
        };
        ViewRepositoryManager.GetColorByIndex = function (index, record) {
            if (!$App.RepositoryManager.View.ShowSpeedInTracking) {
                return record.get('Color');
            }
            if (index == 1) {
                return $App.SettingsManager.Settings.SpeedLimits.Limit1Color;
            }
            if (index == 2) {
                return $App.SettingsManager.Settings.SpeedLimits.Limit2Color;
            }
            return $App.SettingsManager.Settings.SpeedLimits.Limit3Color;
        };
        return ViewRepositoryManager;
    }());
    Views.ViewRepositoryManager = ViewRepositoryManager;
})(Views || (Views = {}));
