var template = '<span style="color:{0};">{1}</span>';

var change = function (value) {
    return Ext.String.format(template, (value > 0) ? "green" : "red", value);
};
function columnRenderR(p1, p2, p3, p4, p5) {
    return columnRelayRender(p3, 'r');
}
function columnRenderR1(p1, p2, p3, p4, p5) {
    return columnRelayRender(p3, 'r1');
}
function columnRenderR2(p1, p2, p3, p4, p5) {
    return columnRelayRender(p3, 'r2');
}
function columnRenderS1(p1, p2, p3, p4, p5) {
    return columnRelayRender(p3, 's1');
}
function columnRenderS2(p1, p2, p3, p4, p5) {
    return columnRelayRender(p3, 's2');
}
function columnRelayRender(p, p1) {
    if (p.data[p1] && p.data[p1].a === true) {
        return Ext.String.format(template, "green", p.data[p1].n);
    } else {
        return Ext.String.format(template, "red", 'выкл');
    }
}

var colorRenderer = function (value, metadata) {
    var bgColor = value.indexOf("#") == -1 ? '#' + value : value;
    metadata.style = 'background-color:' + bgColor + ';';
    return '';
};

function relayStateChanged(n, v) {
    if (App['cnt' + n]) {
        App['cnt' + n].setDisabled(!v);
        App['chkOn' + n].bodyEl.dom.style.color = v ? 'green' : 'red';
        App['chkOn' + n].setBoxLabel(v ? 'Включен' : 'Выключен');
    }
}
function selectTracker() {
    if (!App.gridPanelCars.selModel.hasSelection()) {
        return;
    }
    var record = App.gridPanelCars.selModel.getSelection()[0];
    setSensor('Relay1', record.get('r1'));
    setSensor('Relay2', record.get('r2'));
    setSensor('Sensor1', record.get('s1'));
    setSensor('Sensor2', record.get('s2'));
    setSensor('Relay', record.get('r'));
}

function setSensor(n, obj) {
    App['chkOn' + n].setValue(obj.a === true);
    App['txtName' + n].setValue(obj.n);
}
function saveSensors() {
    if (!App.gridPanelCars.selModel.hasSelection()) {
        return;
    }
    var record = App.gridPanelCars.selModel.getSelection()[0];
    var r1 = getSensorObj('Relay1');
    var r2 = getSensorObj('Relay2');
    var s1 = getSensorObj('Sensor1');
    var s2 = getSensorObj('Sensor2');
    var r = getSensorObj('Relay');
    record.set('r1', r1);
    record.set('r2', r2);
    record.set('s1', s1);
    record.set('s2', s2);
    record.set('r', r);
    App.btnSaveTrackers.fireEvent('click');
}

function getSensorObj(n) {
    var obj = {};
    obj.a = App['chkOn' + n].getValue() === true;
    obj.n = App['txtName' + n].getValue();
    return obj;
}

function renderVirtualTracker(value, meta, record, index) {
    if (record.data.v_trackerid) {
        return Ext.String.format('[{0}] {1}', record.data.v_trackerid, record.data.v_name);
    }
    return '<span style="color: grey;">&#60;нет&#62;</span>';
}

function editVirtualTracker(_this, command, record, recordIndex, cellIndex) {
    var wnd = getWndVirtualTracker();
    wnd.record = record;
    var isNew = record.data.v_trackerid == 0;
    gettxtVirtualName().setValue(record.data.v_name);
    getlblVirtualId().setText('ID трекера: ' + record.data.v_trackerid);
    if (isNew) {
        getbtnDeleteVirtual().hide();
    } else {
        getbtnDeleteVirtual().show();
    }
    wnd.show();
}

function saveVirtual() {
    var wnd = getWndVirtualTracker();
    if (wnd.record.data.v_trackerid != 0) {
        wnd.record.set('v_name', gettxtVirtualName().getValue());        
        wnd.hide();
        App.gridPanelCars.store.sync();
        return;
    }
    wnd.body.mask('Сохранение...');
    DR.GetNextTrackerId({
        success: function (r) {
            wnd.record.set('v_name', gettxtVirtualName().getValue());
            wnd.record.set('v_trackerid', r);
            wnd.body.unmask();
            App.gridPanelCars.store.sync();
            wnd.hide();
        }
    });
}

function clearVirtual() {
    var wnd = getWndVirtualTracker();
    wnd.record.set('v_name', '');
    wnd.record.set('v_trackerid', 0);
    wnd.hide();
    App.gridPanelCars.store.sync();
}

function getWndVirtualTracker() {
    return App.windowVirtualTracker;
}
function getbtnDeleteVirtual() {
    return App.btnDeleteVirtual;
}
function getbtnSaveVirtual() {
    return App.btnSaveVirtual;
}
function gettxtVirtualName() {
    return App.txtVirtualName;
}
function getlblVirtualId() {
    return App.lblVirtualId;
}
