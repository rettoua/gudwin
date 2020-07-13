var Core;
(function (Core) {
    var SettingsManager = /** @class */ (function () {
        function SettingsManager() {
        }
        Object.defineProperty(SettingsManager.prototype, "Settings", {
            get: function () {
                return this._settings;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(SettingsManager.prototype, "CarSettings", {
            get: function () {
                return this._carSettings;
            },
            enumerable: true,
            configurable: true
        });
        SettingsManager.prototype.ApplySettings = function (source) {
            this._settings = new Models.Settings(source);
            this._carSettings = new Core.CarSettings();
            this.Apply();
        };
        SettingsManager.prototype.Apply = function () {
            var wnd = App.pnlCars, wndSettings = $App.SettingsManager.Settings.WindowSettings;
            wnd.suspendEvents();
            wnd.setWidth(wndSettings.Width);
            wnd.setHeight(wndSettings.Height);
            wnd[wndSettings.IsCollapsed === true ? 'collapse' : 'expand']();
            var h = window.innerHeight || window.clientHeight, w = window.innerWidth || window.clientWidth, newX, newY;
            newX = wndSettings.X * w / 100;
            newY = wndSettings.Y * h / 100;
            wnd.setX(newX);
            wnd.setY(newY);
            wnd.resumeEvents();
            wnd.addListener('resize', function () { Core.SettingsManager.WndResized(this); });
            wnd.addListener('expand', function () { Core.SettingsManager.WndExpandedState(false); });
            wnd.addListener('collapse', function () { Core.SettingsManager.WndExpandedState(true); });
            wnd.addListener('move', function () { Core.SettingsManager.WndLocationChanged(this); });
        };
        SettingsManager.WndResized = function (wnd) {
            DR.UpdateWindowSize(wnd.width, wnd.height, { failure: function (e) { } });
        };
        SettingsManager.WndExpandedState = function (state) {
            DR.UpdateWindowExpandedState(state, { failure: function (e) { } });
        };
        SettingsManager.WndLocationChanged = function (wnd) {
            var h = window.innerHeight || window.clientHeight, w = window.innerWidth || window.clientWidth, newX, newY;
            newX = wnd.x * 100 / w;
            newY = wnd.y * 100 / h;
            DR.UpdateWindowLocation(newX, newY, { failure: function (e) { } });
        };
        SettingsManager.prototype.ChangePan = function (button) {
            this._settings.IsPan = button.pressed;
            DR.UpdateSettingsPan(this._settings.IsPan, {
                failure: function (e) { }
            });
        };
        SettingsManager.prototype.ChangeTrackingVisibility = function (button) {
            this._settings.ShowTracking = button.pressed;
            DR.UpdateSettingsShowHideTracking(button.pressed, {
                failure: function (e) { }
            });
            $App.RepositoryManager.UpdateShowTracking();
        };
        SettingsManager.prototype.EditSpeedLiminits = function () {
            App.fldSpeedLimitsL1.setValue(this._settings.SpeedLimits.Limit1);
            App.fldSpeedLimitsL2.setValue(this._settings.SpeedLimits.Limit2);
            App.fldSpeedLimitsL3.setValue(this._settings.SpeedLimits.Limit3);
            App.fldSpeedLimitL1c.setValue(this._settings.SpeedLimits.Limit1Color);
            ;
            App.fldSpeedLimitL2c.setValue(this._settings.SpeedLimits.Limit2Color);
            ;
            App.fldSpeedLimitL3c.setValue(this._settings.SpeedLimits.Limit3Color);
            ;
            App.wndEditSpeedLimits.show();
        };
        SettingsManager.prototype.SpeedLimit1Changed = function () {
            this.CheckSaveSpeedLimitButtonState();
            var v = App.fldSpeedLimitsL1.getValue();
            if (v >= App.fldSpeedLimitsL2.getValue()) {
                App.fldSpeedLimitsL2.setValue(++v);
            }
        };
        SettingsManager.prototype.SpeedLimit2Changed = function () {
            this.CheckSaveSpeedLimitButtonState();
            var v = App.fldSpeedLimitsL2.getValue();
            if (v >= App.fldSpeedLimitsL3.getValue()) {
                App.fldSpeedLimitsL3.setValue(++v);
            }
            if (v <= App.fldSpeedLimitsL1.getValue()) {
                App.fldSpeedLimitsL1.setValue(--v);
            }
        };
        SettingsManager.prototype.SpeedLimit3Changed = function () {
            this.CheckSaveSpeedLimitButtonState();
            var v = App.fldSpeedLimitsL3.getValue();
            if (v <= App.fldSpeedLimitsL2.getValue()) {
                App.fldSpeedLimitsL2.setValue(--v);
            }
        };
        SettingsManager.prototype.SaveSpeedLimites = function () {
            var sl = {
                l1: App.fldSpeedLimitsL1.getValue(),
                l2: App.fldSpeedLimitsL2.getValue(),
                l3: App.fldSpeedLimitsL3.getValue(),
                l1c: App.fldSpeedLimitL1c.getValue(),
                l2c: App.fldSpeedLimitL2c.getValue(),
                l3c: App.fldSpeedLimitL3c.getValue()
            };
            DR.SaveSpeedLimits(sl, {
                success: function (e) { },
                failure: function (e) { }
            });
            this._settings.SpeedLimits.Parse(sl);
            App.wndEditSpeedLimits.hide();
            if ($App.RepositoryManager.View.ShowSpeedInTracking && TM.currentTrack) {
                TM.currentTrack.display();
            }
        };
        SettingsManager.prototype.CheckSaveSpeedLimitButtonState = function () {
            if (!App.fldSpeedLimitsL1.getValue() || !App.fldSpeedLimitsL2.getValue() || !App.fldSpeedLimitsL3.getValue()) {
                App.btnSaveSpeedLimit.setDisabled(true);
            }
            else {
                App.btnSaveSpeedLimit.setDisabled(false);
            }
        };
        return SettingsManager;
    }());
    Core.SettingsManager = SettingsManager;
})(Core || (Core = {}));
