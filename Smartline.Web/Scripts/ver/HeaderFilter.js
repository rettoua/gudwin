Ext.ux.HeaderFilter = Ext.extend(Ext.util.Observable, {
    automaticSave: true,

    constructor: function (config) {
        Ext.apply(this, config);
        Ext.ux.HeaderFilter.superclass.constructor.call(this);

        this.filterSets = this.filterSets || {};
        var sets = {};
        Ext.iterate(this.filterSets, function (key, value) {
            sets[key.Name] = key.Values;
        }, this);

        this.filterSets = sets;
    },

    init: function (grid) {
        this.grid = grid;
        this.view = grid.getView();
        this.cm = grid.getColumnModel();

        this.view.renderHeaders = this.renderHeaders;
        this.view.updateHeaderWidth = this.updateHeaderWidth;
        this.view.refresh = this.refresh;
        this.view.processEvent = this.view.processEvent.createInterceptor(this.processEvent, this);
        this.addStoredFilter();
        this.createMenu();

        this.each(function (cmp) {
            cmp.grid = this.grid;
        });

        this.prevFilters = {};
        if (this.menuOwner)
            if (!this.remote) {
                setInterval(this.runFiltering.createDelegate(this), 250);

                this.menuOwner.on('click', this.enableFilters, this);
            } else {
                this.menuOwner.on('click', this.applyRemoteFilter, this);
                this.grid.store.on('beforeload', this.onBeforeLoad, this);
                this.applySet = this.applySet.createSequence(this.applyRemoteFilter, this);
                this.removeSet = this.removeSet.createSequence(this.applyRemoteFilter, this);
            }

        if (this.values) {
            this.grid.on("viewready", function () {
                this.missSave = true;
                this.applySet(this.values);
                delete this.values;
            }, this, { single: true });
        }
        this.grid.view.on("AfterRender", this.initFilterRow, this, { single: true });
        if (this.enableFilter == '') this.enableFilter = 'enable';
    },

    initFilterRow: function () {
        if (this.enableFilter == 'disable') {
            this.disableFilters();
        }
    },

    getFilterState: function () {
        var o = {},
            values = {};

        this.each(function (cmp) {
            var value = cmp.getValue();
            if (!Ext.isEmpty(value, false)) {
                values[cmp.dataIndex] = value;
            }
        });

        o.filters = {};

        o.filters.values = values;
        o.filters.sets = this.filterSets;

        return o;
    },

    runFiltering: function () {
        var changed = false;
        this.each(function (cmp) {
            var value = cmp.getValue();
            if (Ext.isEmpty(value, false) && !Ext.isDefined(this.prevFilters[cmp.dataIndex])) {
                return;
            }

            var eq = false,
                prevValue = this.prevFilters[cmp.dataIndex];

            if (Ext.isDate(value) && Ext.isDate(prevValue)) {
                eq = value.getTime() !== prevValue.getTime();
            }
            else {
                eq = value !== prevValue;
            }

            if (eq) {
                if (cmp.updateClearButtonVisibility) {
                    cmp.updateClearButtonVisibility();
                }
                changed = true;
                this.prevFilters[cmp.dataIndex] = value;
            }
        });

        if (changed) {
            this.applyFilter();
        }
    },

    createMenu: function () {
        Ext.net.ResourceMgr.registerIcon(["Decline", "Disk", "BulletTick", "NoteDelete"]);

        var items = [];
        Ext.iterate(this.filterSets, function (key, value) {
            items.push({
                itemId: key,
                text: key,
                cls: "x-active-delete",
                iconCls: "icon-filter",
                handler: this.onSetClick,
                scope: this
            });
        }, this);

        items.push({
            text: "Save Filter",
            iconCls: "icon-disk",
            handler: this.saveFilterPrompt,
            scope: this
        });

        items.push("-");

        items.push({
            text: "Clear Filter",
            iconCls: "icon-decline",
            handler: this.clearFilter,
            scope: this
        });
        items.push("-");
        items.push({
            text: "Hide Filter Row",
            iconCls: "icon-notedelete",
            handler: this.disableFilters,
            scope: this
        });

        this.menu = { items: items };
        var o = null;
        if (!this.grid.topToolbar)
            return;
        this.grid.topToolbar.items.each(function (item) {
            if (item.id.indexOf('btnFilterMenu') != -1) {
                o = item;
            }
        });

        this.menuOwner = o;
        if (this.remote) {
            this.menuOwner.setText('Apply Filter');
        }
        this.menu = Ext.menu.MenuMgr.get(this.menu);
        this.menu.ownerCt = this.menuOwner;
        this.menuOwner.menu = this.menu;
    },

    disableFilters: function () {
        this.menuOwner.hideMenuArrow();
        this.menuOwner.setText('Enable Filters');
        this.menuOwner.setIcon('icon-bullettick');
        this.menuOwner.setIconClass('icon-bullettick');
        this.enableFilter = 'disable';
        this.grid.addClass('hideFilter');
        this.grid.syncSize();
    },

    enableFilters: function () {
        this.menuOwner.showMenuArrow();
        this.menuOwner.setText(this.remote ? 'Apply Filters' : 'Filters');
        this.menuOwner.setIcon('icon-accept');
        this.menuOwner.setIconClass('icon-accept');
        this.enableFilter = 'enable';
        this.grid.removeClass('hideFilter');
        this.syncEditorSize();
    },

    syncEditorSize: function () {
        this.each(function (cmp) {
            cmp.setHeight(22);
            cmp.setWidth(cmp.getWidth() + 1);
        });
        this.grid.syncSize();
    },

    saveFilterPrompt: function (name) {
        if (!name || !Ext.isString(name)) {
            Ext.Msg.prompt("Save Filter", "Filter Name:", function (btn, name) {
                if (btn == "ok") {
                    this.saveFilter(name);
                }
            }, this);
            return;
        }

        this.saveFilter(name);
    },

    saveFilter: function (name) {
        var _set = this.filterSets[name],
            addToMenu = false;
        if (!_set) {
            _set = {};
            this.filterSets[name] = _set;
            addToMenu = true;
        }

        this.getFilterState(_set);

        var uid = this.grid.supperObject.uId;
        var e = this.grid.supperObject.entity;
        InOrder.Ajax.SaveSamplerFilter(uid, _set, e, name);

        if (addToMenu) {
            this.addFilterToMenu(name);
        }
    },

    getFilterState: function (_set) {
        this.each(function (cmp) {
            var value = cmp.getValue();
            if (!Ext.isEmpty(value, false)) {
                _set[cmp.dataIndex] = cmp.getValue();
            }
        });
    },

    addFilterToMenu: function (name) {
        this.menu.insert(this.menu.items.getCount() - 3, {
            itemId: name,
            text: name,
            cls: "x-active-delete",
            iconCls: "icon-filter",
            handler: this.onSetClick,
            scope: this
        });
        this.menu.doLayout();
    },

    onSetClick: function (item, e) {
        if (e.getTarget('.x-menu-item-icon')) {
            Ext.Msg.confirm("Warning", "Do you really want to delete filter <b>" + item.text + "</b>?", function (btn) {
                if (btn == "yes") {
                    this.removeSet(item.text);
                }
            }, this);
        }
        else {
            this.applySet(item.text);
        }
    },

    removeSet: function (name) {
        delete this.filterSets[name];
        var item = this.menu.getComponent(name);
        if (item) {
            this.menu.remove(item, true);
            this.menu.doLayout();
            InOrder.Ajax.RemoveSamplerFilter(this.grid.supperObject.uId, this.grid.supperObject.entity, name);
        }
    },

    applySet: function (name, s) {
        var _set;

        if (Ext.isString(name)) {
            _set = this.filterSets[name];
        }
        else if (Ext.isObject(name)) {
            _set = name;
        } else if (s) {
            _set = s;
        }

        this.each(function (cmp) {
            if (Ext.isDefined(_set[cmp.dataIndex])) {
                cmp.setValue(_set[cmp.dataIndex]);
                if (cmp.getValue() != _set[cmp.dataIndex])
                    cmp.el.dom.value = _set[cmp.dataIndex];
            }
            else {
                cmp.clear();
            }
        });
    },

    processEvent: function (name, e) {
        var target = e.getTarget(".filter-menu-trigger");
        if (target) {
            if (this.menu.isVisible()) {
                this.menu.hide();
            }
            this.menu.show(target, "tr-br?");

            e.stopEvent();
            return false;
        }
    },

    updateHeaderWidth: function (updateMain) {
        var innerHdChild = this.innerHd.firstChild,
            totalWidth = this.getTotalWidth();

        innerHdChild.style.width = this.getOffsetWidth();
        innerHdChild.firstChild.style.width = totalWidth;

        if (updateMain !== false) {
            this.mainBody.dom.style.width = (this.cm.getTotalWidth() + 4) + "px";
        }
    },

    refresh: function (headersToo) {
        this.fireEvent('beforerefresh', this);
        this.grid.stopEditing(true);

        var result = this.renderBody();
        this.mainBody.update(result).setWidth((this.cm.getTotalWidth() + 4) + "px");
        if (headersToo === true) {
            this.updateHeaders();
            this.updateHeaderSortState();
        }
        this.processRows(0, true);
        this.layout();
        this.applyEmptyText();
        this.fireEvent('refresh', this);
    },

    renderHeaders: function () {
        var colModel = this.cm,
            templates = this.templates,
            headerTpl = templates.hcell,
            properties = {},
            colCount = colModel.getColumnCount(),
            last = colCount - 1,
            cells = [],
            i, cssCls;

        for (i = 0; i < colCount; i++) {
            if (i == 0) {
                cssCls = 'x-grid3-cell-first ';
            } else {
                cssCls = i == last ? 'x-grid3-cell-last ' : '';
            }

            properties = {
                id: colModel.getColumnId(i),
                value: colModel.getColumnHeader(i) || '',
                style: this.getColumnStyle(i, true),
                css: cssCls,
                tooltip: this.getColumnTooltip(i)
            };

            if (colModel.config[i].align == 'right') {
                properties.istyle = 'padding-right: 16px;';
            } else {
                delete properties.istyle;
            }

            cells[i] = headerTpl.apply(properties);
        }

        var result = templates.header.apply({
            cells: cells.join(""),
            tstyle: String.format("width: {0};", this.getTotalWidth())
        });

        return result;
        return result.replace("</td></tr></tbody></table>", '</td><td style="width:' + Ext.ux.HeaderFilter.triggerWidth + 'px;"><a href="#" hidefocus="on" class="filter-menu-trigger"><div></div></a></td></tr></tbody></table>');
    },

    filterString: function (value, dataIndex, record) {
        var val = record.get(dataIndex);

        if (Ext.isNumber(val)) {
            if (!Ext.isEmpty(value, false) && val != value) {
                return false;
            }

            return true;
        }

        if (typeof val != "string") {
            return value.length == 0;
        }

        return Ext.net.StringUtils.startsWith(val.toLowerCase(), value.toLowerCase());
    },

    filterDate: function (value, dataIndex, record) {
        var val = record.get(dataIndex).clearTime(true).getTime();

        if (!Ext.isEmpty(value, false) && val != value.clearTime(true).getTime()) {
            return false;
        }
        return true;
    },

    filterNumber: function (value, dataIndex, record) {
        var val = record.get(dataIndex);

        if (!Ext.isEmpty(value, false) && val != value) {
            return false;
        }

        return true;
    },

    filterAuto: function (value, dataIndex, record) {
        var val = record.get(dataIndex);

        if (!Ext.isEmpty(value, false) && val != value) {
            return false;
        }

        return true;
    },

    addStoredFilter: function () {
        if (this.headerFilter) {
            this.headerFilter = eval('(' + this.headerFilter + ')');
            for (var key in this.headerFilter) {
                //                this.addFilterToMenu(key);
                var filter = this.headerFilter[key];
                if (filter && filter != "")
                    this.filterSets[key] = eval('(' + filter + ')');
                else this.filterSets[key] = {};
            }

        }
    },

    applyFilter: function () {
        var store = this.grid.getStore();
        store.suspendEvents();
        store.filterBy(this.getRecordFilter());
        store.resumeEvents();
        this.view.refresh(false);
    },

    applyRemoteFilter: function () {
        if (this.enableFilter == 'disable') {
            this.enableFilters();
        } else {
            this.grid.bottomToolbar.changePage(1);
        }
    },

    onBeforeLoad: function (store, options) {
        options.params = options.params || {};
        delete options.params["filters"];
        var v = this.getFilterValues();
        options.params["filters"] = Ext.encode(v);
    },

    getFilterValues: function () {
        var values = {};

        this.each(function (cmp) {
            var val, type;
            if (!Ext.isEmpty(cmp.getValue(), false)) {
                if (cmp.isSmart) {
                    type = this.getCmpType(cmp);
                    val = cmp.getSmartValue(type);

                    values[cmp.dataIndex] = val;
                }
                else {
                    val = cmp.getFilterValue ? cmp.getFilterValue() : cmp.getValue();

                    if (!Ext.isEmpty(val, false)) {
                        values[cmp.dataIndex] = {
                            type: this.getCmpType(cmp),
                            dataIndex: cmp.dataIndex,
                            value: val
                        };
                    }
                }
            }
        });

        return values;
    },

    getCmpType: function (cmp) {
        var type;
        this.grid.store.fields.each(function (field) {
            if (field.name == cmp.dataIndex) {
                type = field.type.type;
                return false;
            }
        }, this);

        return type;
    },

    getRecordFilter: function () {
        if (this.filterFn) {
            return this.filterFn;
        }

        var f = [],
            len;

        this.each(function (cmp) {
            var me = this;

            f.push((function (record) {
                var fn = me.filterAuto,
                    value = cmp.getValue(),
                    dataIndex = cmp.dataIndex;

                if (cmp.filterFn) {
                    fn = cmp.filterFn;
                }
                else if (Ext.isDate(value)) {
                    fn = me.filterDate;
                }
                else if (Ext.isNumber(value)) {
                    fn = me.filterNumber;
                }
                else if (Ext.isString(value)) {
                    fn = me.filterString;
                }

                return fn(value, dataIndex, record);
            }));
        });

        len = f.length;

        this.filterFn = function (record) {
            for (var i = 0; i < len; i++) {
                if (!f[i](record)) {
                    return false;
                }
            }
            return true;
        };

        return this.filterFn;
    },

    clearFilter: function () {
        var cols = this.view.headerRows[0].columns,
            colIndex = 0;

        for (colIndex; colIndex < cols.length; colIndex++) {
            this.each(function (cmp) {
                if (Ext.isFunction(cmp.reset)) {
                    cmp.clear();
                }
            });
        }

        this.grid.store.clearFilter();
        if (this.grid.supperObject)
            this.grid.supperObject.headerFilter = null;
        if (this.remote) {
            this.applyRemoteFilter();
        }
    },

    each: function (fn, excludeDisplayField) {
        if (!Ext.isFunction(fn)) {
            return;
        }

        var cols = this.view.headerRows[0].columns,
            colIndex = 0;

        for (colIndex; colIndex < cols.length; colIndex++) {
            var col = cols[colIndex],
                cmp = col.component;

            if (excludeDisplayField !== false && (!Ext.isFunction(cmp.getValue) || (cmp instanceof Ext.form.DisplayField))) {
                continue;
            }

            var ans = fn.call(this, cmp, colIndex);
            if (ans === false) {
                break;
            }
        }
    }
});
