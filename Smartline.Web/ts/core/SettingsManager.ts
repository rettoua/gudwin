module Core {
    declare var App;
    declare var DR;
    declare var $App: Application;
    declare var window;
    declare var TM;

    export class SettingsManager {
        private _settings: Models.Settings;
        private _carSettings: CarSettings;

        public get Settings(): Models.Settings {
            return this._settings;
        }

        public get CarSettings(): CarSettings {            
            return this._carSettings;
        }

        public ApplySettings(source: any): void {
            this._settings = new Models.Settings(source);
            this._carSettings = new CarSettings();
            this.Apply();
        }

        private Apply(): void {
            var wnd = App.pnlCars,
                wndSettings = $App.SettingsManager.Settings.WindowSettings;

            wnd.suspendEvents();
            wnd.setWidth(wndSettings.Width);
            wnd.setHeight(wndSettings.Height);
            wnd[wndSettings.IsCollapsed === true ? 'collapse' : 'expand']();
            var h = window.innerHeight || window.clientHeight,
                w = window.innerWidth || window.clientWidth,
                newX,
                newY;
            newX = wndSettings.X * w / 100;
            newY = wndSettings.Y * h / 100;
            wnd.setX(newX);
            wnd.setY(newY);
            wnd.resumeEvents();
            wnd.addListener('resize', function () { Core.SettingsManager.WndResized(this); });
            wnd.addListener('expand', () => { Core.SettingsManager.WndExpandedState(false); });
            wnd.addListener('collapse', () => { Core.SettingsManager.WndExpandedState(true); });
            wnd.addListener('move', function () { Core.SettingsManager.WndLocationChanged(this); });
        }

        private static WndResized(wnd: any): void {
            DR.UpdateWindowSize(wnd.width, wnd.height,
                { failure: e=> { } });
        }

        private static WndExpandedState(state: boolean): void {
            DR.UpdateWindowExpandedState(state,
                { failure: e=> { } });
        }

        private static WndLocationChanged(wnd: any): void {
            var h = window.innerHeight || window.clientHeight,
                w = window.innerWidth || window.clientWidth,
                newX,
                newY;
            newX = wnd.x * 100 / w;
            newY = wnd.y * 100 / h;
            DR.UpdateWindowLocation(newX, newY,
                { failure: e=> { } });
        }

        public ChangePan(button: any): void {
            this._settings.IsPan = button.pressed;
            DR.UpdateSettingsPan(this._settings.IsPan, {
                failure: e=> { }
            });
        }

        public ChangeTrackingVisibility(button: any): void {
            this._settings.ShowTracking = button.pressed;
            DR.UpdateSettingsShowHideTracking(button.pressed, {
                failure: e=> { }
            });
            $App.RepositoryManager.UpdateShowTracking();
        }

        public EditSpeedLiminits(): void {
            App.fldSpeedLimitsL1.setValue(this._settings.SpeedLimits.Limit1);
            App.fldSpeedLimitsL2.setValue(this._settings.SpeedLimits.Limit2);
            App.fldSpeedLimitsL3.setValue(this._settings.SpeedLimits.Limit3);

            App.fldSpeedLimitL1c.setValue(this._settings.SpeedLimits.Limit1Color);;
            App.fldSpeedLimitL2c.setValue(this._settings.SpeedLimits.Limit2Color);;
            App.fldSpeedLimitL3c.setValue(this._settings.SpeedLimits.Limit3Color);;

            App.wndEditSpeedLimits.show();
        }

        public SpeedLimit1Changed(): void {
            this.CheckSaveSpeedLimitButtonState();
            var v = App.fldSpeedLimitsL1.getValue();
            if (v >= App.fldSpeedLimitsL2.getValue()) {
                App.fldSpeedLimitsL2.setValue(++v);
            }
        }

        public SpeedLimit2Changed(): void {
            this.CheckSaveSpeedLimitButtonState();
            var v = App.fldSpeedLimitsL2.getValue();
            if (v >= App.fldSpeedLimitsL3.getValue()) {
                App.fldSpeedLimitsL3.setValue(++v);
            }
            if (v <= App.fldSpeedLimitsL1.getValue()) {
                App.fldSpeedLimitsL1.setValue(--v);
            }
        }

        public SpeedLimit3Changed(): void {
            this.CheckSaveSpeedLimitButtonState();
            var v = App.fldSpeedLimitsL3.getValue();
            if (v <= App.fldSpeedLimitsL2.getValue()) {
                App.fldSpeedLimitsL2.setValue(--v);
            }
        }

        public SaveSpeedLimites(): void {
            var sl = {
                l1: App.fldSpeedLimitsL1.getValue(),
                l2: App.fldSpeedLimitsL2.getValue(),
                l3: App.fldSpeedLimitsL3.getValue(),

                l1c: App.fldSpeedLimitL1c.getValue(),
                l2c: App.fldSpeedLimitL2c.getValue(),
                l3c: App.fldSpeedLimitL3c.getValue()
            };
            DR.SaveSpeedLimits(sl, {
                success: e=> { },
                failure: e=> { }
            });
            this._settings.SpeedLimits.Parse(sl);
            App.wndEditSpeedLimits.hide();
            if ($App.RepositoryManager.View.ShowSpeedInTracking && TM.currentTrack) {
                TM.currentTrack.display();
            }
        }

        private CheckSaveSpeedLimitButtonState(): void {
            if (!App.fldSpeedLimitsL1.getValue() || !App.fldSpeedLimitsL2.getValue() || !App.fldSpeedLimitsL3.getValue()) {
                App.btnSaveSpeedLimit.setDisabled(true);
            } else {
                App.btnSaveSpeedLimit.setDisabled(false);
            }
        }
    }
}