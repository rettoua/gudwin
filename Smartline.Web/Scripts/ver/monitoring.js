var monitoring = function () {
    this.lastDate = new Date();
    this.activityData = [];
    this.activityObject = { prev: 0, max: 0, count: 0 };
    this.activityDataForChart = [];
    this.loaded = true;

    this.setMonitoringEvents = function (r) {
        if (!r || r.length == 0) {
            return;
        }
        var grid = this.getMonitoringEventsGrid();
        grid.store.add(r);
        for (var i = 0; i < r.length; i++) {
            var d = common.parseISO8601(r[i].d);
            if (d > this.lastDate) {
                this.lastDate = d;
            }
        }
    };
    this.getMonitoringEventsGrid = function () {
        return App.gridMonitoringEvents;
    };
    this.updateOnlineActivity = function () {
        if (!this.loaded) {
            return;
        }
        this.loaded = false;
        var me = this;
        DR.LoadOnlineActivity(me.lastDate, {
            success: function (r) {
                if (!r) { return; }
                me.setOnlineActivityData(r.activity);
                me.setMonitoringEvents(r.events);
                me.loaded = true;
            },
            failure: function (e) {
                me.loaded = true;
            }
        });
    };
    this.setOnlineActivityData = function (data) {
        if (!data) {
            return;
        }
        data.update_time = common.parseISO8601(data.update_time);
        var exist = false;
        for (var i = 0; i < this.activityData.length; i++) {
            if (this.activityData[i].update_time == data.update_time) {
                exist = true;
                break;
            }
        }
        if (exist === false) {
            this.activityData.push(data);
            if (this.activityData.length > 50) {
                this.activityData.splice(0, 1);
                this.activityDataForChart.splice(0, 1);
            }
            this.refreshActivityChart(data);
        }
    };
    this.refreshActivityChart = function (data) {
        var chart = this.getActivityChart(),
            fromDate = this.activityData[0].update_time,
            toDate = data.update_time,
            numericAxis = this.getAxisByType(chart, 'Numeric'),
            timeAxis = this.getAxisByType(chart, 'Time');
        if (this.activityData.length > 1) {
            this.activityObject.count = data.packages - this.activityObject.prev;
        } else {
            this.activityObject.count = 0;
        }
        this.activityObject.prev = data.packages;
        this.activityDataForChart.push({ packages: this.activityObject.count, date: toDate });
        if (this.activityObject.count > this.activityObject.max) {
            this.activityObject.max = this.activityObject.count;
        }
        var max = 0;
        for (var i = 0; i < this.activityDataForChart.length; i++) {
            if (this.activityDataForChart[i].packages > max) {
                max = this.activityDataForChart[i].packages;
            }
        }
        if (max < this.activityObject.max) {
            this.activityObject.max = max;
        }
        numericAxis.maximum = this.activityObject.max < 10 ? 10 : this.activityObject.max;
        timeAxis.fromDate = fromDate;
        timeAxis.toDate = toDate;
        chart.store.loadData(this.activityDataForChart);
        this.updateConterDatas(data.connected_trackers, data.packages);
    };
    this.getActivityChart = function () {
        return App.OnlineActivityChart;
    };
    this.getAxisByType = function (chart, type) {
        for (var i = 0; i < chart.axes.items.length; i++) {
            if (chart.axes.items[i].type == type) {
                return chart.axes.items[i];
            }
        }
        return "";
    };
    this.updateConterDatas = function (trackers, packages) {
        var txtTrackers = this.getTxtTotalTrackers(),
            txtPackages = this.getTxtPackages();
        txtTrackers.setText(Ext.String.format(this.getCounterPatternStr(), 'Подключено трекеров', trackers), false);
        txtPackages.setText(Ext.String.format(this.getCounterPatternStr(), 'Пакетов за сессию', packages), false);
    };
    this.getTxtTotalTrackers = function () {
        return App.txtTotalTrackers;
    };
    this.getTxtPackages = function () {
        return App.txtPackages;
    };
    this.getCounterPatternStr = function () {
        return '<span class="online-counter">{0}: </span><span class="online-counter-value">{1}</span>';
    };
};

var traffic = function () {
    this.state = {
        fields: [], newFields: [], rotate: function () {
            this.fields = this.newFields;
            this.newFields = [];
        }
    };
    this.addToWatch = function () {
        var records = this.getToWatchRecords();
        if (!records || records.length == 0) { return; }
        var destinationGrid = this.getSelectedTrackersGrid();
        var sourceGrid = this.getAllTrackersGrid();
        this.moveRecords(records, sourceGrid, destinationGrid);
        this.loadTraffic(records);
    };
    this.removeFromWatch = function () {
        var records = this.getFromWatchRecords();
        if (!records || records.length == 0) { return; }
        var sourceGrid = this.getSelectedTrackersGrid();
        var destinationGrid = this.getAllTrackersGrid();
        this.moveRecords(records, sourceGrid, destinationGrid);
    };
    this.moveRecords = function (records, sourceGrid, destinationGrid) {
        sourceGrid.store.suspendEvents();
        destinationGrid.store.suspendEvents();
        for (var i = 0; i < records.length; i++) {
            var r = records[i].data;
            destinationGrid.store.add(r);
            sourceGrid.store.remove(records[i]);
        }
        sourceGrid.store.resumeEvents();
        destinationGrid.store.resumeEvents();
        sourceGrid.view.refresh();
        destinationGrid.view.refresh();
    };
    this.getToWatchRecords = function () {
        var grid = this.getAllTrackersGrid();
        return grid.selModel.getSelection();
    };
    this.getFromWatchRecords = function () {
        var grid = this.getSelectedTrackersGrid();
        return grid.selModel.getSelection();
    };
    this.getAllTrackersGrid = function () {
        return App.gridAllTrackers;
    };
    this.getSelectedTrackersGrid = function () {
        return App.gridSelectedTrackers;
    };
    this.onlineTrackerRenderer = function (value, meta, record, index) {
        if (record.data.IsOnline == true) {
            return Ext.String.format('<span style="color: green;font-weight: bold;">{0}</span>', value);
        }
        return value;
    };
    this.bytesRenderer = function (value, meta, record, index) {
        var bts = bytesToSize(value);
        return bts == '' ? '' : bts + ' (' + value + ')';
    };
    this.packagesRenderer = function (value, meta, record, index) {
        var r = '';
        for (var i in value) {
            if (r == '') {
                r += i + ' - ' + value[i];
            } else {
                r += ', ' + i + ' - ' + value[i];
            }
        }
        return r;
    };
    this.loadTraffic = function (recs) {
        var from = this.getFromValue(),
            to = this.getToValue(),
            ids = [],
            me = this;
        for (var i = 0; i < recs.length; i++) {
            ids.push(recs[i].data.Id);
        }
        DR.LoadTraffic(ids, from, to, {
            success: function (r) {
                var result = eval('(' + r + ')');
                me.addTrafficResultToGrid(result);
            },
            failure: function (e) {
                //TODO: nothing. probably it's make sense to create error log
            }
        });
    };
    this.getFromValue = function () {
        return App.fldFromDate.getValue();
    };
    this.getToValue = function () {
        return App.fldToDate.getValue();
    };
    this.setFromValue = function (v) {
        App.fldFromDate.setValue(v);
    };
    this.setToValue = function (v) {
        App.fldToDate.setValue(v);
    };
    this.addTrafficResultToGrid = function (result) {
        var grid = this.getSelectedTrackersGrid();
        grid.store.suspendEvents();
        for (var id in result) {
            var record = grid.store.getById(parseInt(id));
            var traffic = result[id];
            for (var i = 0; i < traffic.length; i++) {
                record.data.In += traffic[i].in;
                record.data.Out += traffic[i].out;
                if (!record.data.Packages) {
                    record.data.Packages = [];
                }
                this.appendPackage(record.data.Packages, traffic[i].p);
                var date = common.parseISO8601(traffic[i].d),
                    fldName = this.getFieldNameByDate(date);
                record.data[fldName + '_i'] = traffic[i].in;
                record.data[fldName + '_o'] = traffic[i].out;
                record.data[fldName + '_p'] = traffic[i].p;
            }
        }
        grid.store.resumeEvents();
        grid.view.refresh();
    };
    this.appendPackage = function (source, value) {
        for (var i in value) {
            if (source[i]) {
                source[i] += value[i];
            } else {
                source[i] = value[i];
            }
        }
    };
    this.resetTotalValues = function (recs) {
        for (var i = 0; i < recs.length; i++) {
            var r = recs[i];
            r.data.In = 0;
            r.data.Out = 0;
            r.data.Packages = [];
        }
    };
    this.applyFilterByStatus = function () {
        var store = this.getAllTrackersGrid().store;
        store.filterBy(this.filterByStatus);
    };
    this.filterByStatus = function (record) {
        var onlyOnline = App.btnFilterByStatus.pressed;
        return record.data.IsOnline === onlyOnline;
    };
    this.fromDateChanged = function () {
        var fValue = this.getFromValue(),
            tValue = this.getToValue();
        if (fValue > tValue) {
            this.setToValue(fValue);
        }
    };
    this.toDateChanged = function () {
        var fValue = this.getFromValue(),
            tValue = this.getToValue();
        if (fValue > tValue) {
            this.setFromValue(tValue);
        }
    };
    this.calc = function () {
        this.state.f = this.getFromValue();
        this.state.t = this.getToValue();
        this.reconfigureGrid();
    };
    this.reconfigureGrid = function () {
        //DR.ReconfigureTrafficGrid();
        //return;
        this.state.newFields = [];
        var startDate = this.state.f,
            grid = this.getSelectedTrackersGrid();
        grid.store.suspendEvents();
        grid.view.suspendEvents();
        this.tryRemoveUnneccessaryColumns();
        do {
            this.checkColumn(startDate);
            startDate.setDate(startDate.getDate() + 1);
        } while (startDate <= this.state.t)
        this.loadTrafficProcess();
        grid.view.resumeEvents();
        grid.store.resumeEvents();
        grid.view.refresh();
    };
    this.checkColumn = function (date) {
        var grid = this.getSelectedTrackersGrid(),
            fldName = this.getFieldNameByDate(date),
            fldNameIn = fldName + '_i',
            fldNameOut = fldName + '_o',
            fldNamePackages = fldName + '_p';
        this.state.fields[fldNameIn] = { i: fldNameIn, o: fldNameOut, p: fldNamePackages };
        grid.store.addField({ name: fldNameIn, type: 'int' });
        grid.store.addField({ name: fldNameOut, type: 'int' });
        grid.store.addField({ name: fldNamePackages, type: 'object' });
        this.addColumn(grid, fldNameIn, fldNameOut, fldNamePackages, date);
    };
    this.addColumn = function (g, i, o, p, d) {
        var commonColumnName = this.getColumnNameByDate(d);
        //var commonColumn = new Ext.grid.Column({
        //    header: commonColumnName,
        //    locked: true,
        //    lockable: true,            
        //    id: i
        //});
        //commonColumn.columns = [];
        //commonColumn.columns.push(new Ext.grid.Column({
        //    header: "Входящий",
        //    dataIndex: i,
        //    width: 65
        //}));
        //commonColumn.columns.push(new Ext.grid.Column({
        //    header: "Исходящий",
        //    dataIndex: o,
        //    width: 70
        //}));
        //commonColumn.columns.push(new Ext.grid.Column({
        //    header: "Пакетов",
        //    dataIndex: p,
        //    width: 65
        //}));        
        g.addColumn(new Ext.grid.Column({
            header: commonColumnName + "<br />Входящий",
            dataIndex: i,
            width: 65,
            id: i,
            renderer: TRAFFIC.bytesRenderer
        }));
        g.addColumn(new Ext.grid.Column({
            header: commonColumnName + "<br />Исходящий",
            dataIndex: o,
            width: 70,
            id: o,
            renderer: TRAFFIC.bytesRenderer
        }));
        g.addColumn(new Ext.grid.Column({
            header: commonColumnName + "<br />Пакетов",
            dataIndex: p,
            width: 65,
            id: p,
            renderer: TRAFFIC.packagesRenderer
        }));
    };
    this.removeColumn = function (obj) {
        var grid = this.getSelectedTrackersGrid();
        this.removeColumnProccess(grid, obj.i);
        this.removeColumnProccess(grid, obj.o);
        this.removeColumnProccess(grid, obj.p);
        grid.store.model.prototype.fields.removeAtKey(obj.i);
        grid.store.model.prototype.fields.removeAtKey(obj.o);
        grid.store.model.prototype.fields.removeAtKey(obj.p);
    };
    this.removeColumnProccess = function (g, c) {
        for (var i = 0; i < g.columns.length; i++) {
            if (g.columns[i].id == c) {
                g.removeColumn(i);
                break;
            }
        }
    };
    this.tryRemoveUnneccessaryColumns = function () {
        for (var k in this.state.fields) {
            this.removeColumn(this.state.fields[k]);
        }
        this.state.fields = [];
    };
    this.loadTrafficProcess = function () {
        var recs = this.getSelectedTrackersGrid().store.getAllRange();
        this.resetTotalValues(recs);
        this.loadTraffic(recs);
    };
    this.getFieldNameByDate = function (date) {
        return Ext.Date.format(date, 'f_d_m_Y');
    };
    this.getColumnNameByDate = function (date) {
        return Ext.Date.format(date, 'd.m.Y');
    };
};

var MON = new monitoring();
var TRAFFIC = new traffic();