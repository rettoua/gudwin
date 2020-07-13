var Core;
(function (Core) {
    var RepositoryManager = /** @class */ (function () {
        function RepositoryManager() {
            var _this = this;
            this._repositories = [];
            this._gpsProvider = new Controllers.GpsProvider();
            this._view = new Views.ViewRepositoryManager();
            this._sosHelper = new SosHelper(this);
            this._gpsReceivedEventHandler = new Core.EventHandler(function (sender, e) { return _this.OnGpsReceived(sender, e); });
            this._gpsProvider.GpsReceived.subscribe(this._gpsReceivedEventHandler);
        }
        Object.defineProperty(RepositoryManager.prototype, "Repositories", {
            get: function () {
                return this._repositories;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(RepositoryManager.prototype, "IsShowHotTracking", {
            get: function () {
                return App.chkShowTrack.pressed === true;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(RepositoryManager.prototype, "View", {
            get: function () {
                return this._view;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(RepositoryManager.prototype, "GpsProvider", {
            get: function () {
                return this._gpsProvider;
            },
            enumerable: true,
            configurable: true
        });
        RepositoryManager.prototype.LoadRecords = function (grid) {
            this._grid = grid;
            this.InitializeRepositories(this._grid.store.getAllRange());
            this._gpsProvider.Start();
            this._gpsProvider.LoadUnreachableData();
            this.UpdatePrependText();
        };
        RepositoryManager.prototype.GetRepositoriesForRefresh = function () {
            var values = [];
            for (var k in this._repositories) {
                var r = this._repositories[k];
                var obj = {};
                obj.id = k;
                obj.permit = r.CanRefresh !== true;
                obj.date = r.LastTime;
                obj.hottrack = this.IsShowHotTracking;
                values.push(obj);
            }
            return values;
        };
        RepositoryManager.prototype.GetUnreachableRepositories = function () {
            var values = [];
            for (var k in this._repositories) {
                var r = this._repositories[k];
                if (r.Record.get('Speed') != -1) {
                    continue;
                }
                values.push(k);
            }
            return values;
        };
        RepositoryManager.prototype.InitializeRepositories = function (records) {
            var _this = this;
            records.forEach(function (iRecord) {
                _this._repositories[iRecord.internalId] = new Repository(iRecord);
            });
        };
        RepositoryManager.prototype.OnGpsReceived = function (sender, e) {
            this.UpdateRepositories(e.Items);
        };
        RepositoryManager.prototype.BeginHibernateGrid = function () {
            this._grid.store.suspendEvents();
        };
        RepositoryManager.prototype.EndHibernateGrid = function () {
            this._grid.store.resumeEvents();
            this.RefreshGrid();
            this._view.FitToBounds();
        };
        RepositoryManager.prototype.RefreshGrid = function () {
            this._grid.view.refresh(false);
        };
        RepositoryManager.prototype.UpdateRepositories = function (items) {
            var _this = this;
            this.BeginHibernateGrid();
            items.forEach(function (gps) {
                var repo = _this._repositories[gps.TrackerId];
                repo.Update(gps);
            });
            this.EndHibernateGrid();
        };
        RepositoryManager.prototype.StartAllTracking = function () {
            for (var repo in this._repositories) {
                if (this._repositories[repo].IsTracked) {
                    continue;
                }
                if (!this._repositories[repo].IsActive) {
                    continue;
                }
                this._repositories[repo].StartTracking();
            }
        };
        RepositoryManager.prototype.StopAllTracking = function () {
            for (var repo in this._repositories) {
                if (!this._repositories[repo].IsTracked) {
                    continue;
                }
                this._repositories[repo].StopTracking();
            }
        };
        RepositoryManager.prototype.Centralize = function (item, record) {
            this._repositories[record.internalId].Centralize();
        };
        RepositoryManager.prototype.UpdateShowTracking = function () {
            for (var repo in this._repositories) {
                if (!this._repositories[repo].IsTracked) {
                    continue;
                }
                if ($App.SettingsManager.Settings.ShowTracking) {
                    this._repositories[repo].Tracking.Show();
                }
                else {
                    this._repositories[repo].Tracking.Hide();
                }
            }
        };
        RepositoryManager.prototype.ApplyCarsFilter = function () {
            this._grid.store.filterBy(this.FilterString);
            this.UpdatePrependText();
        };
        RepositoryManager.prototype.FilterString = function (record) {
            var value = record.get('Name') ? record.get('Name').toLowerCase() : '', textFilterValue = (App.txtFilterValue.getValue() + '').toLowerCase();
            return (value.indexOf(textFilterValue) > -1 || value == '') && $App.RepositoryManager.GetFilterFunctionByType()(record);
        };
        RepositoryManager.prototype.GetFilterFunctionByType = function () {
            var selectItem = App.btnFilterCarByState.activeItem.id;
            switch (selectItem) {
                case 'cmiAll':
                    return this.FilterCmiAll;
                case 'cmiActive':
                    return this.FilterCmiActive;
                case 'cmiInactive':
                    return this.FilterCmiInactive;
                case 'cmiRun':
                    return this.FilterCmiRun;
                case 'cmiStop':
                    return this.FilterCmiStop;
                default:
                    return this.FilterCmiAll;
            }
        };
        RepositoryManager.prototype.FilterCmiAll = function (record) {
            return true;
        };
        RepositoryManager.prototype.FilterCmiActive = function (record) {
            return $App.RepositoryManager.Repositories[record.internalId].IsActive === true;
        };
        RepositoryManager.prototype.FilterCmiInactive = function (record) {
            return !$App.RepositoryManager.Repositories[record.internalId].IsActive;
        };
        RepositoryManager.prototype.FilterCmiRun = function (record) {
            return $App.RepositoryManager.Repositories[record.internalId].IsActive === true && $App.RepositoryManager.Repositories[record.internalId].IsRun === true;
        };
        RepositoryManager.prototype.FilterCmiStop = function (record) {
            return $App.RepositoryManager.Repositories[record.internalId].IsActive === true && $App.RepositoryManager.Repositories[record.internalId].IsRun === false;
        };
        RepositoryManager.prototype.GetGridRowsCount = function () {
            return App.gridPanelCars.getRowsValues().length;
        };
        RepositoryManager.prototype.UpdatePrependText = function () {
            var cmb = App.btnFilterCarByState, text = cmb.activeItem.text, count = this.GetGridRowsCount();
            cmb.setText(Ext.String.format('Авто(<b>{0}</b>): {1}', count, text));
        };
        return RepositoryManager;
    }());
    Core.RepositoryManager = RepositoryManager;
    var Repository = /** @class */ (function () {
        function Repository(record) {
            this._isTracked = false;
            this._record = record;
            this.UpdateState();
        }
        Object.defineProperty(Repository.prototype, "Record", {
            get: function () {
                return this._record;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "Record2", {
            get: function () {
                return this._record;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "IsTracked", {
            get: function () {
                return this._isTracked;
            },
            set: function (value) {
                this._isTracked = value;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "CanRefresh", {
            get: function () {
                return this.IsTracked;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "LastTime", {
            get: function () {
                return this._record.get('EndSendTime') || this._record.get('LastSendTime');
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "IsActive", {
            get: function () {
                return this._isActive;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "HasValue", {
            get: function () {
                return this._hasValue;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "IsConnected", {
            get: function () {
                return this._connected;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "IsRun", {
            get: function () {
                return this._isRun;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "StopTimeMinutes", {
            get: function () {
                return this._stopTimeMinutes;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "Location", {
            get: function () {
                return new L.LatLng(this._record.get('Latitude'), this._record.get('Longitude'));
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "Id", {
            get: function () {
                return this._record.internalId;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "Tracking", {
            get: function () {
                return this._tracking;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "Image", {
            get: function () {
                return new CarImage(this._record);
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "isSosAlarming", {
            get: function () {
                var d = this._record.data;
                if (d.Sensor2 && d.Sensor2.sos === true && d.s && d.s.s2 === false) { //it's strange but if second sensor has 'false' as a value this means that sensor is in alarming state
                    return true;
                }
                return false;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Repository.prototype, "alarmingColor", {
            get: function () {
                if (!this.isSosAlarming) {
                    return '';
                }
                return this._record.data.lastSos ? '#f5ff00' : '#fb7070';
            },
            enumerable: true,
            configurable: true
        });
        Repository.prototype.updateSensors = function (newState) {
            this._record.set('Relay', newState.r);
            this._record.set('Relay1', newState.r1);
            this._record.set('Relay2', newState.r2);
            this._record.set('Sensor1', newState.s1);
            this._record.set('Sensor2', newState.s2);
        };
        Repository.prototype.updateImage = function (image) {
            this._record.set('Image', image);
        };
        Repository.prototype.Update = function (gps) {
            this._record.set('LastSendTime', gps.SendTime);
            //if (gps.EndTime && gps.EndTime != null) {
            this._record.set('EndSendTime', gps.EndTime);
            //}
            this._record.set('Speed', gps.Speed);
            this._record.set('i', gps.GpsSignal);
            if (gps.Latitude) {
                this._record.set('Latitude', gps.Latitude);
            }
            if (gps.Longitude) {
                this._record.set('Longitude', gps.Longitude);
            }
            this._record.set('Battery', gps.Battery);
            this._record.set('s', { r: gps.Sensors.Relay, r1: gps.Sensors.Relay1, r2: gps.Sensors.Relay2, s1: gps.Sensors.Sensor1, s2: gps.Sensors.Sensor2 });
            this.UpdateState();
            this.UpdateTracking(gps);
        };
        Repository.prototype.UpdateState = function () {
            this._isActive = false;
            this._connected = true;
            this._hasValue = true;
            this._isRun = false;
            var speed = this._record.get('Speed'), sendTime = this.LastTime;
            this._hasValue = !!(sendTime);
            if (speed == -1) {
                this._hasValue = false;
                this._connected = false;
                return;
            }
            else if (this._hasValue) {
                this._stopTimeMinutes = Math.round((Date.now() - +sendTime) / 60000);
                if (this._stopTimeMinutes > 10) {
                    this._connected = false;
                    return;
                }
            }
            if (speed > 0) {
                this._isActive = true;
                this._isRun = true;
            }
            else if (speed == 0) {
                this._isActive = true;
            }
        };
        Repository.prototype.UpdateTracking = function (gps) {
            if (this.IsTracked) {
                this._tracking.AddPoint(gps);
            }
        };
        Repository.prototype.StartTracking = function () {
            if (this.IsTracked === true) {
                return;
            }
            this.IsTracked = true;
            this._tracking = new Views.Tracking(this);
            this._tracking.Start();
            $App.RepositoryManager.RefreshGrid();
        };
        Repository.prototype.StopTracking = function () {
            if (this.IsTracked === false) {
                return;
            }
            this.IsTracked = false;
            this._tracking.Stop();
            delete this._tracking;
            $App.RepositoryManager.RefreshGrid();
        };
        Repository.prototype.GetInfoWindowContent = function () {
            return Ext.String.format('<table><tr><td class="info-window-label">Авто:</td><td class="info-window-value">{0}</td></tr><tr><td class="info-window-label">Скорость:</td><td class="info-window-value">{1} км/ч</td></tr><tr><td class="info-window-label">Отправка:</td><td class="info-window-value">{2}</td></tr><tr><td class="info-window-label">Координаты:</td><td class="info-window-value">{3} х {4}</td></tr></table>', this._record.get('Name'), this._record.get('Speed'), Ext.Date.format(this._record.get('LastSendTime'), 'd-m-Y H:i:s'), this._record.get('Latitude'), this._record.get('Longitude'));
        };
        Repository.prototype.Centralize = function () {
            if (!this.IsTracked) {
                return;
            }
            $App.Map.Map.panTo(this.Location, { duration: 1 });
        };
        return Repository;
    }());
    Core.Repository = Repository;
    var SosHelper = /** @class */ (function () {
        function SosHelper(_repositoryManager) {
            this._repositoryManager = _repositoryManager;
            this.initTimer();
        }
        SosHelper.prototype.initTimer = function () {
            var _this = this;
            this._intervalId = window.setInterval(function () { return _this.doUpdate(); }, 1000);
        };
        SosHelper.prototype.doUpdate = function () {
            if (this.isRefreshRequired() === true) {
                this._repositoryManager.RefreshGrid();
            }
        };
        SosHelper.prototype.isRefreshRequired = function () {
            var _this = this;
            var isRequired = false;
            this._repositoryManager.Repositories.forEach(function (repo) {
                if (repo.isSosAlarming) {
                    isRequired = true;
                    if (repo.IsTracked) {
                        repo.Tracking.pointer.recalc();
                    }
                    if (_this.isRepoEditing(repo)) {
                        $App.SettingsManager.CarSettings.displaySensorState(repo, 2);
                    }
                }
            });
            return isRequired;
        };
        SosHelper.prototype.isRepoEditing = function (repo) {
            var carSettings = $App.SettingsManager.CarSettings;
            if (!carSettings.Record) {
                return false;
            }
            return (carSettings.Record.id || carSettings.Record.Id) == repo.Record.get('Id');
        };
        return SosHelper;
    }());
    var CarImage = /** @class */ (function () {
        function CarImage(record) {
            this.record = record;
            this._carImage = this.record.get('Image');
        }
        Object.defineProperty(CarImage.prototype, "name", {
            get: function () {
                return this._carImage.name;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(CarImage.prototype, "isDirectionRequired", {
            get: function () {
                return this._carImage.isrecalc;
            },
            enumerable: true,
            configurable: true
        });
        return CarImage;
    }());
    Core.CarImage = CarImage;
})(Core || (Core = {}));
