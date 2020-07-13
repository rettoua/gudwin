var RM;

function intitialize() {
    RM = new ReportManager();
    App.TabPanelReports.addListener('tabchange', function () { RM.tabChange.apply(RM, arguments); });
    App.TabPanelReports.addListener('tabclose', function () { RM.tabClose.apply(RM, arguments); });
}

var ReportManager = function () {
    this.reports = [];
    this.isNewReport = false;
    this.addReport = function () {
        if (this.isNewReport) {
            this.generate();
        } else {
            this.generate(App.TabPanelReports.activeTab);
        }
    };
    this.validate = function () {
        var valid = App.cmbReportTypes.getValue() && App.dfFrom.getValue() && App.dfTo.getValue() && App.tfFrom.getValue() && App.tfTo.getValue() && this.carsSelected();
        App.btnGenerateReport.setDisabled(!valid);
    };
    this.carsSelected = function () {
        for (var i = 0; i < App.gridPanelCars.store.data.items.length; i++) {
            if (App.gridPanelCars.store.data.items[i].get('Selected'))
                return true;
        }
        return false;
    };
    this.tabChange = function (panel, tab) {
        if (!this.isNewReport) {
            this.setStateByReport(tab.reportState);
        }
    };
    this.tabClose = function (panel, tab) {
        for (var i = 0; i < this.reports.length; i++) {
            if (this.reports[i].reportId && tab.reportId && this.reports[i].reportId == tab.reportId) {
                this.reports.splice(i, 1);
                break;
            }
        }
        this.hideCreateReport();
    };
    this.setStateByReport = function (state) {
        App.pnlCreateReport.show();
        App.cmbReportTypes.setDisabled(true);
        App.btnCancelReport.hide();
        App.btnGenerateReport.setText('Обновить отчет');
        App.dfFrom.setValue(state.df);
        App.dfTo.setValue(state.dt);
        App.tfFrom.setValue(state.tf);
        App.tfTo.setValue(state.tt);
        App.cmbReportTypes.setValue(state.rt);
        for (var i = 0; i < App.gridPanelCars.store.data.items.length; i++) {
            App.gridPanelCars.store.data.items[i].set('Selected', false);
        }
        for (var j = 0; j < state.trackers.length; j++) {
            for (var i = 0; i < App.gridPanelCars.store.data.items.length; i++) {
                if (App.gridPanelCars.store.data.items[i].internalId == state.trackers[j]) {
                    App.gridPanelCars.store.data.items[i].set('Selected', true);
                }
            }
        }
        App.gridPanelCars.store.commitChanges();
    };
    this.createNewReport = function () {
        App.pnlCreateReport.show();
        App.btnCancelReport.show();
        App.cmbReportTypes.setDisabled(false);
        App.btnGenerateReport.setText('Сгенерировать');
        this.clearCreateData();
        this.validate();
        this.isNewReport = true;
    };
    this.hideCreateReport = function () {
        App.pnlCreateReport.hide();
        this.isNewReport = false;
    };
    this.clearCreateData = function () {
        App.cmbReportTypes.select(-1);
        App.dfFrom.setValue(new Date());
        App.dfTo.setValue(new Date());
        App.tfFrom.setValue('0:00');
        App.tfTo.setValue('23:59');
        for (var i = 0; i < App.gridPanelCars.store.data.items.length; i++) {
            App.gridPanelCars.store.data.items[i].set('Selected', false);
        }
        App.gridPanelCars.store.commitChanges();
    };
    this.export = function () {
        var df = App.dfFrom.getValue(),
            dt = App.dfTo.getValue(),
            tf = App.tfFrom.getValue(),
            tt = App.tfTo.getValue(),
            rt = App.cmbReportTypes.getValue(),
            trackers = [],
            names = [];

        for (var i = 0; i < App.gridPanelCars.store.data.items.length; i++) {
            if (App.gridPanelCars.store.data.items[i].get('Selected')) {
                trackers.push(App.gridPanelCars.store.data.items[i].internalId);
                names.push(this.getCarNameById(App.gridPanelCars.store.data.items[i].internalId));
            }
        }
        return { df: df, dt: dt, tf: tf, tt: tt, rt: rt, trackers: trackers, names: names };
    };
    this.generate = function (tab) {
        this.showMask();
        var df = App.dfFrom.getValue(),
            dt = App.dfTo.getValue(),
            tf = App.tfFrom.getValue(),
            tt = App.tfTo.getValue(),
            rt = App.cmbReportTypes.getValue(),
            trackers = [],
            t = this;
        for (var i = 0; i < App.gridPanelCars.store.data.items.length; i++) {
            if (App.gridPanelCars.store.data.items[i].get('Selected')) {
                trackers.push(App.gridPanelCars.store.data.items[i].internalId);
            }
        }
        var reportState = { df: df, dt: dt, tf: tf, tt: tt, rt: rt, trackers: trackers };
        DR.GenerateReport(rt, df, dt, tf, tt, trackers, {
            success: function (r) {
                t.generateDataView(r, rt, reportState, tab);
                t.hideMask();
            },
            failure: function (e) {
                t.hideMask();
            }
        });
    };
    this.generateDataView = function (data, rep, reportState, tab) {
        this.optimizeData(data.Data, rep);
        if (!tab) {
            var tab = App.TabPanelReports.add({
                id: data.Id,
                title: data.Caption,
                closable: true,
                tpl: window.App['SummaryReportTemplate' + data.Id] = Ext.create("Ext.net.XTemplate",
                    { proxyId: "SummaryReportTemplate" + data.Id, html: this.getTemplateByReport(rep, data) }),
                xtype: "dataview",
                disableSelection: true,
                emptyText: "Нет данных для отображения",
                itemSelector: this.getItemSelectorByReport(rep),
                store: this.getStoreByReport(rep, data),// { model: Ext.define("App.Model" + data.Id, { extend: "Ext.data.Model", fields: [{ name: "Title" }, { name: "Data" }, { name: "Days" }, { name: "tracker" }], idProperty: "Title" }), storeId: "Store" + data.Id, autoLoad: false, proxy: { type: 'memory' } },
                listeners: {
                    refresh: {
                        delay: 100,
                        fn: function (item) {
                            this.el.select('tr.customer-record').addClsOnOver('cust-name-over');
                        }
                    },
                    itemclick: this.getItemClikfnByReport(rep)
                }
            });
            App[data.Id].store.loadData(data.Data);
            this.storeAndActiveTab(tab, data, reportState);
        } else {
            tab.tab.setText(data.Caption);
            tab.tpl = Ext.create("Ext.net.XTemplate",
                {
                    html: this.getTemplateByReport(rep, data),
                    proxyId: tab.tpl.proxyId
                });
            tab.tpl.compile();
            tab.store.loadData(data.Data);
            this.storeAndActiveTab(tab, data, reportState, true);
        }
        this.updateGeoLocation(data.Data);
    };
    this.updateGeoLocation = function (data) {
        for (var i = 0; i < data.length; i++) {
            var v = data[i];
            for (var j = 0; j < v.Days.length; j++) {
                var day = v.Days[j];
                if (!day.items) { continue; }
                for (var k = 0; k < day.items.length; k++) {
                    var item = day.items[k];
                    if (item.parking) {
                        Core.GeoLocation.updateGeoLocationInReport(item.parking, item.parking.id);
                    }
                }
            }
        }
    };
    this.storeAndActiveTab = function (tab, data, reportState, updateReport) {
        this.isNewReport = false;
        tab.reportState = reportState;
        if (!updateReport) {
            tab.reportId = data.Id;
            this.reports.push(tab);
        }
        App.TabPanelReports.setActiveTab(tab);
    };
    this.getTemplateByReport = function (report, data) {
        switch (report) {
            case "1":
                return Ext.String.format('<div id="customers-ct"><div class="header"><p>{0}</p></div><table><tr><th style="width:15%">Дата</th><th style="width:20%">Время движения, ч</th><th style="width:20%">Время стоянки, ч</th><th style="width:18%">Средняя скорость, км/ч</th><th style="width:18%">Макс. скорость, км/ч</th><th style="width:15%">Пробег(расход), км</th></tr><tpl for="."><tr><td colspan="6"><div class="letter-row-div"><h2 class="letter-selector" >{tracker}</h2></div><tpl for="Days"><table class="data-table"><tr class="customer-record"><td style="width:15%">{g}</td><td style="width:20%">&nbsp;{f}</td><td style="width:20%">&nbsp;{e}</td><td style="width:18%">&nbsp;{b}</td><td style="width:18%">&nbsp;{c}</td><td style="width:10%">&nbsp;{d}</td></tr></table></tpl><tpl for="Summary"><table class="data-table"><tr class="summary-record"><td style="width:15%"></td><td style="width:20%">&nbsp;{moving}</td><td style="width:20%">&nbsp;{parking}</td><td style="width:18%">&nbsp;</td><td style="width:18%">&nbsp;</td><td style="width:10%">&nbsp;{distance}</td></tr></table></tpl></td></tr></tpl></table></div>', data.Title);
                //return App.DataView1.tpl.html;
                //return ["<div id=\"customers-ct\">", "<div class=\"header\">", "<p>" + data.Title + "</p>", "</div>", "<table>", "<tr>", "<th style=\"width:15%\">Дата</th>", "<th style=\"width:20%\">Время движения</th>", "<th style=\"width:20%\">Время стоянки</th>", "<th style=\"width:18%\">Средняя скорость, км/ч</th>", "<th style=\"width:18%\">Макс. скорость, км/ч</th>", "<th style=\"width:10%\">Пробег, км</th>", "</tr>", "", "<tpl for=\".\">", "<tr>", "<td class=\"letter-row\" colspan=\"6\">", "<div><h2 class=\"letter-selector\" >{tracker}</h2></div>", "<tpl for=\"Days\">", "<table>", "<tr class=\"customer-record\">", "<td style=\"width:15%\">{g}</td>", "<td style=\"width:20%\">&nbsp;{f}</td>", "<td style=\"width:20%\">&nbsp;{e}</td>", "<td style=\"width:18%\">&nbsp;{b}</td>", "<td style=\"width:18%\">&nbsp;{c}</td>", "<td style=\"width:10%\">&nbsp;{d}</td>", "</tr>", "</table>", "</tpl>", "</td>", "</tr>", "</tpl>", "</table>", "</div>", ""];
            case "2":
                return Ext.String.format('<div id="customers-ct"><div class="header"><p>{0}</p></div><table><tr><td colspan="4" style="text-align: center; padding: 5px;"><h3>Движение</h3></td><td colspan="4" align="center" ><h3>Стоянка</h3></td></tr><tr><th style="width:8%">Начало</th><th style="width:8%">Конец</th><th style="width:10%">Общее время</th><th style="width:12%">Пробег(расход), км</th><th style="width:8%">Начало</th><th style="width:8%">Конец</th><th style="width:10%">Общее время</th><th style="width:36%">Адрес</th></tr><tpl for="."><tr><td colspan="8"><div class="letter-row-div"><h2>{tracker}</h2></div><tpl for="Days"><div class="date-row" ><div class="date-row-div" onclick="detailItemClick(this)"><h3>{g}</h3></div><table class="data-table"><tpl for="items"><tr class="customer-record"><tpl for="moving"><td style="width:8%">{s}</td><td style="width:10%">&nbsp;{e}</td><td style="width:10%">&nbsp;{t}</td><td style="width:8%">&nbsp;{d}</td></tpl><tpl for="parking"><td style="width:8%" class="parking">&nbsp;{s}</td><td style="width:8%" >&nbsp;{e}</td><td style="width:10%">&nbsp;{t}</td><td id="geo_{id}" style="width:36%">&nbsp;{g}</td></tpl></tr></tpl></table></div></tpl></td></tr></tpl></table></div>', data.Title);
                //return App.DataViewDetail.tpl.html;
            case "3":
                return Ext.String.format('<div id="customers-ct"><div class="header"><p>{0}</p></div><table><tr><th style="width:15%">Начало</th><th style="width:15%">Конец</th><th style="width:15%">Общее время</th><th style="width:55%">Адрес</th></tr><tpl for="."><tr><td colspan="8"><div class="letter-row-div"><h2>{tracker}</h2></div><tpl for="Days"><div class="date-row" ><div class="date-row-div" onclick="detailItemClick(this)"><h3>{g}</h3></div><table class="data-table"><tpl for="items"><tr class="customer-record"><tpl for="parking"><td style="width:15%" class="parking">&nbsp;{s}</td><td style="width:15%" >&nbsp;{e}</td><td style="width:15%">&nbsp;{t}</td><td id="geo_{id}" style="width:55%">&nbsp;{g}</td></tpl></tr></tpl></table></div></tpl></td></tr></tpl></table></div>', data.Title);
                //return App.DataViewParking.tpl.html;
            default:
        }
    };
    this.getStoreByReport = function (report, data) {
        switch (report) {
            case "1":
                return { model: Ext.define("App.Model" + data.Id, { extend: "Ext.data.Model", fields: [{ name: "Title" }, { name: "Data" }, { name: "Days" }, { name: "Summary" }, { name: "tracker" }], idProperty: "Title" }), storeId: "Store" + data.Id, autoLoad: false, proxy: { type: 'memory' } };
            case "2":
            case "3":
                return { model: Ext.define("App.Model" + data.Id, { extend: "Ext.data.Model", fields: [{ name: "Title" }, { name: "Data" }, { name: "Days" }, { name: "tracker" }, { name: "g" }, { name: "moving" }], idProperty: "Title" }), storeId: "Store" + data.Id, autoLoad: false, proxy: { type: 'memory' } };
                //return App.Store1;
            default: return null;
        }
    };
    this.getItemSelectorByReport = function (report) {
        switch (report) {
            case "1":
                return "div.letter-row-div";
            case "2":
                return "div.letter-row-div";
            default:
                return "div.letter-row-div";
        }
    };
    this.getItemClikfnByReport = function (report) {
        switch (report) {
            case "1":
                return { fn: summaryItemClick };
            case "2":
            case "3":
                return { fn: detailItemClick };
            default:
                return { fn: summaryItemClick };
        }
    };
    this.optimizeData = function (data, rep) {
        if (rep == '1') {
            for (var i = 0; i < data.length; i++) {
                data[i].tracker = this.getCarNameById(data[i].tracker);
            }
        }
        else if (rep == '2' || rep == '3') {
            for (var i = 0; i < data.length; i++) {
                data[i].tracker = this.getCarNameById(data[i].tracker);
            }
        }
    };
    this.getDetailedReportConfigure = function (values) {
        var temp = [];
        var currentItem = null;
        for (var i = 0; i < values.length; i++) {
            if (!currentItem) {
                currentItem = values[i];
                temp.push(currentItem);
                continue;
            }
            if (currentItem.i == values[i].i) {
                currentItem.e = values[i].e;
                currentItem.d += values[i].d;
            } else {
                currentItem = values[i];
                temp.push(currentItem);
            }
        }
        var items = [];
        for (var j = 0; j < temp.length; j++) {
            var obj = {};
            if (temp[j].i) {
                obj.moving = temp[j];
                obj.parking = temp[++j];
            } else {
                obj.moving = { s: '', e: '', t: '', d: '' };
                obj.parking = temp[j];
            }
            this.optimizeSummaryReportObject(obj);
            items.push(obj);
        }
        return items;
    };
    this.optimizeSummaryReportObject = function (obj) {
        if (obj.moving) {
            obj.moving.t = common.stringTimesDiff(obj.moving.s, obj.moving.e);
            if (obj.moving.s)
                obj.moving.s = common.stringToTimePart(obj.moving.s);
            if (obj.moving.e)
                obj.moving.e = common.stringToTimePart(obj.moving.e);
            obj.moving.d = common.distanceReadable(obj.moving.d);
        }
        if (obj.parking) {
            obj.parking.t = common.stringTimesDiff(obj.parking.s, obj.parking.e);
            if (obj.parking.s)
                obj.parking.s = common.stringToTimePart(obj.parking.s);
            if (obj.parking.e)
                obj.parking.e = common.stringToTimePart(obj.parking.e);
        }
    };
    this.getCarNameById = function (id) {
        return App.gridPanelCars.store.getById(parseInt(id)).get('Name');
    };
    this.getDate = function (d) {
        return d.substr(0, d.indexOf('T'));
    };
    this.fromDateChanged = function () {
        this.validate();
        if (App.dfFrom.getValue() > App.dfTo.getValue()) {
            App.dfTo.setValue(App.dfFrom.getValue());
        }
    };
    this.toDateChanged = function () {
        this.validate();
        if (App.dfTo.getValue() < App.dfFrom.getValue()) {
            App.dfFrom.setValue(App.dfTo.getValue());
        }
    };
    this.showMask = function () {
        App.Viewport1.el.mask('Загрузка отчета...');
    };
    this.hideMask = function () {
        App.Viewport1.el.unmask();
    };
};

var detailItemClick = function (view, record, item, index, e) {
    if (record) {
        var group = e.getTarget("div.letter-row-div");

        if (group) {
            Ext.get(group)
                .toggleCls('collapsed');
            var td = Ext.get(group)
                .up('td');
            if (td) {
                td.select('div.date-row')
                    .toggleCls('collapsed');
            }
        }
    } else {
        Ext.get(view)
            .toggleCls('collapsed')
            .up('div')
            .select('table')
            .toggleCls("collapsed");
    }
};
var summaryItemClick = function (view, record, item, index, e) {
    var group = e.getTarget("h2.letter-selector");

    if (group) {
        Ext.get(group)
            .up("div")
            .toggleCls("collapsed")
            .up("td")
            .select("table")
            .toggleCls("collapsed");
    }
};

var printReport = function () {
    var strHtml = "<html>\n<head>\n <link rel=\"stylesheet\" type=\"text/css\"  href=\"style/settings.css\">\n</head><body>"
		+ App.TabPanelReports.activeTab.el.dom.innerHTML + "\n</body>\n</html>";
    var WinPrint = window.open('', '', 'letf=0,top=0,width=800,height=900,toolbar=0,scrollbars=0,status=0');
    WinPrint.document.write(strHtml);
    WinPrint.document.close();
    WinPrint.focus();
    WinPrint.print();
    delete WinPrint;
};