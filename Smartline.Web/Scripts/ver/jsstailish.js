var NodeStl = function (curnode) {
    this.node = curnode;
};

NodeStl.select = function (node) {
    var divElem = node.getElementsByTagName('div');
    for (var i = 0; i < divElem.length; i++) {
        if (divElem[i].id == 'rdiv') {
            divElem[i].className = 'a-div-run';
        }
        else if (divElem[i].id == 'markerImg') {
            divElem[i].className = 'a-div-marker-select';
        }
    }
    var linkAdd = node.getElementsByTagName('a');
    for (var l = 0; l < linkAdd.length; l++) {
        if (linkAdd[l].id == 'lnkAdd')
            linkAdd[l].hidden = true;
    }
    node.selected = true;
};

NodeStl.deSelect = function (node) {
    var divElem = node.getElementsByTagName('div');
    for (var i = 0; i < divElem.length; i++) {
        if (divElem[i].id == 'rdiv') {
            NodeStl.deActivePath(node);
            divElem[i].className = '';
        }
        else if (divElem[i].id == 'markerImg') {
            divElem[i].className = 'a-div-marker';
        }
    }
    NodeStl.deActivePath(node);

    var linkAdd = node.getElementsByTagName('a');
    for (var l = 0; l < linkAdd.length; l++) {
        if (linkAdd[l].id == 'lnkAdd')
            linkAdd[l].hidden = false;
    }

    node.selected = false;
};

NodeStl.activePath = function (node) {
    if (!node.runActive) {
        var divElem = node.getElementsByTagName('div');
        for (var i = 0; i < divElem.length; i++) {
            if (divElem[i].id == 'rdiv') {
                divElem[i].className = 'a-div-stop';
                node.runActive = true;
            }
        }
    }
};

NodeStl.deActivePath = function (node) {
    if (node.runActive) {
        var divElem = node.getElementsByTagName('div');
        for (var i = 0; i < divElem.length; i++) {
            if (divElem[i].id == 'rdiv') {
                divElem[i].className = 'a-div-run';
                node.runActive = false;
            }
        }
    }
};

NodeStl.getByUidNew = function (_this, uid) {
    var grid = _this.gridPanel;
    var rec = grid.getStore().getById(uid);
    return { node: rec.template, record: rec };
};

NodeStl.setTime = function (record) {
    var node = record.template;
    var divElem = node.getElementsByTagName('td');
    for (var i = 0; i < divElem.length; i++) {
        if (divElem[i].id == 'tdTime' && record.data.SendTime != null) {
            divElem[i].innerText = record.data.SendTime.format("HH:mm");
        }
    }
};

NodeStl.getTimeEl = function (el) {
    var divElem = el.getElementsByTagName('td');
    for (var i = 0; i < divElem.length; i++) {
        if (divElem[i].id == 'tdTime') {
            return divElem[i];
        }
    }
};

NodeStl.getSpeedEl = function (el) {
    var divElem = el.getElementsByTagName('td');
    for (var i = 0; i < divElem.length; i++) {
        if (divElem[i].id == 'tdSpeed') {
            return divElem[i];
        }
    }
};

NodeStl.addExpand = function (r) {
    var el = r.__el;
    var pnl = document.getElementById('statisticPanel');
    var divElem = el.getElementsByTagName('div');
    for (var i = 0; i < divElem.length; i++) {
        if (divElem[i].id == 'divAdd') {
            divElem[i].appendChild(pnl);
            statisticPanel.setVisible(true);
            el.additional = true;
            Smartline.Ajax.ExistAnyGpsOfCar(r.__id, {
                success: function (result) {
                    if (result) {
                        pnlLoadingCar.setVisible(false);
                        pnlDataNotExist.setVisible(false);
                        pnlStatisticFields.setVisible(true);
                    }
                    else {
                        pnlDataNotExist.setVisible(true);
                        pnlLoadingCar.setVisible(false);
                    }
                }
            });
        } else if (divElem[i].id == 'divExt') {
            divElem[i].setAttribute('class', 'div-car-col');
        }
    }
};

NodeStl.addCollapse = function (r) {
    var el = r.__el;
    var divElem = el.getElementsByTagName('div');
    for (var i = 0; i < divElem.length; i++) {
        if (divElem[i].id == 'divAdd') {
            el.additional = false;
            pnlLoadingCar.setVisible(false);
            pnlStatisticFields.setVisible(false);
            pnlDataNotExist.setVisible(false);
            NodeStl.hideStatisticBoxes();
            r.clearPath();
        } else if (divElem[i].id == 'divExt') {
            divElem[i].setAttribute('class', 'div-car-ext');
        }
    }
};

NodeStl.showStatisticBoxes = function () {
    chkShowMarker.setDisabled(false);
    chkShowTime.setDisabled(false);
};

NodeStl.hideStatisticBoxes = function () {
    chkShowMarker.setDisabled(true);
    chkShowTime.setDisabled(true);
};

NodeStl.initCollapse = function () {
    var el = document.getElementById('fly_cars');
    el.innerHTML += '<div id="div-expcol" class="div-col" title="Скрыть автомобили" onclick="NodeStl.collapseCar(this);"></div>';
};

NodeStl.collapseCar = function (c) {
    var column = pnlCars.el.findParent('.x-panel .x-column');
    column.initWidth = column.clientWidth;
    column.style.width = '0px';
    c.setAttribute('class', 'div-exp');
    c.setAttribute('title', 'Отобразить автомобили');
    c.setAttribute('onclick', 'NodeStl.expandCar(this);');
    ViewPort1.doLayout();
    google.maps.event.trigger(road.getMap(), "resize");
};

NodeStl.expandCar = function (c) {
    var column = pnlCars.el.findParent('.x-panel .x-column');     
    column.style.width = column.initWidth+'px';
    c.setAttribute('class', 'div-col');
    c.setAttribute('title', 'Скрыть автомобили');
    c.setAttribute('onclick', 'NodeStl.collapseCar(this);');
    ViewPort1.doLayout();
    google.maps.event.trigger(road.getMap(), "resize");
};