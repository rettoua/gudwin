var setUser = function (selection) {
    App.pnlEditUser.setDisabled(!selection);
    if (!selection || selection.length == 0) {
        clearUserControlData();
    } else {
        var user = selection[0];
        setUserControlValue(user);
        if (App.txtUserName) {
            App.txtUserName.setValue(user.get('UserName'));
        }
    }
};

function clearUserControlData() {
    App.txtLogin.setValue('');
    App.txtSecret.setValue('');
    App.txtName.setValue('');
    App.txtIsBlocked.setValue(false);
    App.txtReason.setValue('');
    App.hdnId.setValue(0);
}
function setUserControlValue(user) {
    App.txtLogin.setValue(user.get('UserName'));
    App.txtName.setValue(user.get('Name'));
    App.txtIsBlocked.setValue(user.get('IsBlocked'));
    App.txtReason.setValue(user.get('Reason'));
    App.hdnId.setValue(user.get('Id'));
}

function onUserDetailClear() {
    clearUserControlData();
    var selection = App.gridUsers.selModel.getSelection();
    if (selection && selection.length > 0) {
        setUserControlValue(selection[0]);
    } else {
        App.pnlEditUser.setDisabled(true);
    }
}

function addUser() {
    clearUserControlData();
    App.pnlEditUser.setDisabled(false);
    App.gridUsers.selModel.clearSelections();
    clearTrackersByUser();
}

function userValidation() {
    var newUser = isNewUser();
    if (newUser) {
        var valid = App.txtLogin.getValue() &&
                    App.txtSecret.getValue() &&
                    App.txtName.getValue();
        App.btnSaveUser.setDisabled(!valid);
    } else {
        App.btnSaveUser.setDisabled(!App.txtName.getValue());
    }
}

function isNewUser() {
    var selection = App.gridUsers.selModel.getSelection();
    var newUser = !selection || selection.length == 0;
    return newUser;
}

function compileUser() {
    return {
        id: App.hdnId.getValue(),
        username: App.txtLogin.getValue(),
        secret: App.txtSecret.getValue(),
        name: App.txtName.getValue(),
        isadmin: App.txtIsAdmin.getValue(),
        isblocked: App.txtIsBlocked.getValue(),
        reason: App.txtReason.getValue()
    };
}

function validateTrackerInfo() {
    var valid = App.cmbUser.getValue() && App.txtTrackerId.getValue();
    App.btnValidate.setDisabled(!valid);
}

function checkTrackerId(f, p) {
    DR.ExistTrackerId(f.getValue(), {
        success: function (r) {
            p.show();
            if (r === false) {
                window[p.contentEl].innerHTML = "<span style='color: green;'> Трекер ID <b>" + f.getValue() + "</b> свободен</span>";
            } else {
                window[p.contentEl].innerHTML = "<span style='color: red;'> Трекер ID <b>" + f.getValue() + "</b> занят</span>";
            }
        },
        failure: function (e) {
            var z = 0;
        }
    });
}

function checkLogin(f, p) {
    DR.ExistLogin(f.getValue(), {
        success: function (r) {
            p.show();
            if (r === false) {
                window[p.contentEl].innerHTML = "<span style='color: green;'> Логин <b>" + f.getValue() + "</b> свободен</span>";
            } else {
                window[p.contentEl].innerHTML = "<span style='color: red;'> Логин <b>" + f.getValue() + "</b> занят</span>";
            }
        },
        failure: function (e) {
            var z = 0;
        }
    });
}

function deleteTrackerInfo(grid) {
    var id = grid.selModel.getSelection()[0].data;
    DR.RemoveTrackerInfo(Ext.encode(id));
}

function deleteTrackerInfoConfirmation(grid) {
    Ext.Msg.confirm('Удаление', 'Удалить трекер?', function (btn) {
        if (btn == 'yes') {
            deleteTrackerInfo(grid);
            return true;
        } else {
            return false;
        }
    });
}

function clearTrackeInfoData() {
    App.txtTrackerId.setValue('');
    App.cmbUser.setValue(null);
    App.pnlCheckTrackerResult.hide();
}

//----users' trackers

function loadTrackerByUser(grid, gridPanel) {
    if (gridPanel) {
        if (grid.selModel.hasSelection()) {
            var userName = grid.selModel.getSelection()[0].get('Id');
            DR.LoadTrackers(userName, {
                success: function (r) {
                    var obj = eval(r);
                    gridPanel.store.loadData(obj);
                    App.gridUserTrackersToolbar.setDisabled(false);
                },
                failure: function (error) {

                }
            });
            //gridUserTrackers
        }
    }
}

function clearTrackersByUser() {
    if (App.gridUserTrackers) {
        App.gridUserTrackers.store.loadData([]);
        App.gridUserTrackersToolbar.setDisabled(true);
    }
}

function validateNewTracker() {
    var valid = App.cmbInfoTrackers.getValue();
    App.btnAddTrackerToUser.setDisabled(!valid);
}

function clearNewTrackerData() {
    App.cmbInfoTrackers.reset();
}

var showResult = function (btn) {
    if (btn == 'yes') {
        App.btnSaveNewTrackerInfo.fireEvent('click', App.btnSaveNewTrackerInfo);
    }
};

var showResetTracker = function (gridT, gridU, wnd, cmb) {
    wnd.show();
    wnd.body.mask('Загрузка данных...');
    var us = gridU.selModel.getSelection()[0];

    DR.GetUsersByCurrentUser(us.data.Id, {
        success: function (r) {
            cmb.store.loadData(eval(r));
            cmb.setValue(null);
            wnd.body.unmask();
        },
        failure: function (e) {
            wnd.body.unmask();
        }
    });
};

var resetTrackers = function (gridT, gridU, wnd, cmb) {
    wnd.show();
    wnd.body.mask('Обновление данных...');
    var us = gridU.selModel.getSelection()[0],
        tr = gridT.selModel.getSelection(),
        trIds = [];
    for (var i = 0; i < tr.length; i++) {
        trIds.push(tr[i].data.id);
    }
    DR.ResetTrackers(us.data.Id, cmb.getValue(), trIds, {
        success: function (r) {
            loadTrackerByUser(App.gridUsers, App.gridUserTrackers);
            wnd.body.unmask();
            wnd.hide();
            Ext.Msg.notify('Обновление данных', 'Трекеры добавлены другому пользователю!');
        },
        failure: function (e) {
            wnd.body.unmask();
        }
    });
};
