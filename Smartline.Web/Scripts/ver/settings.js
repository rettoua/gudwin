function check() {
    App.btnChange.setDisabled(!(App.txtOldSecret.getValue() && App.txtPassword.getValue() && App.txtRepPassword.getValue()));
    return true;
}

function validate_new_login() {
    if (App.txtPassword.getValue() == App.txtRepPassword.getValue()) {
        return true;
    }
    Ext.Msg.alert('Новый пароль', 'Новый пароль введен повторно неверно');
    return false;
}

function changeIntegarationButtonState() {
    var chk = App.chkOnIntegration,
        sec = App.cntSecurity;
    sec.setDisabled(chk.checked === false);
}

function saveEvosIntegration() {
    var chk = App.chkOnIntegration,
        lgn = App.txtIntegrationLogin,
        sec = App.txtIntegrationSecret,
        btn = App.btnSaveEvosIntegration;
    if (!App.pnlEvosIntegration.getForm().isValid()) {
        btn.setDisabled(true);
        return;
    }
    var login = lgn.getValue(),
        secret = sec.getValue(),
        available = chk.getValue(),
        viewPort = App.Viewport1;
    viewPort.mask('Сохранение настроек интеграции...');
    DR.SaveEvosIntegration(available, login, secret, {
        success: function () {
            viewPort.unmask();
            Ext.Msg.notify('Сохранение', 'Настройки интеграции успешно сохранены');
        },
        failure: function (e) {
            Ext.Msg.alert('Ошибка сохранения', 'Ошибка при сохранении настроек интеграции. Повторите попытку');
            viewPort.unmask();
        }
    });
}

function savePoinsSettings(points) {
    DR.UpdateSettingsPoints(points,
        {
            success: function () {
                Ext.Msg.alert('Сохранение', 'Данные успешно сохранены');
            },
            failure: function (e) {
                Ext.Msg.alert('Сохранение', 'Ошибка сохранения. Повторите попытку');
            }
        });
    try {
        parent.frames[0].globalUserSettings.p = points;
    } catch (e) { }
}


function saveWeightSettings(weight) {
    DR.UpdateSettingsStroke(weight,
        {
            success: function () {
                Ext.Msg.alert('Сохранение', 'Данные успешно сохранены');
            },
            failure: function (e) {
                Ext.Msg.alert('Сохранение', 'Ошибка сохранения. Повторите попытку');
            }
        });
    try {
        parent.frames[0].globalUserSettings.weight = weight;
    } catch (e) { }
}

var usersettingsClass = function () {
    this.operators = new operatorsClass();
};
var operatorsClass = function () {
    this.add_tracker_n = 'bntAddTracker';
    this.remove_tracker_n = 'bntRemoveTracker';
    this.add_all_tracker_n = 'bntAddAllTracker';
    this.remove_all_tracker_n = 'bntRemoveAllTracker';
    this.getGrid = function () {
        return App.gridPanelOperators;
    };
    this.getRsm = function () {
        return App.rsmOp;
    };
    this.getNewButton = function () {
        return App.btnOpAdd;
    };
    this.getEditButton = function () {
        return App.btnOpEdit;
    };
    this.getRemoveButton = function () {
        return App.btnOpRemove;
    };
    this.getUserNameTxt = function () {
        return App.txtOpUserName;
    };
    this.getSecretTxt = function () {
        return App.txtOpSecret;
    };
    this.getNameTxt = function () {
        return App.txtOpName;
    };
    this.getBlockedChk = function () {
        return App.chkOpBlocked;
    };
    this.getSaveButton = function () {
        return App.btnOpSave;
    };
    this.loadOperators = function () {
        this.getGrid().store.reload();
    };
    this.getWnd = function () {
        return App.wndOperator;
    };
    this.getOpTrackersGrid = function () {
        return App.gridOpTrackers;
    };
    this.getAllTrackersGrid = function () {
        return App.gridAllTrackers;
    };
    this.addOperator = function () {
        this.new = true;
        this.record = null;
        this.clearControls();
        this.showWnd();
    };
    this.clearControls = function () {
        this.getUserNameTxt().setValue(null);
        this.getSecretTxt().setValue(null);
        this.getNameTxt().setValue(null);
        this.getBlockedChk().setValue(false);
    };
    this.removeOperator = function () {
        this.getGrid().deleteSelected();
    };
    this.editOperator = function () {
        this.new = false;
        this.record = this.getRsm().getSelection()[0];
        this.displayData(this.record.data);
        this.showWnd();
    };
    this.save = function () {
        this.checkUserName();
    };
    this.checkUserName = function () {
        if (!this.checkNameInGrid()) { return; }
        var userName = this.getUserNameTxt().getValue(),
            me = this,
            id = (this.record && this.record.data && this.record.data.Id) || -1;
        this.showCheckUserNameMask();
        DR.UserNameExist(userName, id, {
            success: function (r) {
                if (r === true) {
                    me.showLoginExistAlert(userName);
                } else {
                    me.applyData();
                }
                me.hideCheckUserNameMask();
            },
            failure: function (e) {
                Ext.Msg.alert('Ошибка проверки', 'Ошибка проверки логина. Повторите попытку.');
                me.hideCheckUserNameMask();
            }
        });
    };
    this.checkNameInGrid = function () {
        var id = this.record && this.record.internalId,
            userName = this.getUserNameTxt().getValue();
        var allRec = this.getGrid().store.getAllRange();
        for (var i = 0; i < allRec.length; i++) {
            if (id == allRec[i].internalId) { continue; }
            if (userName == allRec[i].data.UserName) {
                this.showLoginExistAlert(userName);
                return false;
            }
        }
        return true;
    };
    this.showLoginExistAlert = function (login) {
        Ext.Msg.alert('Недопустимый логин', 'Логин <b>' + login + '</b> уже существует в системе. Введите другой логин и повторите попытку');
    };
    this.applyData = function () {
        if (this.new) {
            var obj = {},
                grid = this.getGrid();
            this.updateDataObejct(obj);
            grid.store.add(obj);
        } else {
            this.updateDataObejct(this.getGrid().selModel.getSelection()[0].data);
            this.getGrid().selModel.getSelection()[0].setDirty();
            this.getGrid().view.refresh();
        }
        this.hideWnd();
    };
    this.updateDataObejct = function (obj) {
        obj.UserName = this.getUserNameTxt().getValue();
        obj.NormalSecret = this.getSecretTxt().getValue();
        obj.Name = this.getNameTxt().getValue();
        obj.IsBlocked = this.getBlockedChk().getValue();
    };
    this.displayData = function (obj) {
        this.obj = obj;
        this.getUserNameTxt().setValue(obj.UserName);
        this.getSecretTxt().setValue(obj.NormalSecret);
        this.getNameTxt().setValue(obj.Name);
        this.getBlockedChk().setValue(obj.IsBlocked);
    };
    this.showWnd = function () {
        this.getWnd().show();
    };
    this.hideWnd = function () {
        this.getWnd().hide();
    };
    this.updateButtonStates = function () {
        var s = this.getRsm().hasSelection();
        this.getEditButton().setDisabled(!s);
        this.getRemoveButton().setDisabled(!s);
        this.reloadOperatorTrackers();
        this.updateTrackersButtons();
    };
    this.validateWindow = function () {
        var v = !this.getUserNameTxt().isValid() | !this.getSecretTxt().isValid();
        this.getSaveButton().setDisabled(v);
    };
    this.showCheckUserNameMask = function () {
        var wnd = this.getWnd();
        wnd.body.mask('Проверка логина...');
    };
    this.hideCheckUserNameMask = function () {
        var wnd = this.getWnd();
        wnd.body.unmask();
    };
    this.updateTrackersButtons = function () {
        var s = this.getRsm().hasSelection();
        if (!s) {
            App[this.add_tracker_n].setDisabled(true);
            App[this.remove_tracker_n].setDisabled(true);
            App[this.add_all_tracker_n].setDisabled(true);
            App[this.remove_all_tracker_n].setDisabled(true);
        } else {
            var opGrid = this.getOpTrackersGrid(),
             allGrid = this.getAllTrackersGrid();
            App[this.remove_tracker_n].setDisabled(!opGrid.selModel.hasSelection());
            App[this.add_tracker_n].setDisabled(!allGrid.selModel.hasSelection());
            App[this.add_all_tracker_n].setDisabled(allGrid.store.count() == 0);
            App[this.remove_all_tracker_n].setDisabled(opGrid.store.count() == 0);
        }
    };
    this.reloadOperatorTrackers = function () {
        var s = this.getRsm().hasSelection(),
            opGrid = this.getOpTrackersGrid(),
            allGrid = this.getAllTrackersGrid();
        opGrid.store.suspendEvents();
        allGrid.store.suspendEvents();
        opGrid.store.removeAll();
        allGrid.store.removeAll();
        if (s) {
            var oper = this.getRsm().getSelection()[0];
            this.allTracks = parent.frames[0].$App.RepositoryManager.Repositories;
            for (var i in this.allTracks) {
                if (oper.data.Trackers && oper.data.Trackers.length > 0) {
                    var exist = false;
                    for (var j = 0; j < oper.data.Trackers.length; j++) {
                        if (this.allTracks[i].Record.data.TrackerId == oper.data.Trackers[j]) {
                            exist = true;
                            break;
                        }
                    }
                    if (!exist) {
                        allGrid.store.add(this.allTracks[i].Record.data);
                    } else {
                        opGrid.store.add(this.allTracks[i].Record.data);
                    }
                } else {
                    allGrid.store.add(this.allTracks[i].Record.data);
                }
            }
        }        
        opGrid.store.resumeEvents() & opGrid.view.refresh();
        allGrid.store.resumeEvents() & allGrid.view.refresh();
    };
    this.addTracker = function () {
        var allGrid = this.getAllTrackersGrid(),
            opGrid = this.getOpTrackersGrid();
        if (!allGrid.selModel.hasSelection()) { return; }
        var selected = allGrid.selModel.getSelection();
        for (var i = 0; i < selected.length; i++) {
            opGrid.store.add(selected[i].data);
            allGrid.deleteSelected();
        }
        this.updateOperatorTrackers();
    };
    this.removeTracker = function () {
        var allGrid = this.getAllTrackersGrid(),
            opGrid = this.getOpTrackersGrid();
        if (!opGrid.selModel.hasSelection()) { return; }
        var selected = opGrid.selModel.getSelection();
        for (var i = 0; i < selected.length; i++) {
            allGrid.store.add(selected[i].data);
            opGrid.deleteSelected();
        }
        this.updateOperatorTrackers();
    };
    this.addAllTrackers = function () {
        var allGrid = this.getAllTrackersGrid(),
            opGrid = this.getOpTrackersGrid();
        opGrid.store.add(allGrid.store.getAllRange());
        allGrid.store.removeAll();
        this.updateOperatorTrackers();
    };
    this.removeAllTrackers = function () {
        var allGrid = this.getAllTrackersGrid(),
            opGrid = this.getOpTrackersGrid();
        allGrid.store.add(opGrid.store.getAllRange());
        opGrid.store.removeAll();
        this.updateOperatorTrackers();
    };
    this.updateOperatorTrackers = function () {
        var selection = this.getRsm().getSelection()[0],
            opGrid = this.getOpTrackersGrid(),
            r = opGrid.store.getAllRange();
        selection.data.Trackers = [];
        for (var i = 0; i < r.length; i++) {
            selection.data.Trackers.push(r[i].data.TrackerId);
        }
        this.getGrid().selModel.getSelection()[0].setDirty();
    };
};
settings = new usersettingsClass();