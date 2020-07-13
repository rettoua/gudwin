/*
 * @version   : 1.0.0 - Professional Edition (Ext.Net Professional License)
 * @author    : Ext.NET, Inc. http://www.ext.net/
 * @date      : 2010-06-15
 * @copyright : Copyright (c) 2006-2010, Ext.NET, Inc. (http://www.ext.net/). All rights reserved.
 * @license   : See license.txt and http://www.ext.net/license/. 
 * @website   : http://www.ext.net/
 */



// @source data/HttpProxy.js

Ext.data.HttpProxy.prototype.doRequest = function (action, rs, params, reader, cb, scope, arg) {
    var o = {
        method : (this.api[action]) ? this.api[action].method : undefined,
        request : {
            callback: cb,
            scope: scope,
            arg: arg
        },
        reader : reader,
        callback : this.createCallback(action, rs),
        scope : this
    };

    if (this.conn.json) {
        o.jsonData = params;

        if ((o.method || this.conn.method) === "GET") {
           o.params = params || {};
        }
    } else if (params.jsonData) {
        o.jsonData = params.jsonData;
    } else if (params.xmlData) {
        o.xmlData = params.xmlData;
    } else {
        o.params = params || {};
    }

    this.conn.url = this.buildUrl(action, rs);

    if (this.useAjax) {

        Ext.applyIf(o, this.conn);
        this.activeRequest[action] = Ext.Ajax.request(o);
    } else {
        this.conn.request(o);
    }

    this.conn.url = null;
};

// @source data/HttpWriteProxy.js

Ext.net.HttpWriteProxy = function (conn) {
    Ext.net.HttpWriteProxy.superclass.constructor.call(this, {});
    
    this.conn = conn;
    this.useAjax = !conn || !conn.events;
        
    if (conn && conn.handleSaveResponseAsXml) {
        this.handleSaveResponseAsXml = conn.handleSaveResponseAsXml;
    }
};

Ext.extend(Ext.net.HttpWriteProxy, Ext.data.HttpProxy, {
    handleSaveResponseAsXml : false,
    
    save : function (params, reader, callback, scope, arg) {
        if (this.fireEvent("beforesave", this, params) !== false) {
            var o = {
                params   : params || {},
                request  : {
                    callback : callback,
                    scope    : scope,
                    arg      : arg
                },
                reader   : reader,
                scope    : this,
                callback : this.saveResponse
            };
            
            if (this.conn.json) {
                o.jsonData = params;
            }
            
            if (this.useAjax) {
                Ext.applyIf(o, this.conn);
                o.url = this.conn.url;
                
                if (this.activeRequest) {
                    Ext.Ajax.abort(this.activeRequest);
                }

                this.activeRequest = Ext.Ajax.request(o);
            } else {
                this.conn.request(o);
            }
        } else {
            callback.call(scope || this, null, arg, false);
        }
    },

    saveResponse : function (o, success, response) {
        delete this.activeRequest;
        
        if (!success) {
            this.fireEvent("saveexception", this, o, response, { message : response.statusText });
            this.fireEvent("exception", this, "response", "write", o, response, { message : response.statusText });
            o.request.callback.call(o.request.scope, null, o.request.arg, false);

            return;
        }
        
        var result;
        
        try {
            if (!this.handleSaveResponseAsXml) {
                var json = response.responseText,
                    responseObj = eval("(" + json + ")");
                    
                result = {
                    success : responseObj.success,
                    msg     : responseObj.message,
                    data    : responseObj.data
                };
            } else {
                var doc = response.responseXML,
                    root = doc.documentElement || doc,
                    q = Ext.DomQuery,
                    sv = q.selectValue("Success", root, false),
                    data = q.selectValue("Data", root, undefined);
                    
                success = sv !== false && sv !== "false";
                if (data) {
                    data = Ext.decode(data);
                }
                
                result = { 
                    success : success, 
                    msg     : q.selectValue("Message", root, ""),
                    data    : data
                };
            }
        } catch (e) {
            this.fireEvent("saveexception", this, o, response, e);
            this.fireEvent("exception", this, "remote", "write", o, response, e);
            o.request.callback.call(o.request.scope, null, o.request.arg, false);

            return;
        }
        
        if (result.success) {
            this.fireEvent("save", this, o, o.request.arg);
        } else {
            this.fireEvent("saveexception", this, o, response, { message : result.msg });
        }
        
        o.request.callback.call(o.request.scope, result, o.request.arg, result.success);
    }
});

// @source data/GroupingStore.js


// @source data/Store.js

Ext.data.Record.AUTO_ID = -1;

Ext.data.Record.id = function (rec) {
    rec.phantom = true;
    return Ext.data.Record.AUTO_ID--;   
};

Ext.data.Record.prototype.commit = Ext.data.Record.prototype.commit.createInterceptor(function () {
    if (this.newRecord) {
        this.newRecord = false; 
    }
});

Ext.data.Record.prototype.isNew = function () {
    return this.newRecord;
};

Ext.data.Store.override({
    metaId : function () {
        if (this.reader.isArrayReader) {
            var id = Ext.num(parseInt(this.reader.meta.idIndex === 0 ? this.reader.meta.idIndex : (this.reader.meta.idIndex || this.reader.meta.id), 10), -1);

            if (id !== -1) {
                return this.reader.meta.fields[id].name;
            }
        }

        return this.reader.meta.idIndex || this.reader.meta.idProperty || this.reader.meta.idPath || this.reader.meta.id;
    }
});

Ext.net.Store = function (config) {
    Ext.apply(this, config);

    this.deleted = [];

    this.addEvents(
        "beforesave",
        "save",
        "saveexception",
        "commitdone",
        "commitfailed"
    );
        
    if (this.proxyId) {
        this.storeId = this.proxyId;
        Ext.StoreMgr.register(this);
    }

    if (this.updateProxy) {
        this.relayEvents(this.updateProxy, ["saveexception"]);
    }

    if (!Ext.isEmpty(this.updateProxy)) {
        this.on("saveexception", function (ds, o, response, e) {
            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", response, { "errorMessage": e.message }, null, null, null, null, o) !== false) {
                if (this.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure(response, e.message);
                }
            }
        }, this);
    }

    if (this.proxy && !this.proxy.refreshByUrl && !this.proxy.isDataProxy) {
        this.on("loadexception", function (ds, o, response, e) {
            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", response, { "errorMessage": response.statusText }, null, null, null, null, o) !== false) {
                if (this.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure(response, response.responseText);
                }
            }
        }, this);
    }

    if (this.beforeLoadParams) {
        this.on("beforeload", this.beforeLoadParams);
    }

    if (this.beforeSaveParams) {
        if (this.restful) {
            this.on("beforewrite", function (store, action, rs, options) { 
                return this.beforeSaveParams(store, options); 
            }, this);
        } else {
            this.on("beforesave", this.beforeSaveParams);
        }
    }

    if (this.autoSave || this.restful) {
        this.writer = new Ext.data.JsonWriter({ writeAllFields: this.saveAllFields });
        this.on("write", function (store, action, result, res, rs) {
            this.fireEvent("save", store, result, res);
        }, this);
    }
    
    if (this.proxy instanceof Ext.data.PagingMemoryProxy && (this.autoLoad || this.deferLoad)) {        
        this.deferAutoLoad = true;
        this.autoLoad = false;        
    }
    
    this.on("load", function () {
        this.isLoaded = true;
    }, this, { single : true });

    Ext.net.Store.superclass.constructor.call(this, config);
    
    if (this.deferAutoLoad) {
        this.load(typeof this.deferAutoLoad === "object" ? this.deferAutoLoad : undefined);
        this.deferAutoLoad = false;
    }
};

Ext.extend(Ext.net.Store, Ext.data.GroupingStore, {
    pruneModifiedRecords : true,
    warningOnDirty : true,
    
    dirtyWarningTitle : "Uncommitted Changes",
    
    dirtyWarningText : "You have uncommitted changes.  Are you sure you want to reload data?",
    updateProxy : null,

    // "none" - no refresh after saving
    // "always" - always refresh after saving
    // "auto" - auto refresh. If no new records then refresh doesn't perfom. If new records exists then refresh will be perfom for refresh id fields
    refreshAfterSave     : "Auto",
    useIdConfirmation    : false,
    showWarningOnFailure : true,
    autoSave      : false,
    saveAllFields : true,
    
    sortData : function () {
        var sortInfo  = this.hasMultiSort ? this.multiSortInfo : this.sortInfo,
            direction = sortInfo.direction || "ASC",
            sorters   = sortInfo.sorters,
            sortFns   = [];

        //if we just have a single sorter, pretend it's the first in an array
        if (!this.hasMultiSort) {
            sorters = [{direction: direction, field: sortInfo.field}];
        }
        
        if (!sorters || sorters.length === 0) {
            return;
        }

        //create a sorter function for each sorter field/direction combo
        var i,
            j;

        for (i = 0, j = sorters.length; i < j; i++) {
            if (sorters[i] && sorters[i].field) {
                sortFns.push(this.createSortFunction(sorters[i].field, sorters[i].direction));
            }
        }
        
        if (sortFns.length === 0) {
            return;
        }

        //the direction modifier is multiplied with the result of the sorting functions to provide overall sort direction
        //(as opposed to direction per field)
        var directionModifier = direction.toUpperCase() === "DESC" ? -1 : 1;

        //create a function which ORs each sorter together to enable multi-sort
        var fn = function (r1, r2) {
            var result = sortFns[0].call(this, r1, r2);

            //if we have more than one sorter, OR any additional sorter functions together
            if (sortFns.length > 1) {
                var i,
                    j;

                for (i = 1, j = sortFns.length; i < j; i++) {
                    result = result || sortFns[i].call(this, r1, r2);
                }
            }

            return directionModifier * result;
        };
        
        if (this.isPagingStore() && this.allData) {
            this.data = this.allData;
            delete this.allData;
        }

        //sort the data
        this.data.sort(direction, fn);

        if (this.snapshot && this.snapshot != this.data) {
            this.snapshot.sort(direction, fn);
        }
        
        if (this.applyPaging) {
            this.applyPaging();
        }
    },
    
    multiSort: function (sorters, direction) {
        this.hasMultiSort = true;
        direction = direction || "ASC";

        if (this.multiSortInfo && direction == this.multiSortInfo.direction) {
            direction = direction.toggle("ASC", "DESC");
        }

        this.multiSortInfo = {
            sorters  : sorters,
            direction: direction
        };

        if (!this.remoteSort) {
            this.applySort();
            this.fireEvent('datachanged', this);
        } else {
            var sortInfo   = this.sortInfo || null;
            sorters = sorters || [{}];
            
            if (sorters.length === 1) {
                if (!sorters[0].field) {
                    return;
                }
                this.sortInfo = {field: sorters[0].field, direction: sorters[0].direction};
            } else {
                var field = [],
                    i,
                    j;

                for (i = 0, j = sorters.length; i < j; i++) {
                    if (sorters[i].field) {
                        field.push(sorters[i].field + ":" + (sorters[i].direction || "ASC"));
                    }
                }
                
                this.sortInfo = {field: field.join(","), direction: direction};
            }
            
            this.load(this.lastOptions);

            if (sortInfo) {
                this.sortInfo = sortInfo;
            }              
        }
    },

    addRecord : function (values, commit, clearFilter) {
        var rowIndex = this.data.length,
            record = this.insertRecord(rowIndex, values, false, commit, clearFilter);

        return {
            index: rowIndex,
            record: record
        };
    },

    addSortedRecord : function (values, commit) {
        return this.insertRecord(0, values, true, commit);
    },

    insertRecord : function (rowIndex, values, asSorted, commit, clearFilter) {
        if (clearFilter !== false) {
            this.clearFilter(false);
        }
        values = values || {};

        var f = this.recordType.prototype.fields, 
            dv = {},
            i = 0;

        for (i; i < f.length; i++) {
            dv[f.items[i].name] = f.items[i].defaultValue;

            if (!Ext.isEmpty(values[f.items[i].name])) {
                values[f.items[i].name] = f.items[i].convert(values[f.items[i].name], values);
            }
        }

        var record = new this.recordType(dv, values[this.metaId()]), v;

        record.newRecord = true;        

        record.beginEdit();
        
        for (v in values) {
            record.set(v, values[v]);
        }

        if (!Ext.isEmpty(this.metaId())) {
            record.set(this.metaId(), record.id);
        }

        record.endEdit();
        
        if (this.groupField && !asSorted) {
            this.totalLength = Math.max(1, this.data.length + 1);
            this.add(record);
            this.fireEvent("load", this, record, { add: true });

            this.suspendEvents();
            this.applyGrouping(true);
            this.resumeEvents();
            this.fireEvent("datachanged", this);
        } else {
            if (!asSorted) {
                this.insert(rowIndex, record);
            } else {
                this.addSorted(record);
            }
        }

        if (commit) {
            record.phantom = false;
            record.commit();            
        }
        
        if (!Ext.isDefined(this.writer) && this.modified.indexOf(record) === -1) {
            this.modified.push(record);
        }

        return record;
    },

    addField : function (field, index, clear) {
        if (typeof field === "string") {
            field = { name: field };
        }

        if (Ext.isEmpty(this.recordType)) {
            this.recordType = Ext.data.Record.create([]);
        }

        field = new Ext.data.Field(field);

        if (Ext.isEmpty(index) || index === -1) {
            this.recordType.prototype.fields.replace(field);
        } else {
            this.recordType.prototype.fields.insert(index, field);
        }

        if (typeof field.defaultValue !== "undefined") {
            this.each(function (r) {
                if (typeof r.data[field.name] === "undefined") {
                    r.data[field.name] = field.defaultValue;
                }
            });
        }

        if (clear) {
            this.clearMeta();
        }
    },
    
    clearMeta : function () {
        if (this.reader.ef) {
            delete this.reader.ef;
            this.reader.buildExtractors();
        }
    },

    removeFields : function () {
        if (this.recordType) {
            this.recordType.prototype.fields.clear();
        }

        this.removeAll();
    },

    removeField : function (name) {
        this.recordType.prototype.fields.removeKey(name);

        this.each(function (r) {
            delete r.data[name];

            if (r.modified) {
                delete r.modified[name];
            }
        });
    },

    prepareRecord : function (data, record, options, isNew) {
        var newData = {},
            field;

        if (options.filterRecord && options.filterRecord(record) === false) {
            return;
        }

        if (options.visibleOnly && options.grid) {
            var cm = options.grid.getColumnModel(),
                i;

            for (i in data) {
                var columnIndex = cm.findColumnIndex(i);

                if (columnIndex > -1 && !cm.isHidden(columnIndex)) {
                    newData[i] = data[i];
                }
            }

            data = newData;
        }

        if (options.dirtyRowsOnly && !isNew) {
            if (!record.dirty) {
                return;
            }
        }

        if ((options.dirtyCellsOnly === true || (options.dirtyCellsOnly !== false && this.saveAllFields === false)) && !isNew) {
            var j;

            for (j in data) {
                if (record.isModified(j)) {
                    newData[j] = data[j];
                }
            }

            data = newData;
        }

        var k;

        for (k in data) {
            if (options.filterField && options.filterField(record, k, data[k]) === false) {
                data[k] = undefined;
            }
            
            field = this.getFieldByName(k);
            
            if (Ext.isEmpty(data[k], false) && this.isSimpleField(k, field)) {
                switch (field.submitEmptyValue) {
                case "null":
                    data[k] = null;        
                    break;
                case "emptystring":
                    data[k] = "";        
                    break;
                default:
                    data[k] = undefined;        
                    break;
                }
            }
        }
        
        if (options.mappings !== false && this.saveMappings !== false) {
            var m,
                map = record.fields.map, 
                mappings = {};
            
            Ext.iterate(data, function (prop, value) {            
                m = map[prop];

                if (m) {
                    mappings[m.mapping ? m.mapping : m.name] = value;
                }
            });
 
            if (options.excludeId !== true) {
                mappings[this.metaId()] = record.id; 
            }

            data = mappings;
        }

        return data;
    },
    
    getFieldByName : function (name) {
        var i = 0;

        for (i; i < this.fields.getCount(); i++) {
            var field = this.fields.get(i);

            if (name === (field.mapping || field.name)) {
                return field;
            }
        }        
    },

    isSimpleField: function (name, field) {
        var f = field || this.getFieldByName(name),
            type = f && f.type ? f.type.type : "";

        return type === "int" || type === "float" || type === "boolean" || type === "date";
    },

    getRecordsValues : function (options) {
        options = options || {};

        var records = (options.records ? options.records : (options.currentPageOnly ? this.getRange() : this.getAllRange())) || [],
            values = [],
            i;

        for (i = 0; i < records.length; i++) {
            var obj = {}, dataR;
            
            dataR = Ext.apply(obj, records[i].data);

            if (this.metaId()) {
                obj[this.metaId()] = options.excludeId === true ? undefined : records[i].id;
            }
                        
            dataR = this.prepareRecord(dataR, records[i], options);

            if (!Ext.isEmptyObj(dataR)) {
                values.push(dataR);
            }
        }

        return values;
    },

    refreshIds : function (newRecordsExists, deletedExists, dataAbsent) {
        switch (this.refreshAfterSave) {
        case "None":
            return;
        case "Always":
            if (dataAbsent) {
                this.reload();
            } else {
                this.reload(undefined, true);
            }
            break;
        case "Auto":
            if (newRecordsExists || deletedExists) {
                if (dataAbsent) {
                    this.reload();
                } else {
                    this.reload(undefined, true);
                }
            }
            break;
        }
    },

    reload : function (options, baseReload) {
        if (this.proxy.refreshByUrl && baseReload !== true) {
            var opts = options || {};            
            this.callbackReload(this.warningOnDirty, opts);
        } else {
            if (options && options.params && options.params.submitDirectEventConfig) {
                delete options.params.submitDirectEventConfig;
            }

            Ext.net.Store.superclass.reload.call(this, options);
        }
    },

    load : function (options) {
        var loadData = function (store, options) {
        
            store.on("beforeload", function () {
                this.deleted = [];
                this.modified = [];
            }, store, { single : true });            

            return Ext.net.Store.superclass.load.call(store, options);
        };

        if (this.warningOnDirty && this.isDirty() && !this.silentMode) {
            this.silentMode = false;
            Ext.MessageBox.confirm(
                this.dirtyWarningTitle,
                this.dirtyWarningText,
                function (btn, text) {
                    return (btn === "yes") ? loadData(this, options) : false;
                },
                this
            );
        } else {
            return loadData(this, options);
        }
    },

    save : function (options) {
        if (this.restful) {
            Ext.net.Store.superclass.save.call(this, options);
            return;
        }

        if (Ext.isEmpty(this.updateProxy)) {
            this.callbackSave(options);

            return;
        }

        options = options || {};

        if (this.fireEvent("beforesave", this, options) !== false) {
            var json = this.getChangedData(options);

            if (json.length > 0) {
                var p = Ext.apply(options.params || {}, { data: "{" + json + "}" });
                this.updateProxy.save(p, this.reader, this.recordsSaved, this, options);
            } else {
                this.fireEvent("commitdone", this, options);
            }
        }
    },

    getChangedData : function (options) {
        options = options || {};
        var json = "",
            d = this.deleted,
            m = this.modified;

        if (d.length > 0) {
            json += '"Deleted":[';

            var exists = false,
                i = 0;

            for (i; i < d.length; i++) {
                var obj = {},
                    list = Ext.apply(obj, d[i].data);

                if (this.metaId()) {
                    
                    list[this.metaId()] = d[i].id;
                }

                list = this.prepareRecord(list, d[i], options);

                if (!Ext.isEmptyObj(list)) {
                    json += Ext.util.JSON.encode(list) + ",";
                    exists = true;
                }
            }

            if (exists) {
                json = json.substring(0, json.length - 1) + "]";
            } else {
                json = "";
            }
        }

        var jsonUpdated = "",
            jsonCreated = "",
            j = 0;

        for (j; j < m.length; j++) {

            var obj2 = {},
                list2 = Ext.apply(obj2, m[j].data);

            if (this.metaId()) {
                
                list2[this.metaId()] = m[j].id;
            }

            list2 = this.prepareRecord(list2, m[j], options, m[j].isNew());
            
            if (m[j].isNew() && this.skipIdForNewRecords !== false && !this.useIdConfirmation) {
                list2[this.metaId()] = undefined;
            }

            if (!Ext.isEmptyObj(list2)) {
                if (m[j].isNew()) {
                    jsonCreated += Ext.util.JSON.encode(list2) + ",";
                } else {
                    jsonUpdated += Ext.util.JSON.encode(list2) + ",";
                }
            }

        }

        if (jsonUpdated.length > 0) {
            jsonUpdated = jsonUpdated.substring(0, jsonUpdated.length - 1) + "]";
        }

        if (jsonCreated.length > 0) {
            jsonCreated = jsonCreated.substring(0, jsonCreated.length - 1) + "]";
        }

        if (jsonUpdated.length > 0) {
            if (json.length > 0) {
                json += ",";
            }

            json += '"Updated":[';
            json += jsonUpdated;
        }

        if (jsonCreated.length > 0) {
            if (json.length > 0) {
                json += ",";
            }

            json += '"Created":[';
            json += jsonCreated;
        }

        return options.encode ? Ext.util.Format.htmlEncode(json) : json;
    },

    getByDataId : function (id) {
        if (!this.metaId()) {
            return undefined;
        }

        var m = this.modified, 
            i;

        for (i = 0; i < m.length; i++) {
            if (m[i].data[this.metaId()] === id) {
                return m[i];
            }
        }

        return undefined;
    },

    recordsSaved : function (o, options, success) {
        if (!o || success === false) {
            if (success !== false) {
                this.fireEvent("save", this, options);
            }

            if (options.callback) {
                options.callback.call(options.scope || this, options, false);
            }

            if (this.autoSave && success === false) {
                this.rejectDeleting();
            }

            return;
        }

        var serverSuccess = o.success,
            msg = o.msg;
            
        this.responseSaveData = o.data || null;

        this.fireEvent("save", this, options, { message: msg });

        if (options.callback) {
            options.callback.call(options.scope || this, options, true);
        }

        var serviceResult = o.data || {},
            newRecordsExists = false,
            deletedExists = this.deleted.length > 0,
            m = this.modified,
            j;

        for (j = 0; j < m.length; j++) {
            if (m[j].isNew()) {
                newRecordsExists = true;
                break;
            }
        }

        if (this.useIdConfirmation) {
            if (Ext.isEmpty(serviceResult.confirm) && serverSuccess) {
                msg = "The confirmation list is absent";
                this.fireEvent("commitfailed", this, msg);
                this.fireEvent("exception", this, "remote", "write", {}, {}, { message: msg });

                if (this.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure({ status: "", statusText: "" }, msg);
                }

                return;
            }

            if (!Ext.isEmpty(serviceResult.confirm)) {
                var r = serviceResult.confirm,
                    failCount = 0,
                    i = 0;

                for (i; i < r.length; i++) {
                    if (r[i].s === false) {
                        failCount++;
                    } else {
                        var record = this.getById(r[i].oldId) || this.getByDataId(r[i].oldId);

                        if (record) {                            
                            if (record.isNew()) {
                                this.updateRecordId(record, r[i].newId || r[i].oldId);
                            }
                            record.phantom = false;
                            record.commit();                            
                        } else {
                            var d = this.deleted,
                                i2 = 0;

                            for (i2; i2 < d.length; i2++) {
                                //do not replace == by ===
                                if (this.metaId() && d[i2].id == r[i].oldId) {
                                    this.deleted.splice(i2, 1);
                                    failCount--;
                                    break;
                                }
                            }
                            failCount++;
                        }
                    }
                }

                if (failCount > 0 && serverSuccess) {
                    msg = "Some records have no success confirmation!";
                    this.fireEvent("commitfailed", this, msg);
                    this.fireEvent("exception", this, "remote", "write", {}, {}, { message: msg });

                    if (this.showWarningOnFailure) {
                        Ext.net.DirectEvent.showFailure({ status: "", statusText: "" }, msg);
                    }

                    return;
                }

                if (failCount === 0 && serverSuccess) {
                    this.modified = [];
                    this.deleted = [];
                }
            }
        } else if (serverSuccess) {
            this.commitChanges();
        }

        if (!serverSuccess) {
            this.fireEvent("commitfailed", this, msg);
            this.fireEvent("exception", this, "remote", "write", {}, {}, { message: msg });

            if (this.showWarningOnFailure) {
                Ext.net.DirectEvent.showFailure({ status: "", statusText: "" }, msg);
            }

            if (this.autoSave) {
                this.rejectDeleting();
            }

            return;
        }

        this.fireEvent("commitdone", this, options);

        var dataAbsent = true;

        if (serviceResult.data && serviceResult.data !== null && this.proxy.refreshData) {
            dataAbsent = false;
            this.proxy.refreshData(serviceResult.data);

            if (this.isPagingStore()) {
                this.loadData(serviceResult.data);
            }
        }

        this.refreshIds(newRecordsExists, deletedExists, dataAbsent);
    },

    isPagingStore : function () {
        return !!(this.isPaging && this.applyPaging && this.openPage && this.findPage);
    },

    getDeletedRecords : function () {
        return this.deleted;
    },

    remove : function (record) {
        if (Ext.isArray(record)) {
            Ext.each(record, function (r) {
                this.remove(r);
            }, this);
        } 
        
        if (!record.isNew()) {
            record.lastIndex = this.indexOf(record);
            this.deleted.push(record);
        }

        Ext.net.Store.superclass.remove.call(this, record);
    },

    commitChanges : function () {
        var i,
            length;

        for (i = 0, length = this.modified.length; i < length; i++) {
            this.modified[i].phantom = false;
        }
        
        Ext.net.Store.superclass.commitChanges.call(this);

        this.deleted = [];
    },

    rejectChanges : function () {
        Ext.net.Store.superclass.rejectChanges.call(this);

        var d = this.deleted.slice(0),
            i,
            len;

        this.deleted = [];

        for (i = 0, len = d.length; i < len; i++) {
            this.insert(d[i].lastIndex || 0, d[i]);
            d[i].reject();
        }
    },

    isDirty : function () {
        return (this.deleted.length > 0 || this.modified.length > 0) ? true : false;
    },

    prepareCallback : function (context, options) {
        options = options || {};
        options.params = options.params || {};

        if (context.fireEvent("beforesave", context, options) !== false) {
            var json = context.getChangedData(options);

            if (json.length > 0) {
                var p = { data: "{" + json + "}", extraParams: options.params };
                return p;
            } else {
                context.fireEvent("commitdone", context, options);
            }
        }
        
        return null;
    },

    callbackHandler : function (response, result, context, type, action, extraParams, o) {
        try {
            var responseObj = result.serviceResponse;

            result = { success: responseObj.success, msg: responseObj.message, data: responseObj.data };
        } catch (e) {
            context.fireEvent("saveexception", context, o, response, e);
            context.fireEvent("exception", context, "remote", "write", o, response, e);

            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", response, { "errorMessage": e.message }, null, null, null, null, o) !== false) {
                if (context.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure(response, e.message);
                }
            }

            return;
        }
        context.recordsSaved(result, {}, true);
    },

    silentMode : false,

    callbackRefreshHandler : function (response, result, context, type, action, extraParams, o) {
        var p = context.proxy;

        try {
            var responseObj = result.serviceResponse;
            result = { success: responseObj.success, msg: responseObj.message || null, data: responseObj.data || {} };
        } catch (e) {
            context.fireEvent("loadexception", context, o, response, e);
            context.fireEvent("exception", context, "remote", "read", o, response, e);

            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", response, { "errorMessage": e.message }, null, null, null, null, o) !== false) {
                if (context.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure(response, e.message);
                }
            }

            if (o && o.userCallback) {
                o.userCallback.call(o.userScope || this, [], o, false);
            }

            return;
        }

        if (result.success === false) {
            context.fireEvent("loadexception", context, o, response, { message: result.msg });
            context.fireEvent("exception", context, "remote", "read", o, response, { message: result.msg });

            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", response, { "errorMessage": result.msg }, null, null, null, null, o) !== false) {
                if (context.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure(response, result.msg);
                }
            }

            if (o && o.userCallback) {
                o.userCallback.call(o.userScope || this, [], o, false);
            }

            return;
        }

        if (p.refreshData) {
            if (result.data.data && result.data.data !== null) {
                p.refreshData(result.data.data);

                if (context.isPagingStore()) {
                    context.loadData(result.data.data);
                }
            } else {
                p.refreshData({});

                if (context.isPagingStore()) {
                    context.loadData({});
                }
            }
        }

        if (o && o.userCallback) {
            o.callback = o.userCallback;
            o.userCallback = undefined;
            o.scope = o.userScope;
            o.userScope = undefined;
        }

        if (!context.isPagingStore()) {
            context.silentMode = true;
            context.reload(o, true);
            context.silentMode = false;
        } else {
            if (o && o.callback) {
                o.callback.call(o.scope || this, [], o, true);
            }
        }
    },

    callbackErrorHandler : function (response, result, context, type, action, extraParams, o) {
        context.fireEvent("saveexception", context, o, response, { message: result.errorMessage || response.statusText });
        context.fireEvent("exception", context, "response", "write", o, response, { message: result.errorMessage || response.statusText });
        
        if (o.showWarningOnFailure !== false && o.cancelFailureWarning !== true && Ext.isEmpty(result, false)) {
            Ext.net.DirectEvent.showFailure(response, response.responseText);
        }

        if (context.autoSave) {
            context.rejectDeleting();
        }
    },

    callbackRefreshErrorHandler : function (response, result, context, type, action, extraParams, o) {
        context.fireEvent("loadexception", context, o, response, { message: result.errorMessage || response.statusText });
        context.fireEvent("exception", context, "response", "read", o, response, { message: result.errorMessage || response.statusText });
        
        if (o && o.userCallback) {
            o.userCallback.call(o.userScope || this, [], o, false);
        }

        if (o.showWarningOnFailure !== false && o.cancelFailureWarning !== true && Ext.isEmpty(result, false)) {
            Ext.net.DirectEvent.showFailure(response, response.responseText);
        }
    },

    callbackSave: function (options) {
        var requestObject = this.prepareCallback(this, options);

        if (requestObject !== null) {
            var config = {},
                ac = this.directEventConfig;

            ac.userSuccess = this.callbackHandler;
            ac.userFailure = this.callbackErrorHandler;
            ac.extraParams = requestObject.extraParams;
            ac.enforceFailureWarning = !this.hasListener("saveexception") && !this.hasListener("exception");

            Ext.apply(config, ac, {
                control: this,
                eventType: "postback",
                action: "update",
                serviceParams: requestObject.data
            });
            Ext.net.DirectEvent.request(config);
        }
    },

    submitData : function (data, options) {
        if (Ext.isEmpty(data)) {
            data = this.getRecordsValues(options);
        }
        
        if (!data || data.length === 0) {
            return false;
        } 

        data = Ext.encode(data);

        if (options && options.encode) {
            data = Ext.util.Format.htmlEncode(data);
        }

        if (Ext.isEmpty(this.updateProxy)) {
            options = { params: (options && options.params) ? options.params : {} };

            if (this.fireEvent("beforesave", this, options) !== false) {

                var config = {}, ac = this.directEventConfig;

                ac.userSuccess = this.submitSuccess;
                ac.userFailure = this.submitFailure;
                ac.extraParams = options.params;
                ac.enforceFailureWarning = !this.hasListener("saveexception") && !this.hasListener("exception");

                Ext.apply(config, ac, {
                    control: this,
                    eventType: "postback",
                    action: "submit",
                    serviceParams: data
                });

                Ext.net.DirectEvent.request(config);
            }
        } else {
            options = { params: (options && options.params) ? options.params : {} };

            if (this.fireEvent("beforesave", this, options) !== false) {
                var p = Ext.apply(options.params || {}, { data: data });
                this.updateProxy.save(p, this.reader, this.finishSubmit, this, options);
            }
        }
    },

    finishSubmit : function (o, options, success) {
        if (!o || success === false) {

            if (success !== false) {
                this.fireEvent("save", this, options);
            }

            return;
        }

        var serverSuccess = o.success,
            msg = o.msg;

        if (!serverSuccess) {
            this.fireEvent("saveexception", this, options, {}, { message: msg });
            this.fireEvent("exception", this, "remote", "write", options, {}, { message: msg });

            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", {}, { "errorMessage": msg }, null, null, null, null, o) !== false) {
                if (this.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure({ status: 200, statusText: "OK" }, msg);
                }
            }

            return;
        }

        this.fireEvent("save", this, options, { message: msg });
    },

    submitFailure : function (response, result, context, type, action, extraParams, o) {
        context.fireEvent("saveexception", context, {}, response, { message: result.errorMessage || response.statusText });
        context.fireEvent("exception", context, "response", "write", o, response, { message: result.errorMessage || response.statusText });

        if (o.showWarningOnFailure !== false && o.cancelFailureWarning !== true && Ext.isEmpty(result, false)) {
            Ext.net.DirectEvent.showFailure(response, response.responseText);
        }
    },

    submitSuccess: function (response, result, context, type, action, extraParams, o) {
        try {
            var responseObj = result.serviceResponse;
            result = { success: responseObj.success, msg: responseObj.message };
        } catch (e) {
            context.fireEvent("saveexception", context, {}, response, e);
            context.fireEvent("exception", context, "remote", "write", o, response, e);

            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", {}, { "errorMessage": e.message }, null, null, null, null, o) !== false) {
                if (context.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure(response, e.message);
                }
            }

            return;
        }

        if (!result.success) {
            context.fireEvent("saveexception", context, {}, response, { message: result.msg });
            context.fireEvent("exception", context, "remote", "write", o, response, { message: result.msg });

            if (Ext.net.DirectEvent.fireEvent("ajaxrequestexception", {}, { "errorMessage": result.msg }, null, null, null, null, o) !== false) {
                if (context.showWarningOnFailure) {
                    Ext.net.DirectEvent.showFailure(response, result.msg);
                }
            }

            return;
        }

        context.fireEvent("save", context, { message: result.msg });
    },

    callbackReload : function (dirtyConfirm, reloadOptions) {
        var options = Ext.applyIf(reloadOptions || {}, this.lastOptions);

        options.params = options.params || {};

        var reload = function (store, options) {
            if (store.fireEvent("beforeload", store, options) !== false) {
                store.storeOptions(options);
				store.deleted = [];
                store.modified = [];

                var config = {},
                    ac = store.directEventConfig;

                ac.userSuccess = store.callbackRefreshHandler;
                ac.userFailure = store.callbackRefreshErrorHandler;
                ac.extraParams = options.params;
                ac.enforceFailureWarning = !store.hasListener("loadexception") && !store.hasListener("exception");
                config.userCallback = options.callback;
                config.userScope = options.scope;

                Ext.apply(config, ac, { control: store, eventType: "postback", action: "refresh" });
                Ext.net.DirectEvent.request(config);
            }
        };

        if (dirtyConfirm && this.isDirty()) {
            Ext.MessageBox.confirm(
                this.dirtyWarningTitle,
                this.dirtyWarningText, 
                function (btn, text) {
                    if (btn === "yes") {
                        reload(this, options);
                    }
                }, 
                this
            );
        } else {
            reload(this, options);
        }
    },

    getAllRange : function (start, end) {
        return this.getRange(start, end);
    },

    updateRecordId : function (id, newId, silent) {
        var record = (id instanceof Ext.data.Record) ? id : this.getById(id);

        if (Ext.isEmpty(record)) {
            throw new Ext.data.Store.Error("Record with id='" + id + "' not found");
        }

        record._phid = record.id;

        record.id = newId;

        if (this.metaId()) {
            record.data[this.metaId()] = record.id;
        }

        this.reMap(record);

        if (silent === false) {
            this.fireEvent("update", this, record, Ext.data.Record.EDIT);
        }
    },

    removeFromBatch : function (batch, action, data) {
        var b = this.batches,
            key = this.batchKey + batch,
            o = b[key],
            arr;

        if (o) {
            arr = o.data[action] || [];
            o.data[action] = arr.concat(data);
            
            if (o.count === 1) {
                data = o.data;
                delete b[key];
                //this.fireEvent('save', this, batch, data);
            } else {
                --o.count;
            }
        }
        this.deleted = [];
    },

    rejectDeleting : function () {
        var d = this.deleted.slice(0),
            i = d.length - 1;

        this.deleted = [];
        
        for (i; i >= 0; i--) {
            this.insert(d[i].lastIndex || 0, d[i]);
            d[i].reject();
        }
    }
});

// @source data/PagingStore.js

Ext.ns("Ext.ux.data");

Ext.ux.data.PagingStore = Ext.extend(Ext.net.Store, {
    reMap : function(record) {
        if (Ext.isArray(record)) {
            for (var i = 0, len = record.length; i < len; i++) {
                this.reMap(record[i]);
            }
        } else {
            delete this.data.map[record._phid];
            this.data.map[record.id] = record;
            var index = this.data.keys.indexOf(record._phid);
            this.data.keys.splice(index, 1, record.id);
            
            if (this.allData) {
                delete this.allData.map[record._phid];
                this.allData.map[record.id] = record;
                index = this.allData.keys.indexOf(record._phid);
                this.allData.keys.splice(index, 1, record.id);
            }           
            
            delete record._phid;
        }
    },
    
    destroy : function () {
        if (window[this.storeId || this.id]) {
            window[this.storeId || this.id] = null;
        }
        
        if (window[this.storeId + "_Data" || this.id + "_Data"]) {
            window[this.storeId + "_Data" || this.id + "_Data"] = null;
        }
        
        if (this.storeId) {
            Ext.StoreMgr.unregister(this);
        }
        
        this.data = this.allData = this.snapshot = null;
        Ext.destroy(this.proxy);
        this.reader = this.writer = null;
        this.purgeListeners();
    },
    
    add : function (records) {
        var i, record, index;
        
        records = [].concat(records);
        if (records.length < 1) {
            return;
        }
        
        for (i = 0, len = records.length; i < len; i++) {
            record = records[i];
            
            record.join(this);
            
            if (record.dirty || record.phantom) {
                this.modified.push(record);
            }
        }
        
        index = this.data.length;
        this.data.addAll(records);
        
        if (this.allData) {
            this.allData.addAll(records);
        }
        
        if (this.snapshot) {
            this.snapshot.addAll(records);
        }
        
        // *** add ***
        this.totalLength += records.length;
        // *** end ***
        this.fireEvent("add", this, records, index);
    },
    
    remove : function (record) {
        if (Ext.isArray(record)) {
            Ext.each(record, function (r) {
                this.remove(r);
            }, this);
            return;
        }
        // *** add ***
        if (this != record.store) {
            return;
        }
        record.join(null);
        // *** end ***
        var index = this.data.indexOf(record);
    
        if (index > -1) {
            this.data.removeAt(index);
        }
        if (this.pruneModifiedRecords) {
            this.modified.remove(record);
        }
        // *** add ***
        if (this.allData) {
            this.allData.remove(record);
        }
        // *** end ***
        if (this.snapshot) {
            this.snapshot.remove(record);
        }
    
         // *** add ***
        this.totalLength--;
        // *** end ***
        
        if (!record.isNew()) {
            record.lastIndex = index;
            this.deleted.push(record);
        }
    
        if (index > -1) {
            this.fireEvent("remove", this, record, index);
        }
    },
    removeAll: function (silent) {
        // *** add ***
        var items = [].concat((this.snapshot || this.allData || this.data).items);
        // *** end ***
        // var items = [];
        // this.each(function (rec) {
        //     items.push(rec);
        // });
        
        this.clearData();
        
        // if (this.snapshot) {
        //     this.snapshot.clear();
        // }
        
        if (this.pruneModifiedRecords) {
            this.modified = [];
        }
        
        // *** add ***
        this.totalLength = 0;
        // *** end ***
        
        if (silent !== true) {
            this.fireEvent("clear", this, items);
        }
    },
    
    insert : function (index, records) {
        var i, record;
        
        records = [].concat(records);
        for (i = 0, len = records.length; i < len; i++) {
            record = records[i];
            
            this.data.insert(index + i, record);
            record.join(this);
            
            if (record.dirty || record.phantom) {
                this.modified.push(record);
            }
        }
        
        if (this.allData) {
            this.allData.addAll(records);
        }
        
        if (this.snapshot) {
            this.snapshot.addAll(records);
        }
        
        // *** add ***
        this.totalLength += records.length;
        // *** end ***
        this.fireEvent("add", this, records, index);
    },
    
    getById : function (id) {
        return (this.snapshot || this.allData || this.data).key(id);
    },
    clearData: function () {
        // *** add ***
        if (this.allData) {
            this.data = this.allData;
            delete this.allData;
        }
        
        if (this.snapshot) {
            this.data = this.snapshot;
            delete this.snapshot;
        }
        
        // *** end ***
        
        this.data.each(function (rec) {
            rec.join(null);
        });
        
        this.data.clear();
    },
    
    load : function (options) {
        if (options && options.params && (options.params.start > (this.snapshot || this.allData || this.data).getCount())) {
            options.params.start = start = this.start = 0;
        }
        
        return Ext.net.Store.superclass.load.call(this, options);
    },
    execute: function (action, rs, options, batch) {
        if (!Ext.data.Api.isAction(action)) {
            throw new Ext.data.Api.Error("execute", action);
        }
        options = Ext.applyIf(options || {}, {
            params: {}
        });
        if (batch !== undefined) {
            this.addToBatch(batch);
        }
        var doRequest = true;
        
        if (action === "read") {
            doRequest = this.fireEvent("beforeload", this, options);
            Ext.applyIf(options.params, this.baseParams);
        } else {
            if (this.writer.listful === true && this.restful !== true) {
                rs = (Ext.isArray(rs)) ? rs : [rs];
            } else if (Ext.isArray(rs) && rs.length === 1) {
                rs = rs.shift();
            }
            
            if ((doRequest = this.fireEvent("beforewrite", this, action, rs, options)) !== false) {
                this.writer.apply(options.params, this.baseParams, action, rs);
            }
        }
        
        if (doRequest !== false) {
            //var params = Ext.apply(options.params || {}, this.baseParams);
            var params = Ext.apply({}, options.params, this.baseParams);
            
            if (this.writer && this.proxy.url && !this.proxy.restful && !Ext.data.Api.hasUniqueUrl(this.proxy, action)) {
                params.xaction = action;
            }
            
            if (action === "read" && this.isPaging(params)) {
                (function () {
                    if (this.allData) {
                        this.data = this.allData;
                        delete this.allData;
                    }
                    
                    this.applyPaging();
                    this.fireEvent("datachanged", this);
                    var r = [].concat(this.data.items);
                    this.fireEvent("load", this, r, options);
                    
                    if (options.callback) {
                        options.callback.call(options.scope || this, r, options, true);
                    }
                }).defer(1, this);
                
                return true;
            }
            
            this.proxy.request(Ext.data.Api.actions[action], rs, params, this.reader, this.createCallback(action, rs), this, options);
        }
        
        return doRequest;
    },
    
    loadRecords : function (o, options, success) {
        if (this.isDestroyed === true) {
            return;
        }
        if (!o || success === false) {
            if (success !== false) {
                this.fireEvent("load", this, [], options);
            }
            
            if (options.callback) {
                options.callback.call(options.scope || this, [], options, false, o);
            }
            
            return;
        }
        var r = o.records,
            t = o.totalRecords || r.length;
        if (!options || options.add !== true) {
            if (this.pruneModifiedRecords) {
                this.modified = [];
            }

            var i = 0;
            
            for (i, len = r.length; i < len; i++) {
                r[i].join(this);
            }
            
            //if (this.snapshot) {
            //    this.data = this.snapshot;
            //    delete this.snapshot;
            //}
            
            this.clearData();
            this.data.addAll(r);
            this.totalLength = t;
            this.applySort();
            
            if (!this.allData) {
                this.applyPaging();
            }
            
            if (r.length > this.getCount()) {
                r = [].concat(this.data.items);
            }
            
            this.fireEvent("datachanged", this);
        } else {
            this.totalLength = Math.max(t, this.data.length + r.length);
            this.add(r);
        }
    
        this.fireEvent("load", this, r, options);
    
        if (options.callback) {
            options.callback.call(options.scope || this, r, options, true);
        }
    },
    
    loadData : function (o, append) {
        this.isPaging(Ext.apply({}, this.lastOptions ? this.lastOptions.params : null, this.baseParams));
        var r = this.reader.readRecords(o);
        this.loadRecords(r, Ext.apply({add: append}, this.lastOptions || {}), true);
    },
    getTotalCount : function () {
        // *** add ***
        if (this.allData) {
            return this.allData.getCount();
        }
        // *** end ***
        return this.totalLength || 0;
    },
    
    filterBy : function (fn, scope) {
        this.snapshot = this.snapshot || this.allData || this.data;
        this.data = this.queryBy(fn, scope || this);
        this.applyPaging();
        this.fireEvent("datachanged", this);
    },
    clearFilter : function (suppressEvent) {
        if (this.isFiltered()) {
            this.data = this.snapshot;
            delete this.snapshot;
            // *** add ***
            delete this.allData;
            this.applyPaging();
            // *** end ***
            if (suppressEvent !== true) {
                this.fireEvent("datachanged", this);
            }
        }
    },
    isFiltered : function () {
        // *** add ***
        return !!this.snapshot && this.snapshot != (this.allData || this.data);
        // *** end ***
        // return !!this.snapshot && this.snapshot != this.data;
    },
    queryBy : function (fn, scope) {
        // *** add ***
        var data = this.snapshot || this.allData || this.data;
        return data.filterBy(fn, scope || this);
    },
    
    collect : function (dataIndex, allowNull, bypassFilter) {
        var d = (bypassFilter === true ? this.snapshot || this.allData || this.data : this.data).items,
            v, 
            sv, 
            r = [], 
            l = {},
            i = 0,
            len;
        
        for (i, len = d.length; i < len; i++) {
            v = d[i].data[dataIndex];
            sv = String(v);
        
            if ((allowNull || !Ext.isEmpty(v)) && !l[sv]) {
                l[sv] = true;
                r[r.length] = v;
            }
        }
        
        return r;
    },
    findInsertIndex : function (record) {
        this.suspendEvents();
        var data = this.data.clone();
        this.data.add(record);
        this.applySort();
        var index = this.data.indexOf(record);
        this.data = data;
        // *** add ***
        this.totalLength--;
        // *** end ***
        this.resumeEvents();
        return index;
    },
    // *** add ***
    isPaging: function (params) {
        var pn = this.paramNames,
            start = params[pn.start],
            limit = params[pn.limit];
            
        if ((typeof start !== "number") || (typeof limit !== "number")) {
            delete this.start;
            delete this.limit;
            this.lastParams = params;

            return false;
        }
        
        this.start = start;
        this.limit = limit;
        delete params[pn.start];
        delete params[pn.limit];
        var lastParams = this.lastParams;
        this.lastParams = params;
        
        if (!this.proxy) {
            return true;
        }
        
        if (!lastParams) {
            return false;
        }
        
        var param;

        for (param in params) {
            if (params.hasOwnProperty(param) && (params[param] !== lastParams[param])) {
                return false;
            }
        }
        
        for (param in lastParams) {
            if (lastParams.hasOwnProperty(param) && (params[param] !== lastParams[param])) {
                return false;
            }
        }
        
        return true;
    },
    
    applyPaging : function () {
        var start = this.start, limit = this.limit;
        
        if ((typeof start === "number") && (typeof limit === "number")) {
            var allData = this.data, data = new Ext.util.MixedCollection(allData.allowFunctions, allData.getKey);
            
            if (start > allData.getCount()) {
                start = this.start = 0;
            }
            
            data.items = allData.items.slice(start, start + limit);
            data.keys = allData.keys.slice(start, start + limit);

            var len = data.length = data.items.length,
                map = {},
                i = 0;
            
            for (i; i < len; i++) {
                var item = data.items[i];
                map[data.getKey(item)] = item;
            }
            
            data.map = map;
            this.allData = allData;
            this.data = data;
        }
    },
    
    getAllRange : function (start, end) {
        return (this.snapshot || this.allData || this.data).getRange(start, end);
    },

    findPage : function (record) {
        if ((typeof this.limit === "number")) {
            return Math.ceil((this.allData || this.data).indexOf(record) / this.limit);
        }

        return -1;
    },

    openPage : function (pageIndex, callback) {
        if ((typeof pageIndex !== "number")) {
            pageIndex = this.findPage(pageIndex);
        }

        this.load({ 
            params : {
                start : (pageIndex - 1) * this.limit, 
                limit : this.limit
            }, 
            callback : callback 
        });
    }
});

Ext.ux.PagingToolbar = Ext.extend(Ext.PagingToolbar, {
    onLoad : function (store, r, o) {
        if (!this.rendered) {
            this.dsLoaded = [store, r, o];
            return;
        }
        
        var p = this.getParams();
        this.cursor = (o.params && o.params[p.start]) ? o.params[p.start] : 0;
        this.onChange();
    },
    
    onChange : function () {
        // *** add ***
        var t = this.store.getTotalCount(),
            s = this.pageSize;

        if (t === 0) {
            this.cursor = 0;
        } else if (this.cursor >= t) {
            this.cursor = (Math.ceil(t / s) - 1) * s;
        }
        // *** end ***

        var d = this.getPageData(),
            ap = d.activePage,
            ps = d.pages;

        // *** add ***    
        ap = ap > ps ? ps : ap;
        // *** end ***

        this.afterTextItem.setText(String.format(this.afterPageText, d.pages));
        this.inputItem.setValue(ap);
        this.first.setDisabled(ap === 1);
        this.prev.setDisabled(ap === 1);
        this.next.setDisabled(ap === ps);
        this.last.setDisabled(ap === ps);
        this.refresh.enable();
        this.updateInfo();
        this.fireEvent("change", this, d);
    },
    
    onClear : function () {
        this.cursor = 0;
        this.onChange();
    },
    
    doRefresh : function () {
        // *** add ***
        delete this.store.lastParams;
        // *** end ***
        this.doLoad(this.cursor);
    },
    
    bindStore : function (store, initial) {
        var doLoad;
        if (!initial && this.store) {
            if (store !== this.store && this.store.autoDestroy) {
                this.store.destroy();
            } else {
                this.store.un("beforeload", this.beforeLoad, this);
                this.store.un("load", this.onLoad, this);
                this.store.un("exception", this.onLoadError, this);
                // *** add ***
                this.store.un("datachanged", this.onChange, this);
                this.store.un("add", this.onChange, this);
                this.store.un("remove", this.onChange, this);
                this.store.un("clear", this.onClear, this);
                // *** end ***
            }
            
            if (!store) {
                this.store = null;
            }
        }
        if (store) {
            store = Ext.StoreMgr.lookup(store);
            store.on({
                scope       : this,
                beforeload  : this.beforeLoad,
                load        : this.onLoad,
                exception   : this.onLoadError,
                // *** add ***
                datachanged : this.onChange,
                add         : this.onChange,
                remove      : this.onChange,
                clear       : this.onClear
                // *** end ***
            });
            doLoad = true;
        }
        
        this.store = store;
        
        if (doLoad) {
            this.onLoad(store, null, {});
        }
    }
});

Ext.reg("ux.paging", Ext.ux.PagingToolbar);

// @source data/SaveMask.js

Ext.net.SaveMask = function (el, config) {
    this.el = Ext.get(el);
    
    Ext.apply(this, config);
    
    if (this.writeStore) {
        this.writeStore.on("beforesave", this.onBeforeSave, this);
        this.writeStore.on("save", this.onSave, this);
        this.writeStore.on("saveexception", this.onSave, this);
        this.writeStore.on("commitdone", this.onSave, this);
        this.writeStore.on("commitfailed", this.onSave, this);
        this.removeMask = Ext.value(this.removeMask, false);
    }
};

Ext.net.SaveMask.prototype = {
    msg      : "Saving...",
    msgCls   : "x-mask-loading",
    disabled : false,
    
    disable  : function () {
        this.disabled = true;
    },
    
    enable : function () {
        this.disabled = false;
    },

    onSave : function () {
        this.el.unmask(this.removeMask);
    },

    onBeforeSave : function () {
        if (!this.disabled) {
            this.el.mask(this.msg, this.msgCls);
        }
    },

    show : function () {
        this.onBeforeSave();
    },

    hide : function () {
        this.onSave();    
    },

    destroy : function () {
        if (this.writeStore) {
            this.writeStore.un("beforesave", this.onBeforeSave, this);
            this.writeStore.un("save", this.onSave, this);
            this.writeStore.un("saveexception", this.onSave, this);
            this.writeStore.un("commitdone", this.onSave, this);
            this.writeStore.un("commitfailed", this.onSave, this);
        }
    }
};

// @source data/RowSelectionModel.js

Ext.grid.CellSelectionModel.prototype.handleMouseDown = Ext.grid.CellSelectionModel.prototype.handleMouseDown.createInterceptor(function (g, row, cell, e) {
    if (this.ignoreTargets) {
        var i = 0;

        for (i; i < this.ignoreTargets.length; i++) {
            if (e.getTarget(this.ignoreTargets[i])) {
                return false;
            }
        }
    }
});

Ext.grid.RowSelectionModel.prototype.handleMouseDown = Ext.grid.RowSelectionModel.prototype.handleMouseDown.createInterceptor(function (g, rowIndex, e) {
    if (e.button !== 0 || this.isLocked()) {
        return;
    }
    
    if (this.ignoreTargets) {
        var i = 0;

        for (i; i < this.ignoreTargets.length; i++) {
            if (e.getTarget(this.ignoreTargets[i])) {
                return false;
            }
        }
    }
});

Ext.grid.RowSelectionModel.override({
    selectById : function (ids, keepExisting) {
        if (!keepExisting) {
            this.clearSelections();
        }
        
        if (!Ext.isArray(ids)) {
            ids = [ids];
        }
        
        var ds = this.grid.store,
            i,
            len;
        
        for (i = 0, len = ids.length; i < len; i++) {
            this.selectRow(ds.indexOfId(ids[i]), true);
        }
    }
});

// @source data/GridPanel.js

Ext.net.GridPanel = function (config) {
    this.selectedIds = {};
    this.memoryIDField = "id";

    //Ext.apply(this, config);
    this.addEvents("editcompleted", "command", "groupcommand");
    Ext.net.GridPanel.superclass.constructor.call(this, config);
    this.initSelection();    
};

Ext.extend(Ext.net.GridPanel, Ext.grid.EditorGridPanel, {
    clearEditorFilter     : true,
    selectionSavingBuffer : 0,

    getFilterPlugin : function () {
        if (this.plugins && Ext.isArray(this.plugins)) {
            var i = 0;

            for (i; i < this.plugins.length; i++) {
                if (this.plugins[i].isGridFiltersPlugin) {
                    return this.plugins[i];
                }
            }
        } else {
            if (this.plugins && this.plugins.isGridFiltersPlugin) {
                return this.plugins;
            }
        }
    },

    getRowEditor : function () {
        if (this.plugins && Ext.isArray(this.plugins)) {
            var i = 0;

            for (i; i < this.plugins.length; i++) {
                if (this.plugins[i].isRowEditor) {
                    return this.plugins[i];
                }
            }
        } else {
            if (this.plugins && this.plugins.isRowEditor) {
                return this.plugins;
            }
        }
    },

    getRowExpander : function () {
        if (this.plugins && Ext.isArray(this.plugins)) {
            var i = 0;

            for (i; i < this.plugins.length; i++) {
                if (this.plugins[i].id === "expander") {
                    return this.plugins[i];
                }
            }
        } else {
            if (this.plugins && this.plugins.id === "expander") {
                return this.plugins;
            }
        }
    },

    doSelection : function () {
        var data = this.selModel.selectedData,
            silent = true;

        if (!Ext.isEmpty(this.fireSelectOnLoad)) {
            silent = !this.fireSelectOnLoad;
        }

        if (!Ext.isEmpty(data)) {
            if (silent) {
                this.suspendEvents();
                this.selModel.suspendEvents();
            }

            if (this.selModel.select) {
                if (!Ext.isEmpty(data.recordID) && !Ext.isEmpty(data.name)) {
                    var rowIndex = this.store.indexOfId(data.recordID),
                        colIndex = this.getColumnModel().findColumnIndex(data.name);

                    if (rowIndex > -1 && colIndex > -1) {
                        this.selModel.select(rowIndex, colIndex);
                    }
                } else if (!Ext.isEmpty(data.rowIndex) && !Ext.isEmpty(data.colIndex)) {
                    this.selModel.select(data.rowIndex, data.colIndex);
                }

                if (silent) {
                    this.updateCellSelection();
                }
            } else if (this.selModel.selectRow && data.length > 0) {
                var records = [],
                    record,
                    i = 0;

                for (i; i < data.length; i++) {
                    if (!Ext.isEmpty(data[i].recordID)) {
                        record = this.store.getById(data[i].recordID);

                        if (this.selectionMemory) {
                            var idx = data[i].rowIndex || -1;

                            if (!Ext.isEmpty(record)) {
                                idx = this.store.indexOfId(record.id);
                                idx = this.getAbsoluteIndex(idx);
                            }

                            this.onMemorySelectId(null, idx, data[i].recordID);
                        }
                    } else if (!Ext.isEmpty(data[i].rowIndex)) {
                        record = this.store.getAt(data[i].rowIndex);

                        if (this.selectionMemory && !Ext.isEmpty(record)) {
                            this.onMemorySelectId(null, data[i].rowIndex, record.id);
                        }
                    }

                    if (!Ext.isEmpty(record)) {
                        records.push(record);
                    }
                }
                this.selModel.selectRecords(records);

                if (silent) {
                    this.updateSelectedRows();
                }
            }

            if (silent) {
                this.resumeEvents();
                this.selModel.resumeEvents();
            }
        }
    },

    updateSelectedRows : function () {
        var records = [],
            id;

        if (this.selectionMemory) {
            for (id in this.selectedIds) {
                records.push({ RecordID: this.selectedIds[id].id, RowIndex: this.selectedIds[id].index });
            }
        } else {
            var selectedRecords = this.selModel.getSelections(),
                i = 0;

            for (i; i < selectedRecords.length; i++) {
                records.push({ RecordID: selectedRecords[i].id, RowIndex: this.store.indexOfId(selectedRecords[i].id) });
            }
        }

        this.hField.setValue(Ext.encode(records));
    },

    updateCellSelection : function (sm, selection) {
        if (selection === null) {
            this.hField.setValue("");
        }
    },

    cellSelect : function (sm, rowIndex, colIndex) {
        var r = this.store.getAt(rowIndex),
            selection = {
                record: r,
                cell: [rowIndex, colIndex]
            },
            name = this.getColumnModel().getDataIndex(selection.cell[1]),
            value = selection.record.get(name),
            id = selection.record.id || "";

        this.hField.setValue(Ext.encode({ RecordID: id, Name: name, SubmittedValue: value, RowIndex: selection.cell[0], ColIndex: selection.cell[1] }));
    },

    selectionMemory: true,

    //private
    removeOrphanColumnPlugins : function (column) {
        var p,
            i = 0;

        while (i < this.plugins.length) {
            p = this.plugins[i];

            if (p.isColumnPlugin) {
                if (this.getColumnModel().config.indexOf(p) === -1) {
                    this.plugins.remove(p);

                    if (p.destroy) {
                        p.destroy();
                    }
                } else {
                    i++;
                }
            } else {
                i++;
            }
        }
    },

    addColumnPlugins : function (plugins, init) {
        if (Ext.isArray(plugins)) {
            var i = 0;

            for (i; i < plugins.length; i++) {

                this.plugins.push(plugins[i]);

                if (init && plugins[i].init) {
                    plugins[i].init(this);
                }
            }
        } else {
            this.plugins.push(plugins);

            if (init && plugins.init) {
                plugins.init(this);
            }
        }
    },

    initColumnPlugins : function (plugins, init) {
        var cp = [],
            p,
            i = 0;

        this.initGridPlugins();

        if (init) {
            this.removeOrphanColumnPlugins();
        }

        for (i; i < plugins.length; i++) {
            p = this.getColumnModel().config[plugins[i]];
            p.isColumnPlugin = true;
            cp.push(p);
        }
        this.addColumnPlugins(cp, init);
    },

    initGridPlugins : function () {
        if (Ext.isEmpty(this.plugins)) {
            this.plugins = [];
        } else if (!Ext.isArray(this.plugins)) {
            this.plugins = [this.plugins];
        }
    },

    initSelectionData : function () {
        if (this.store) {
            if (this.store.getCount() > 0) {
                this.doSelection();
            } else {
                this.store.on("load", this.doSelection, this, { single: true });
            }
        }
    },

    initComponent : function () {
        Ext.net.GridPanel.superclass.initComponent.call(this);

        this.initGridPlugins();

        if (this.columnPlugins) {
            this.initColumnPlugins(this.columnPlugins, false);
        }

        if (this.getView().headerGroupRows) {
            this.plugins.push(new Ext.ux.grid.ColumnHeaderGroup({ rows: this.getView().headerGroupRows }));
        }

        var cm = this.getColumnModel(),
            j = 0;

        for (j; j < cm.config.length; j++) {
            var column = cm.config[j];

            if (column.commands) {
                this.addColumnPlugins([new Ext.net.CellCommands()]);
                break;
            }
        }

        if (this.selectionMemory) {
            this.selModel.on("rowselect", this.onMemorySelect, this);
            this.selModel.on("rowdeselect", this.onMemoryDeselect, this);
            this.store.on("remove", this.onStoreRemove, this);
            this.getView().on("refresh", this.memoryReConfigure, this);
        }

        this.on("viewready", this.initSelectionData, this);

        if (!this.record && this.store) {
            this.record = this.store.recordType;
        }

        if (this.disableSelection) {
            if (this.selModel.select) {
                this.selModel.select = Ext.emptyFn;
            } else if (this.selModel.selectRow) {
                this.selModel.selectRow = Ext.emptyFn;
            }
        }

        if (this.getView().headerRows) {
            var rowIndex = 0;

            for (rowIndex; rowIndex < this.view.headerRows.length; rowIndex++) {
                var cols = this.view.headerRows[rowIndex].columns,
                    colIndex = 0;

                for (colIndex; colIndex < cols.length; colIndex++) {
                    var col = cols[colIndex];

                    if (Ext.isEmpty(col.component)) {
                        continue;
                    }

                    if (Ext.isArray(col.component) && col.component.length > 0) {
                        col.component = col.component[0];
                    }

                    col.component = col.component.render ? col.component : Ext.ComponentMgr.create(col.component, "panel");
                }
            }

            this.on("resize", this.syncHeaders, this);
            this.on("columnresize", this.syncHeaders, this);
            this.colModel.on("hiddenchange", this.onHeaderRowHiddenChange, this);

            Ext.apply(this.getView(), {
                onColumnMove : function (cm, oldIndex, newIndex) {
                    var rowIndex = 0;

                    for (rowIndex; rowIndex < this.headerRows.length; rowIndex++) {
                        var cols = this.headerRows[rowIndex].columns,
                            c = cols[oldIndex];
                        cols.splice(oldIndex, 1);
                        cols.splice(newIndex, 0, c);
                    }
                    this.constructor.prototype.onColumnMove.call(this, cm, oldIndex, newIndex);
                },

                updateHeaders : function () {
                    var col, div;

                    if (this.headerControlsInsideGrid) {
                        var el = Ext.net.ResourceMgr.getAspForm() || Ext.getBody(),
                            ce,
                            i = 0;

                        for (i; i < this.headerRows.length; i++) {
                            var c1 = this.headerRows[i].columns,
                                j = 0;

                            for (j; j < c1.length; j++) {
                                col = c1[j];

                                if (col.component) {
                                    ce = Ext.fly(col.component.getPositionEl());
                                } else if (!Ext.isEmpty(col.target)) {
                                    var p1 = Ext.getCmp(col.target.id || "");

                                    if (!Ext.isEmpty(p1)) {
                                        ce = p1.getPositionEl();
                                    } else {
                                        ce = col.target;
                                    }
                                } else {
                                    continue;
                                }

                                ce.addClass("x-hidden");
                                el.dom.appendChild(ce.dom);
                            }
                        }

                        this.headerControlsInsideGrid = false;
                    }

                    this.constructor.prototype.updateHeaders.call(this);

                    if (this.headerRows) {
                        var ii = 0;

                        for (ii; ii < this.headerRows.length; ii++) {
                            var c2 = this.headerRows[ii].columns,
                                tr = this.mainHd.child("tr.x-grid3-hd-row-r" + ii),
                                jj = 0;

                            for (jj; jj < c2.length; jj++) {
                                col = c2[jj];

                                if (!Ext.isEmpty(col.component)) {
                                    div = Ext.fly(tr.dom.cells[jj]).child("div.x-grid3-hd-inner");

                                    if (col.component.rendered) {
                                        div.appendChild(col.component.getPositionEl());
                                        col.component.getPositionEl().removeClass("x-hidden");
                                    } else {
                                        col.component.render(div);
                                    }
                                } else if (!Ext.isEmpty(col.target)) {
                                    var p2 = Ext.getCmp(col.target.id || "");

                                    div = Ext.fly(tr.dom.cells[jj]).child("div.x-grid3-hd-inner");

                                    if (!Ext.isEmpty(p2)) {
                                        div.dom.appendChild(p2.getPositionEl().dom);
                                        p2.getPositionEl().removeClass("x-hidden");
                                    } else {
                                        div.dom.appendChild(col.target.dom);
                                        col.target.removeClass("x-hidden");
                                    }
                                }
                            }
                        }

                        this.grid.syncHeaders.defer(100, this.grid);

                        var cm = this.grid.getColumnModel(),
                            k = 0;

                        for (k; k < cm.columns.length; k++) {
                            if (cm.isHidden(k)) {
                                this.grid.onHeaderRowHiddenChange(cm, k, true);
                            }
                        }

                        this.headerControlsInsideGrid = true;
                    }
                }
            });
        }

        if (this.clearEditorFilter) {
            this.on("beforeedit", function (e) {
                var ed = this.getColumnModel().config[e.column].editor;

                if (!Ext.isEmpty(ed) && ed.field && ed.field.store && ed.field.store.clearFilter) {
                    ed.field.store.clearFilter();
                }
            }, this);
        }
    },

    

    clearMemory : function () {
        delete this.selModel.selectedData;
        this.selectedIds = {};
        this.hField.setValue("");
    },

    memoryReConfigure : function () {
        this.store.on("clear", this.onMemoryClear, this);
        this.store.on("datachanged", this.memoryRestoreState, this);
    },

    onMemorySelect : function (sm, idx, rec) {
        if (this.getSelectionModel().singleSelect) {
            this.clearMemory();
        }

        var id = this.getRecId(rec),
            absIndex = this.getAbsoluteIndex(idx);

        this.onMemorySelectId(sm, absIndex, id);
    },

    onMemorySelectId : function (sm, index, id) {
        var obj = { 
            id    : id, 
            index : index 
        };
        
        this.selectedIds[id] = obj;
    },

    getPagingToolbar : function () {
        var bar = this.getBottomToolbar();

        if (bar && bar.getPageData) {
            return bar;
        }

        bar = this.getTopToolbar();

        if (bar && bar.getPageData) {
            return bar;
        }

        return null;
    },

    getAbsoluteIndex : function (pageIndex) {
        var absIndex = pageIndex,
            bar = this.getPagingToolbar();

        if (!Ext.isEmpty(bar)) {
            absIndex = ((bar.getPageData().activePage - 1) * bar.pageSize) + pageIndex;
        }

        return absIndex;
    },

    onMemoryDeselect : function (sm, idx, rec) {
        delete this.selectedIds[this.getRecId(rec)];
    },

    onStoreRemove : function (store, rec, idx) {
        this.onMemoryDeselect(null, idx, rec);
    },

    memoryRestoreState : function () {
        if (this.store !== null) {
            var i = 0,
                sel = [],
                all = true,
                silent = true;

            if (this.selModel.isLocked()) {
                this.wasLocked = true;
                this.selModel.unlock();
            }

            this.store.each(function (rec) {
                var id = this.getRecId(rec);

                if (!Ext.isEmpty(this.selectedIds[id])) {
                    sel.push(i);
                } else {
                    all = false;
                }

                ++i;
            }, this);

            if (!Ext.isEmpty(this.fireSelectOnLoad)) {
                silent = !this.fireSelectOnLoad;
            }

            if (sel.length > 0) {
                if (silent) {
                    this.suspendEvents();
                    this.selModel.suspendEvents();
                }

                this.selModel.selectRows(sel);

                if (silent) {
                    this.resumeEvents();
                    this.selModel.resumeEvents();
                }
            }

            if (this.selModel.checkHeader) {
                if (all) {
                    this.selModel.checkHeader();
                } else {
                    this.selModel.uncheckHeader();
                }
            }

            if (this.wasLocked) {
                this.selModel.lock();
            }
        }
    },

    getRecId : function (rec) {
        var id = rec.get(this.memoryIDField);

        if (Ext.isEmpty(id)) {
            id = rec.id;
        }

        return id;
    },

    onMemoryClear : function () {
        this.selectedIds = {};
    },

    

    getSelectionModelField : function () {
        if (!this.selectionModelField) {
            this.selectionModelField = new Ext.form.Hidden({ id: this.id + "_SM", name: this.id + "_SM" });
            this.on("beforedestroy", function () { 
                if (this.rendered) {
                    this.destroy();
                }
            }, this.selectionModelField);
        }

        return this.selectionModelField;
    },

    initSelection : function () {
        this.hField = this.getSelectionModelField();

        if (this.selModel.select) {
            this.selModel.on("cellselect", this.cellSelect, this);
            this.selModel.on("selectionchange", this.updateCellSelection, this);
        } else if (this.selModel.selectRow) {
            this.selModel.on("rowselect", this.updateSelectedRows, this, { buffer: this.selectionSavingBuffer });
            this.selModel.on("rowdeselect", this.updateSelectedRows, this, { buffer: this.selectionSavingBuffer });
            this.store.on("remove", this.updateSelectedRows, this, { buffer: this.selectionSavingBuffer });
        }
    },

    getKeyMap : function () {
        if (!this.keyMap) {
            this.keyMap = new Ext.KeyMap(this.view.el, this.keys);
        }

        return this.keyMap;
    },

    onRender : function (ct, position) {
        Ext.net.GridPanel.superclass.onRender.call(this, ct, position);

        this.getSelectionModelField().render(this.el.parent() || this.el);

        if (this.menu instanceof Ext.menu.Menu) {
            this.on("contextmenu", this.showContextMenu);
            this.on("rowcontextmenu", this.onRowContextMenu);
        }

        this.relayEvents(this.selModel, ["rowselect", "rowdeselect"]);
        this.relayEvents(this.store, ["commitdone", "commitfailed"]);
    },

    onHeaderRowHiddenChange : function (cm, colIndex, hidden) {
        var display = hidden ? "none" : "",
            rowIndex = 0;

        for (rowIndex; rowIndex < this.view.headerRows.length; rowIndex++) {
            var tr = this.view.mainHd.child("tr.x-grid3-hd-row-r" + rowIndex);

            if (tr && tr.dom.cells.length > colIndex) {
                Ext.fly(tr.dom.cells[colIndex]).dom.style.display = display;
            }
        }

        this.syncHeaders.defer(100, this);
    },

    syncHeaders : function () {
        var rowIndex = 0;

        for (rowIndex; rowIndex < this.view.headerRows.length; rowIndex++) {
            var cols = this.view.headerRows[rowIndex].columns,
                colIndex = 0;

            for (colIndex; colIndex < cols.length; colIndex++) {
                var col = cols[colIndex],
                    cmp;

                if (!Ext.isEmpty(col.component)) {
                    cmp = col.component;
                } else if (!Ext.isEmpty(col.target)) {
                    cmp = Ext.getCmp(col.target.id || "");
                } else {
                    continue;
                }

                if (col.autoWidth !== false) {
                    var autoCorrection = Ext.isEmpty(col.correction) ? 3 : col.correction;

                    if (Ext.isIE && !Ext.isEmpty(cmp)) {
                        autoCorrection -= 1;
                    }

                    if (!Ext.isEmpty(cmp) && cmp.setSize) {
                        cmp.setSize(this.getColumnModel().getColumnWidth(colIndex) - autoCorrection);
                    } else if (col.target) {
                        col.target.setSize(this.getColumnModel().getColumnWidth(colIndex) - autoCorrection, col.target.getSize().height);
                    }
                }
            }
        }
    },

    onRowContextMenu : function (grid, rowIndex, e) {
        e.stopEvent();

        if (!this.selModel.isSelected(rowIndex)) {
            this.selModel.selectRow(rowIndex);
            this.fireEvent("rowclick", this, rowIndex, e);
        }

        this.showContextMenu(e, rowIndex);
    },

    showContextMenu : function (e, rowIndex) {
        e.stopEvent();

        if (rowIndex === undefined) {
            this.selModel.clearSelections();
        }

        if (this.menu) {
            this.menu.showAt(e.getXY());
        }
    },

    reload : function (options) {
        this.store.reload(options);
    },

    isDirty : function () {
        if (this.store.modified.length > 0 || this.store.deleted.length > 0) {
            return true;
        }

        return false;
    },

    hasSelection : function () {
        return this.selModel.hasSelection();
    },

    addRecord : function (values, commit, clearFilter) {
        var rowIndex = this.store.data.length;

        this.insertRecord(rowIndex, values, commit, clearFilter);
        return rowIndex;
    },

    addRecordEx : function (values, commit, clearFilter) {
        var rowIndex = this.store.data.length,
            record = this.insertRecord(rowIndex, values, commit, clearFilter);

        return { index: rowIndex, record: record };
    },

    insertRecord : function (rowIndex, values, commit, clearFilter) {
        if (arguments.length === 0) {
            this.insertRecord(0, {});
            this.getView().focusRow(0);
            this.startEditing(0, 0);

            return;
        }

        return this.store.insertRecord(rowIndex, values, false, commit, clearFilter);
    },

    deleteRecord : function (record) {
        this.store.remove(record);
    },

    deleteSelected : function () {
        var s = this.selModel.getSelections(),
            i;

        for (i = 0, len = s.length; i < len; i++) {
            this.deleteRecord(s[i]);
        }
    },

    clear : function () {
        this.store.removeAll();
    },

    saveMask: false,

    initEvents : function () {
        Ext.net.GridPanel.superclass.initEvents.call(this);

        if (this.saveMask) {
            this.saveMask = new Ext.net.SaveMask(this.bwrap,
                    Ext.apply({ writeStore: this.store }, this.saveMask));
        }
    },

    reconfigure : function (store, colModel) {
        Ext.net.GridPanel.superclass.reconfigure.call(this, store, colModel);

        if (this.saveMask) {
            this.saveMask.destroy();
            this.saveMask = new Ext.net.SaveMask(this.bwrap,
                    Ext.apply({ writeStore: store }, this.initialConfig.saveMask));
        }
    },

    onDestroy : function () {
        if (this.rendered) {
            if (this.saveMask) {
                this.saveMask.destroy();
            }
        }

        Ext.net.GridPanel.superclass.onDestroy.call(this);
    },

    insertColumn : function (index, newCol) {
        var c = this.getColumnModel().config,
            cfg;

        if (index >= 0) {
            c.splice(index, 0, newCol);
        }

        cfg = Ext.apply({ columns: c }, { events: this.getColumnModel().events, directEvents: this.getColumnModel().directEvents, defaultSortable: this.getColumnModel().defaultSortable });

        this.reconfigure(this.store, new Ext.grid.ColumnModel(cfg));
    },

    addColumn : function (newCol) {
        var c = this.getColumnModel().config,
            cfg;

        c.push(newCol);

        cfg = Ext.apply({ columns: c }, { events: this.getColumnModel().events, directEvents: this.getColumnModel().directEvents, defaultSortable: this.getColumnModel().defaultSortable });

        this.reconfigure(this.store, new Ext.grid.ColumnModel(cfg));
    },

    removeColumn : function (index) {
        var c = this.getColumnModel().config,
            cfg;

        if (index >= 0) {
            c.splice(index, 1);
        }

        cfg = Ext.apply({ columns: c }, { events: this.getColumnModel().events, directEvents: this.getColumnModel().directEvents, defaultSortable: this.getColumnModel().defaultSortable });

        this.reconfigure(this.store, new Ext.grid.ColumnModel(cfg));
    },

    reconfigureColumns : function (cfg) {
        var oldCM = this.getColumnModel(),
            newCM,
            specialCols = ["checker", "expander"],
            i;

        cfg = Ext.apply(cfg.columns ? cfg : { columns: cfg }, { events: oldCM.events, directEvents: oldCM.directEvents, defaultSortable: oldCM.defaultSortable });

        Ext.each(cfg.columns, function (col) {
            if (col.id === "expander") {
                specialCols.remove("expander");
                return false;
            }
        });
        
        for (i = 0; i < specialCols.length; i++) {
            var specCol = oldCM.getColumnById(specialCols[i]);

            if (!Ext.isEmpty(specCol)) {
                var index = oldCM.getIndexById(specialCols[i]);

                if (index !== 0 && index >= cfg.columns.length) {
                    index = cfg.columns.length - 1;
                }

                cfg.columns.splice(index, 0, specCol);
            }
        }
        newCM = oldCM.isLocked ? new Ext.ux.grid.LockingColumnModel(cfg) : new Ext.grid.ColumnModel(cfg);
        this.reconfigure(this.store, newCM);
    },

    load : function (options) {
        this.store.load(options);
    },

    save : function (options) {
        if (options && options.visibleOnly) {
            options.grid = this;
        }

        this.stopEditing(false);

        this.store.save(options);
    },

    // config :
    //    - selectedOnly
    //    - visibleOnly
    //    - dirtyCellsOnly
    //    - dirtyRowsOnly
    //    - currentPageOnly
    //    - excludeId
    //    - filterRecord - function (record) - return false to exclude the record
    //    - filterField - function (record, fieldName, value) - return false to exclude the field for particular record
    getRowsValues : function (config) {
        config = config || {};

        this.stopEditing(false);

        var records = (config.selectedOnly ? this.selModel.getSelections() : config.currentPageOnly ? this.store.getRange() : this.store.getAllRange()) || [],
            values = [],
            record,
            i;

        if (this.selectionMemory && config.selectedOnly && !config.currentPageOnly && this.store.isPagingStore()) {
            records = [];

            var id;

            for (id in this.selectedIds) {
                record = this.store.getById(this.selectedIds[id].id);

                if (!Ext.isEmpty(record)) {
                    records.push(record);
                }
            }
        }

        for (i = 0; i < records.length; i++) {
            var obj = {}, dataR;

            if (this.store.metaId()) {
                obj[this.store.metaId()] = config.excludeId === true ? undefined : records[i].id;
            }

            dataR = Ext.apply(obj, records[i].data);
            config.grid = this;
            dataR = this.store.prepareRecord(dataR, records[i], config);

            if (!Ext.isEmptyObj(dataR)) {
                values.push(dataR);
            }
        }

        return values;
    },

    serialize : function (config) {
        return Ext.encode(this.getRowsValues(config));
    },

    // config:
    //   - selectedOnly,
    //   - visibleOnly
    //   - dirtyCellsOnly
    //   - dirtyRowsOnly
    //   - currentPageOnly
    //   - excludeId
    //   - encode
    //    - filterRecord - function (record) - return false to exclude the record
    //    - filterField - function (record, fieldName, value) - return false to exclude the field for particular record
    submitData : function (config) {
        config = config || {};
        config.selectedOnly = config.selectedOnly || false;
        encode = config.encode;

        var values = this.getRowsValues(config);

        if (!values || values.length === 0) {
            return false;
        }

        if (encode) {
            values = Ext.util.Format.htmlEncode(values);
            delete config.encode;
        }

        this.store.submitData(values, config);
    },

    onEditComplete : function (ed, value, startValue) {
        Ext.net.GridPanel.superclass.onEditComplete.call(this, ed, value, startValue);

        ed.field.reset();

        if (!ed.record.dirty && ed.record.firstEdit) {
            this.store.remove(ed.record);
        }

        delete ed.record.firstEdit;
        this.fireEvent("editcompleted", ed, value, startValue);
    },

    stopEditing : function (cancel) {
        var ae = this.activeEditor;

        Ext.net.GridPanel.superclass.stopEditing.call(this, cancel);

        if (ae) {
            ae.field.reset();
        }
    },

    startEditing : function (row, col) {
        this.stopEditing();

        if (this.colModel.isCellEditable(col, row)) {
            this.view.ensureVisible(row, col, true);
            var r = this.store.getAt(row),
                field = this.colModel.getDataIndex(col),
                e = {
                    grid   : this,
                    record : r,
                    field  : field,
                    value  : r.data[field],
                    row    : row,
                    column : col,
                    cancel : false
                };

            if (this.fireEvent("beforeedit", e) !== false && !e.cancel) {
                this.editing = true;
                var ed = this.colModel.getCellEditor(col, row);

                if (!ed) {
                    return;
                }

                if (!ed.rendered) {
                    ed.parentEl = this.view.getEditorParent(ed);
                    ed.on({
                        scope  : this,
                        render : {
                            fn : function (c) {
                                c.field.focus(false, true);
                            },
                            single : true,
                            scope  : this
                        },
                        specialkey : function (field, e) {
                            this.getSelectionModel().onEditorKey(field, e);
                        },
                        complete   : this.onEditComplete,
                        canceledit : this.stopEditing.createDelegate(this, [true])
                    });
                }

                Ext.apply(ed, {
                    row    : row,
                    col    : col,
                    record : r
                });

                this.lastEdit = {
                    row : row,
                    col : col
                };

                this.activeEditor = ed;

                // Set the selectSameEditor flag if we are reusing the same editor again and
                // need to prevent the editor from firing onBlur on itself.
                ed.selectSameEditor = (this.activeEditor == this.lastActiveEditor);
                var v = this.preEditValue(r, field);
                ed.startEdit(this.view.getCell(row, col).firstChild, Ext.isDefined(v) ? v : "");

                // Clear the selectSameEditor flag
                (function () {
                    delete ed.selectSameEditor;
                }).defer(250); // IT IS OVERRIDEN (50 ms is too small for IE)
            }
        }
    }
});

Ext.reg("netgrid", Ext.net.GridPanel);

// @source data/GroupingView.js

Ext.grid.GroupingView.override({
    onRemove : function (ds, record, index, isUpdate) {
        Ext.grid.GroupingView.superclass.onRemove.apply(this, arguments);
        
        var g = document.getElementById(Ext.util.Format.htmlDecode(record._groupId));
        
        if (g && g.childNodes[1].childNodes.length < 1) {
            Ext.removeNode(g);
        }
        
        this.applyEmptyText();
    },
    
    getRows : function () {
        if (!this.canGroup()) {
            return Ext.grid.GroupingView.superclass.getRows.call(this);
        }
        
        var r = [],
            g, 
            gs = this.getGroups(),
            i = 0,
            len;
        
        for (i, len = gs.length; i < len; i++) {
            if (gs[i].childNodes.length > 1) {
                g = gs[i].childNodes[1].childNodes;
                
                var j,
                    jlen;

                for (j = 0, jlen = g.length; j < jlen; j++) {
                    r[r.length] = g[j];
                }
            }
        }
        
        return r;
    },
    
    getGroupRecords : function (groupValue) {
        var gid = this.getGroupId(groupValue);
        
        if (gid) {
            var re = new RegExp(RegExp.escape(gid)),
                records = this.grid.store.queryBy(function (record) {
                    return record._groupId.match(re);
                });
                
            return records ? records.items : [];
        }
        
        return [];
    },
    
    // remove after ExtJS 3.3.0 release
    renderUI : function () {
        return Ext.grid.GroupingView.superclass.renderUI.call(this);
    }
});

// @source data/PagingMemoryProxy.js

Ext.data.PagingMemoryProxy = function (data, isUrl) {
	Ext.data.PagingMemoryProxy.superclass.constructor.call(this);
	this.data = data;
	this.isUrl = isUrl || false;		
	this.isNeedRefresh = this.isUrl;
	this.url = this.isUrl ? data : "";	
	this.isMemoryProxy = true;
};

Ext.extend(Ext.data.PagingMemoryProxy, Ext.data.MemoryProxy, {
    refreshData : function (data, store) {
        if (this.isUrl === true) {
            this.isNeedRefresh = true;
        } else {
            if (data && data !== null) {
                this.data = data;
            } else {
                store.callbackReload(store.warningOnDirty);
            }
        }
    },

    refreshByUrl : function (params, reader, callback, scope, arg) {
        var o = {
            method   : "GET",
            request  : {
                callback : callback,
                scope    : scope,
                arg      : arg,
                params   : params || {}
            },
            reader   : reader,
            url      : this.url,
            callback : this.loadResponse,
            scope    : this
        };

        if (this.activeRequest) {
            Ext.Ajax.abort(this.activeRequest);
        }

        this.activeRequest = Ext.Ajax.request(o);
    },

    loadResponse : function (o, success, response) {
        delete this.activeRequest;
        
        if (!success) {
            this.fireEvent("loadexception", this, o, response);
            o.request.callback.call(o.request.scope, null, o.request.arg, false);

            return;
        }

        try {
            if (o.reader.getJsonAccessor) {
                this.data = response.responseText;
            } else {
                this.data = response.responseXML;
            }

            if (!this.data) {
                throw { message : "The data is not available" };
            }
        } catch (e) {
            this.fireEvent("loadexception", this, o, response, e);
            o.request.callback.call(o.request.scope, null, o.request.arg, false);

            return;
        }

        this.isNeedRefresh = false;
        this.load(o.request.params, o.reader, o.request.callback, o.request.scope, o.request.arg);
    },
    
    doRequest : function (action, rs, params, reader, callback, scope, arg) {
        this.fireEvent("beforeload", this, params);
        
        params = params || {};

        if (this.isNeedRefresh === true) {
            this.refreshByUrl(params, reader, callback, scope, arg);

            return;
        }

        var result;
        
        try {
            result = reader.readRecords(this.data);
        } catch (e) {
            this.fireEvent("loadexception", this, null, arg, e);
            this.fireEvent("exception", this, "response", action, arg, null, e);
            callback.call(scope, null, arg, false);

            return;
        }

        if (params.gridfilters !== undefined) {
            var r = [],
                i,
                len;

            for (i = 0, len = result.records.length; i < len; i++) {
                if (params.gridfilters.call(this, result.records[i])) {
                    r.push(result.records[i]);
                }
            }
            result.records = r;
            result.totalRecords = result.records.length;
        }


        if (params.sort !== undefined) {
            var dir = String(params.dir).toUpperCase() === "DESC" ? -1 : 1,
                st = scope.fields.get(params.sort).sortType,
                fn = function (r1, r2) {
                    var v1 = st(r1), v2 = st(r2);
                    return v1 > v2 ? 1 : (v1 < v2 ? -1 : 0);
                };

            result.records.sort(function (a, b) {
                var v = 0;
                
                v = (typeof (a) === "object") ? fn(a.data[params.sort], b.data[params.sort]) * dir : fn(a, b) * dir;
                
                if (v === 0) {
                    v = (a.index < b.index ? -1 : 1);
                }
                
                return v;
            });
        }

        if (params.start !== undefined && params.limit !== undefined) {
            result.records = result.records.slice(params.start, params.start + params.limit);
        }

        callback.call(scope, result, arg, true);
    }
});

// @source data/PagingToolbar.js

Ext.PagingToolbar.prototype.onRender = Ext.PagingToolbar.prototype.onRender.createSequence(function (el) {
    if (this.pageIndex) {
        if (this.store.getCount() === 0) {
            this.store.on("load", function () {
                this.changePage(this.pageIndex);
            }, this, { single : true });
        } else {
            this.changePage(this.pageIndex);
        }
    }
    
    this.on("change", function (el, data) {
        this.getActivePageField().setValue(data.activePage);
    }, this);
    
    this.getActivePageField().render(this.el.parent() || this.el);
    
    if (this.store.proxy.isMemoryProxy) {
        this.refresh.setHandler(function () {                    
            if (this.store.proxy.refreshData) {
                this.store.proxy.refreshData(null, this.store);
            }
            
            if (this.store.proxy.isUrl) {
                item.initialConfig.handler();
            }
        }, this);         
    }
    
    if (this.hideRefresh) {
        this.refresh.hide();
    }
});

Ext.PagingToolbar.prototype.initComponent = Ext.PagingToolbar.prototype.initComponent.createSequence(function () {
    if (this.ownerCt instanceof Ext.net.GridPanel) {
        this.ownerCt.on("viewready", this.fixFirstLayout, this, {single : true});
    } else {
        this.on("afterlayout", this.fixFirstLayout, this, {single : true});
    }
});

Ext.PagingToolbar.override({
    hideRefresh: false,
    onFirstLayout : Ext.emptyFn,
    
    getActivePageField : function () {
        if (!this.activePageField) {
            this.activePageField = new Ext.form.Hidden({ 
                id   : this.id + "_ActivePage", 
                name : this.id + "_ActivePage" 
            });

			this.on("beforedestroy", function () { 
                if (this.rendered) {
                    this.destroy();
                }
            }, this.activePageField);
        }
        
        return this.activePageField;
    },
    
    fixFirstLayout : function () {
        if (this.dsLoaded) {
            this.onLoad.apply(this, this.dsLoaded);
        }
    }   
});

// @source data/PropertyGrid.js

Ext.net.PropertyGrid = function () {
    Ext.net.PropertyGrid.superclass.constructor.call(this);	
	this.addEvents("beforesave", "save", "saveexception");
};

Ext.net.PropertyGrid = Ext.extend(Ext.grid.PropertyGrid, {
    editable : true,
        
    getDataField : function () {
        if (!this.dataField) {
            this.dataField = new Ext.form.Hidden({ id : this.id + "_Data", name : this.id + "_Data" });

			this.on("beforedestroy", function () { 
                if (this.rendered) {
                    this.destroy();
                }
            }, this.dataField);
        }
        
        return this.dataField;
    },

    initComponent : function () {
        Ext.net.PropertyGrid.superclass.initComponent.call(this);
        
        this.propertyNames = this.propertyNames || [];
        
        if (!this.editable) {
            this.on("beforeedit", function (e) {
                return false;
            });
        }
        
        this.on("propertychange", function (source) {
            this.saveSource(source);
        });
    },

    onRender : function () {
        Ext.net.PropertyGrid.superclass.onRender.apply(this, arguments);
        this.getDataField().render(this.el.parent() || this.el);
    },

    callbackHandler : function (response, result, context, type, action, extraParams) {
        try {
            var responseObj = result.serviceResponse;
            result = { success : responseObj.success, msg : responseObj.message || null };
        } catch (e) {
            context.fireEvent("saveexception", context, response, e);

            return;
        }

        if (result.success === false) {
            context.fireEvent("saveexception", context, response, { message : result.msg });

            return;
        }

        context.fireEvent("save", context, response);
    },

    callbackErrorHandler : function (response, result, context, type, action, extraParams) {
        context.fireEvent("saveexception", context, response, { message : result.errorMessage || response.statusText });
    },
    
    saveSource : function (source) {
        this.getDataField().setValue(Ext.encode(source || this.propStore.getSource()));
    },

    save : function () {
        var options = { params : {} };
        
        if (this.fireEvent("beforesave", this, options) !== false) {
            var config = {}, 
                ac = this.directEventConfig;
                
            ac.userSuccess = this.callbackHandler;
            ac.userFailure = this.callbackErrorHandler;
            ac.extraParams = options.params;
            ac.enforceFailureWarning = !this.hasListener("saveexception");

            Ext.apply(config, ac, { control : this, eventType : "postback", action : "update", serviceParams : Ext.encode(this.getSource()) });
            Ext.net.DirectEvent.request(config);
        }
    },
    
    setProperty : function (prop, value, create) {
        this.propStore.setValue(prop, value, create);   
        if (create) {
            this.saveSource(); 
        }
    },
    
    removeProperty : function (prop) {
        this.propStore.remove(prop);
        this.saveSource(); 
    } 
});

Ext.reg("netpropertygrid", Ext.net.PropertyGrid);

// @source data/ArrayReader.js

Ext.data.ArrayReader.override({
    isArrayReader : true
});

// @source data/PageProxy.js

Ext.net.PageProxy = function () {
    var api = {};
    
    api[Ext.data.Api.actions.read] = true;
    Ext.net.PageProxy.superclass.constructor.call(this, {
        api: api
    });
};

Ext.extend(Ext.net.PageProxy, Ext.data.DataProxy, {
    ro : {},
    isDataProxy : true,

    doRequest : function (action, rs, params, reader, callback, scope, arg) {
        if (this.fireEvent("beforeload", this, params) !== false) {
            this.ro = {
                params  : params || {},
                request : {
                    callback : callback,
                    scope    : scope,
                    arg      : arg
                },
                reader   : reader,
                callback : this.loadResponse,
                scope    : this
            };

            var config = {},
                ac = scope.directEventConfig;

            ac.userSuccess = this.successHandler;
            ac.userFailure = this.errorHandler;
            ac.extraParams = params;
            ac.enforceFailureWarning = !this.hasListener("loadexception");

            Ext.apply(config, ac, { 
                control   : scope, 
                eventType : "postback", 
                action    : "refresh" 
            });
            
            Ext.net.DirectEvent.request(config);
        } else {
            callback.call(scope || this, null, arg, false);
        }
    },

    successHandler : function (response, result, context, type, action, extraParams) {
        var p = context.proxy;

        try {
            var responseObj = result.serviceResponse;
            result = { success: responseObj.success, msg: responseObj.message || null, data: responseObj.data || {} };
        } catch (e) {
            context.fireEvent("loadexception", context, {}, response, e);
            context.fireEvent("exception", context, "remote", "read", {}, response, e);
            p.ro.request.callback.call(p.ro.request.scope, null, p.ro.request.arg, false);

            if (p.ro.request.scope.showWarningOnFailure) {
                Ext.net.DirectEvent.showFailure(response, e.message);
            }

            return;
        }

        if (result.success === false) {
            context.fireEvent("loadexception", context, {}, response, { message: result.msg });
            context.fireEvent("exception", context, "remote", "read", {}, response, { message: result.msg });
            p.ro.request.callback.call(p.ro.request.scope, null, p.ro.request.arg, false);

            if (p.ro.request.scope.showWarningOnFailure) {
                Ext.net.DirectEvent.showFailure(response, result.msg);
            }

            return;
        }

        try {
            var meta = p.ro.reader.meta,
                rebuild = false;

            if (Ext.isEmpty(meta.totalProperty)) {
                rebuild = true;
                meta.totalProperty = "total";
            }

            if (Ext.isEmpty(meta.root)) {
                rebuild = true;
                meta.root = "data";
            }

            if (rebuild) {
                delete p.ro.reader.ef;
                p.ro.reader.buildExtractors();
            }

            if (Ext.isEmpty(result.data[meta.root])) {
                result.data[meta.root] = [];
            }

            result = p.ro.reader.readRecords(result.data);

        } catch (ex) {
            p.fireEvent("loadexception", p, p.ro, response, ex);
            p.fireEvent("exception", p, "remote", "read", p.ro, response, ex);
            p.ro.request.callback.call(p.ro.request.scope, null, p.ro.request.arg, false);

            if (p.ro.request.scope.showWarningOnFailure) {
                Ext.net.DirectEvent.showFailure(response, ex.message);
            }

            return;
        }
        
        p.fireEvent("load", p, p.ro, p.ro.request.arg);
        p.ro.request.callback.call(p.ro.request.scope, result, p.ro.request.arg, true);

    },

    errorHandler : function (response, result, context, type, action, extraParams) {
        var p = context.proxy;

        p.fireEvent("loadexception", p, p.ro, response, {message : response.responseText});
        p.fireEvent("exception", p, "response", "read", p.ro, response, {message : response.responseText});
        p.ro.request.callback.call(p.ro.request.scope, null, p.ro.request.arg, false);

        if (p.ro.request.scope.showWarningOnFailure) {
            Ext.net.DirectEvent.showFailure(response, response.responseText);
        }
    }
});

// @source data/RowExpander.js

Ext.grid.RowExpander = Ext.extend(Ext.util.Observable, {
    header        : "",
    width         : 20,
    sortable      : false,
    fixed         : true,
    menuDisabled  : true,
    dataIndex     : "",
    id            : "expander",
    lazyRender    : true,
    enableCaching : true,
    expandOnEnter : true,
    singleExpand  : false,
    hideable      : false,
    expandOnDblClick  : true,
    swallowBodyEvents : false,
    rowBodySelector   : "table.x-grid3-row-table > tbody > tr:nth(2) div.x-grid3-row-body",
    
    constructor: function (config) {
        Ext.apply(this, config);

        this.addEvents({
            beforeexpand   : true,
            expand         : true,
            beforecollapse : true,
            collapse       : true
        });

        Ext.grid.RowExpander.superclass.constructor.call(this);

        if (this.tpl) {
            if (typeof this.tpl === "string") {
                this.tpl = new Ext.Template(this.tpl);
            }
            
            this.tpl.compile();
        }
        
        if (this.component) {
            if (!this.component.rendered) {
                this.componentCfg = this.component;
                this.component = Ext.ComponentMgr.create(this.component, "panel");
            }            
        }

        this.state = {};
        this.bodyContent = {};
    },
    
    getExpanded : function () {
        var store = this.grid.store,
            expandedRecords = [];

        store.each(function (record, index) {
            if (this.state[record.id]) {
                expandedRecords.push({
                    record : record, 
                    index  : index
                });
            }
        }, this);
        
        return expandedRecords;
    },
    
    getRowClass : function (record, rowIndex, p, ds) {
        p.cols = p.cols - 1;
        var content = this.bodyContent[record.id];
        
        if (!content && !this.lazyRender) {
            content = this.getBodyContent(record, rowIndex);
        }
        
        if (content) {
            p.body = content;
        }
        
        return this.state[record.id] ? "x-grid3-row-expanded" : "x-grid3-row-collapsed";
    },

    init : function (grid) {
        this.grid = grid;

        var view = grid.getView();
        view.getRowClass = this.getRowClass.createDelegate(this);

        view.enableRowBody = true;


        grid.on("render", this.onRender, this);
        grid.on("destroy", this.onDestroy, this);
        
        this.grid.store.on("load", function () {
            this.bodyContent = {};
        }, this);
    },
    
    onRender : function () {
        var grid = this.grid;
        var mainBody = grid.getView().mainBody;
        mainBody.on("mousedown", this.onMouseDown, this, { delegate : ".x-grid3-row-expander" });
        
        if (this.expandOnEnter) {            
            this.keyNav = new Ext.KeyNav(this.grid.getGridEl(), {
                "enter" : this.onEnter,
                scope: this        
            });    
        }
        
        var view = this.grid.getView();
        
        if (this.expandOnDblClick) {
            grid.on("rowdblclick", this.onRowDblClick, this);
        }        
        
        if (this.swallowBodyEvents) {
            view.on("rowupdated", this.swallowRow, this);
            view.on("refresh", this.swallowRow, this);            
        }
               
        if (this.component) {            
            view.removeRow = view.removeRow.createInterceptor(this.moveComponent, this);
            view.refreshRow = view.refreshRow.createInterceptor(this.moveComponent, this);
            view.removeRows = view.removeRows.createInterceptor(this.moveComponent, this);            
            view.on("beforerefresh", this.moveComponent, this);
            view.on("rowupdated", this.restoreComponent, this);
            view.on("refresh", this.restoreComponent, this);            
        }
    },
    
    moveComponent: function () {
        if (!this.componentInsideGrid) {
            return;
        }
        
        var ce = Ext.fly(this.component.getEl()), el = Ext.net.ResourceMgr.getAspForm() || Ext.getBody();
                    
        ce.addClass("x-hidden");
        
        el = el.dom;        
        el.appendChild(ce.dom);
        this.componentInsideGrid = false;
    },
    
    restoreComponent: function () {
        if (this.component.rendered === false) {
            return;
        }
        
        this.grid.store.each(function (record, i) {
            if (this.state[record.id]) {
                var row = this.grid.view.getRow(i),              
                    body = Ext.DomQuery.selectNode(this.rowBodySelector, row);                
                
                Ext.fly(body).appendChild(this.component.getEl());
                this.component.removeClass("x-hidden");
                this.componentInsideGrid = true;
                return false;
            }
        }, this);
    },
    
    swallowRow: function () {
        this.grid.store.each(function (record, i) {
            if (this.state[record.id]) {
                var row = this.grid.view.getRow(i),              
                    body = Ext.DomQuery.selectNode(this.rowBodySelector, row);                
                
                Ext.fly(body).swallowEvent(['click', 'mousedown', 'mouseup', 'dblclick'], true);
            }
        }, this);
    },
    
    onDestroy: function () {
        if (this.keyNav) {
            this.keyNav.disable();
            delete this.keyNav;
        }
        
        var mainBody = this.grid.getView().mainBody;
        if (mainBody) {
            mainBody.un('mousedown', this.onMouseDown, this);
        }
        
        if (this.tpl && this.tpl.destroy) {
            this.tpl.destroy();
        }
        
        this.purgeListeners();
    },
   
    onRowDblClick: function (grid, rowIdx, e) {
        this.toggleRow(rowIdx);
    },
    
    onEnter : function (e) {
        var g = this.grid,
            sm = g.getSelectionModel(),
            sels = sm.getSelections(),
            i,
            len;
        
        for (i = 0, len = sels.length; i < len; i++) {
            var rowIdx = g.getStore().indexOf(sels[i]);
            this.toggleRow(rowIdx);
        }
    },

    getBodyContent : function (record, index) {
        if (!this.enableCaching) {
            return this.tpl.apply(record.data);
        }
        
        var content = this.bodyContent[record.id];
        
        if (!content) {
            content = this.tpl.apply(record.data);
            this.bodyContent[record.id] = content;
        }
        
        return content;
    },

    onMouseDown : function (e, t) {
        e.stopEvent();
        var row = e.getTarget(".x-grid3-row");
        this.toggleRow(row);
    },

    renderer : function (v, p, record) {
        p.cellAttr = 'rowspan="2"';
        return '<div class="x-grid3-row-expander">&#160;</div>';
    },

    beforeExpand : function (record, body, rowIndex) {
        if (this.fireEvent("beforeexpand", this, record, body, rowIndex) !== false) {
            if (this.singleExpand || this.component) {
                this.collapseAll();
            }
            
            if (!this.component && this.tpl && this.lazyRender && !body.expanderRendered) {
                body.innerHTML = this.getBodyContent(record, rowIndex);
                body.expanderRendered = true;
            }

            return true;
        } else {
            return false;
        }
    },

    toggleRow : function (row) {
        if (typeof row === "number") {
            row = this.grid.view.getRow(row);
        }
        
        this[Ext.fly(row).hasClass("x-grid3-row-collapsed") ? "expandRow" : "collapseRow"](row);
    },
    
    expandAll : function () {
        if (this.singleExpand || this.component) {
            return;
        }
        
        var i = 0;

        for (i; i < this.grid.store.getCount(); i++) {
            this.expandRow(i);
        }
    },
    
    collapseAll : function () {
        var i = 0;

        for (i; i < this.grid.store.getCount(); i++) {
            this.collapseRow(i);
        }
        this.state = {};
    },

    expandRow : function (row) {
        if (typeof row === "number") {
            row = this.grid.view.getRow(row);
        }
        
        if (Ext.isEmpty(row) || !Ext.fly(row).hasClass("x-grid3-row-collapsed")) {
            return;
        }            
        
        var record = this.grid.store.getAt(row.rowIndex),
            body = Ext.DomQuery.selectNode(this.rowBodySelector, row);
        
        if (this.beforeExpand(record, body, row.rowIndex)) {
            this.state[record.id] = true;
            Ext.fly(row).replaceClass("x-grid3-row-collapsed", "x-grid3-row-expanded");
            
            if (this.swallowBodyEvents) {
                Ext.fly(body).swallowEvent(['click', 'mousedown', 'mouseup', 'dblclick'], true);
            }            
            
            if (this.component) {
                if (this.recreateComponent) {
                    this.component.destroy();
                    this.component = Ext.ComponentMgr.create(this.componentCfg, "panel");
                }
                
                if (this.component.rendered) {                    
                    Ext.fly(body).appendChild(this.component.getEl());
                } else {
                    this.component.render(body);
                }
                
                this.component.addClass("x-row-expander-control");
                this.component.removeClass("x-hidden");
                
                this.componentInsideGrid = true;
            }
            
            this.fireEvent("expand", this, record, body, row.rowIndex);
        }
    },

    collapseRow : function (row) {
        if (typeof row === "number") {
            row = this.grid.view.getRow(row);
        }
        
        if (Ext.isEmpty(row) || !Ext.fly(row).hasClass("x-grid3-row-expanded")) {
            return;
        } 
        
        var record = this.grid.store.getAt(row.rowIndex),
            body = Ext.DomQuery.selectNode(this.rowBodySelector, row);
        
        if (this.fireEvent("beforecollapse", this, record, body, row.rowIndex) !== false) {
            this.state[record.id] = false;
            Ext.fly(row).replaceClass("x-grid3-row-expanded", "x-grid3-row-collapsed");
            this.fireEvent("collapse", this, record, body, row.rowIndex);
        }
    },
    
    isCollapsed : function (row) {
        if (typeof row === "number") {
            row = this.grid.view.getRow(row);
        }

        return Ext.fly(row).hasClass("x-grid3-row-collapsed");
    },
    
    isExpanded : function (row) {
        if (typeof row === "number") {
            row = this.grid.view.getRow(row);
        }

        return Ext.fly(row).hasClass("x-grid3-row-expanded");
    }
});

// @source data/CheckColumn.js

Ext.grid.CheckColumn = function (config) {
    Ext.apply(this, config);
    
    if (!this.id) {
        this.id = Ext.id();
    }
    
    this.renderer = this.renderer.createDelegate(this);
};

Ext.grid.CheckColumn.prototype = {
    init : function (grid) {
        this.grid = grid;
        
        var view = grid.getView();
        
        if (view.mainBody) {
            view.mainBody.on("mousedown", this.onMouseDown, this);
        } else {
            this.grid.on("render", function () {            
                this.grid.getView().mainBody.on("mousedown", this.onMouseDown, this);
            }, this);
        }       
    },

    onMouseDown : function (e, t) {
        if (this.editable && t.className && Ext.fly(t).hasClass("x-grid3-cc-" + this.dataIndex)) {
            e.stopEvent();
            
            var rIndex = this.grid.getView().findRowIndex(t),
                dataIndex = this.dataIndex,
                record = this.grid.store.getAt(rIndex);
            
            var ev = {
                grid   : this.grid,
                record : record,
                field  : this.dataIndex,
                value  : record.data[this.dataIndex],
                row    : rIndex,
                column : this.grid.getColumnModel().findColumnIndex(this.dataIndex),
                cancel : false
            };

            if (this.grid.fireEvent("beforeedit", ev) === false || ev.cancel === true) {
                return;
            }  
                      
            ev.originalValue = ev.value;
            ev.value = !record.data[this.dataIndex];
            
            if (this.grid.fireEvent("validateedit", ev) === false || ev.cancel === true) {
                return;
            } 
            
            if (this.singleSelect) {
                this.grid.store.each(function (record, i) {
                    var value = (i === rIndex);

                    if (value !== record.get(dataIndex)) {
                        record.set(dataIndex, value);
                    }
                });
            } else {
                record.set(this.dataIndex, !record.data[this.dataIndex]);
            }
            
            this.grid.fireEvent("afteredit", ev);            
        }
    },

    renderer : function (v, p, record) {
        p.css += " x-grid3-check-col-td";
        return '<div class="x-grid3-check-col' + (v ? "-on" : "") + " x-grid3-cc-" + this.dataIndex + '">&#160;</div>';
    },
    
    destroy : function () {
        if (this.grid) {
            this.grid.getView().mainBody.un("mousedown", this.onMouseDown, this);
        }
    },
    
    getCellEditor : Ext.emptyFn
};

Ext.grid.Column.types.checkcolumn = Ext.grid.CheckColumn;

// @source data/TableGrid.js

Ext.grid.TableGrid = function (config) {
    config = config || {};
    
    Ext.apply(this, config);
    
    var cf = config.fields || [], ch = config.columns || [],
        i,
        h;

    if (config.table.isComposite) {
        if (config.table.elements.length > 0) {
            table = Ext.get(config.table.elements[0]);
        }
    } else {
        table = Ext.get(config.table);
    }

    var ct = table.insertSibling();
    
    if (!Ext.isEmpty(config.id)) {
        ct.id = config.id;
    }

    var fields = [], cols = [],
        headers = table.query("thead th");
        
    for (i = 0; i < headers.length; i++) {
        h = headers[i];
        var text = h.innerHTML,
            name = "tcol-" + i;

        fields.push(Ext.applyIf(cf[i] || {}, {
            name    : name,
            mapping : "td:nth(" + (i + 1) + ")/@innerHTML"
        }));

        cols.push(Ext.applyIf(ch[i] || {}, {
            "header"    : text,
            "dataIndex" : name,
            "width"     : h.offsetWidth,
            "tooltip"   : h.title,
            "sortable"  : true
        }));
    }

    var ds = new Ext.data.Store({
        reader : new Ext.data.XmlReader({
            record : "tbody tr"
        }, fields)
    });

    ds.loadData(table.dom);

    var cm = new Ext.grid.ColumnModel(cols);

    if (config.width || config.height) {
        ct.setSize(config.width || "auto", config.height || "auto");
    } else {
        ct.setWidth(table.getWidth());
    }

    if (config.remove !== false) {
        table.remove();
    }

    Ext.applyIf(this, {
        "ds"       : ds,
        "cm"       : cm,
        "sm"       : new Ext.grid.RowSelectionModel(),
        autoHeight : this.autoHeight,
        autoWidth  : false
    });
    
    Ext.grid.TableGrid.superclass.constructor.call(this, ct, {});
};

Ext.extend(Ext.grid.TableGrid, Ext.grid.GridPanel, {
    autoHeight : true
});

Ext.reg("tablegrid", Ext.grid.TableGrid);

// @source data/RowNumberer.js

Ext.override(Ext.grid.RowNumberer, {
    isRowNumberer : true,
    hideable      : false,
    
    renderer : function (v, p, record, rowIndex) {
        if (this.grid && this.grid.getRowExpander()) {
            p.cellAttr = 'rowspan="2"';
        }        
        
        if (this.rowspan) {
            p.cellAttr = 'rowspan="' + this.rowspan + '"';
        }

        var so = record.store.lastOptions,
            sop = so ? so.params : null;
            
        return ((sop && sop.start) ? sop.start : 0) + rowIndex + 1;
    }
});

// @source data/CheckboxSelectionModel.js

Ext.override(Ext.grid.CheckboxSelectionModel, {
    allowDeselect        : true,
    keepSelectionOnClick : "always",
    hideable             : false,

    onMouseDown: function (e, t) {
        
        
        if (e.button !== 0 || this.isLocked()) {
            return;
        }     

        if (this.checkOnly && t.className !== "x-grid3-row-checker") {
            return;
        }
        
        if (this.ignoreTargets) {
            var i = 0;

            for (i; i < this.ignoreTargets.length; i++) {
                if (e.getTarget(this.ignoreTargets[i])) {
                    return;
                }
            }
        }

        if (e.button === 0 &&
                (this.keepSelectionOnClick === "always" || t.className === "x-grid3-row-checker") &&
                t.className !== "x-grid3-row-expander" &&
                !Ext.fly(t).hasClass("x-grid3-td-expander")) {

            e.stopEvent();
            var row = e.getTarget(".x-grid3-row"),
                index;

            if (!row) {
                return;
            }

            index = row.rowIndex;

            if (this.keepSelectionOnClick === "withctrlkey") {
                if (this.isSelected(index)) {
                    this.deselectRow(index);
                } else {
                    this.selectRow(index, true);
                }
            } else {
                if (this.isSelected(index)) {
                    if (!this.grid.enableDragDrop) {
                        if (this.allowDeselect === false) {
                            return;
                        }

                        this.deselectRow(index);
                    } else {
                        this.deselectingFlag = true;
                    }
                } else {
                    if (this.grid.enableDragDrop) {
                        this.deselectingFlag = false;
                    }

                    this.selectRow(index, true);
                }
            }
        }
    },

    uncheckHeader: function () {
        var view = this.grid.getView(),
            t = Ext.fly(view.innerHd).child(".x-grid3-hd-checker"),
            isChecked = t.hasClass("x-grid3-hd-checker-on");

        if (isChecked) {
            t.removeClass("x-grid3-hd-checker-on");
        }
    },

    toggleHeader: function () {
        var view = this.grid.getView(),
            t = Ext.fly(view.innerHd).child(".x-grid3-hd-checker"),
            isChecked = t.hasClass("x-grid3-hd-checker-on");

        if (isChecked) {
            t.removeClass("x-grid3-hd-checker-on");
        } else {
            t.addClass("x-grid3-hd-checker-on");
        }
    },

    checkHeader: function () {
        var view = this.grid.getView(),
            t = Ext.fly(view.innerHd).child(".x-grid3-hd-checker"),
            isChecked = t.hasClass("x-grid3-hd-checker-on");

        if (!isChecked) {
            t.addClass("x-grid3-hd-checker-on");
        }
    },

    renderer: function (v, p, record) {
        if (this.grid && this.grid.getRowExpander()) {
            p.cellAttr = 'rowspan="2"';
        }

        if (this.grid) {
            var rowSpan = this.grid.getSelectionModel().rowSpan;
        
            if (rowSpan > 1) {
                p.cellAttr = 'rowspan="' + rowSpan + '"';
            }
        }
        
        return '<div class="x-grid3-row-checker">&#160;</div>';
    },

    onHdMouseDown: function (e, t) {
        if (t.className === "x-grid3-hd-checker") {
            e.stopEvent();
            var hd = Ext.fly(t.parentNode);
            var isChecked = hd.hasClass("x-grid3-hd-checker-on");

            if (this.fireEvent("beforecheckallclick", this, isChecked) === false) {
                return;
            }

            if (isChecked) {
                hd.removeClass("x-grid3-hd-checker-on");
                this.clearSelections();
            } else {
                hd.addClass("x-grid3-hd-checker-on");
                this.selectAll();
            }

            this.fireEvent("aftercheckallclick", this, !isChecked);
        }
    },

    isCheckAllChecked: function () {
        return Ext.fly(this.grid.getView().innerHd).child(".x-grid3-hd-checker").hasClass("x-grid3-hd-checker-on");
    },
    
    handleMouseDown : function (g, rowIndex, e) {
        this.onMouseDown(e, e.getTarget());
    }
});

Ext.grid.CheckboxSelectionModel.prototype.initEvents = Ext.grid.CheckboxSelectionModel.prototype.initEvents.createSequence(function () {
    if ((this.grid.enableDragDrop || this.grid.enableDrag) && this.checkOnly) {
        this.handleMouseDown = function (g, rowIndex, e) {
            this.onMouseDown(e, e.getTarget());
        };
    }
    
    this.grid.on("rowclick", function (grid, rowIndex, e) {
        if (this.deselectingFlag && this.grid.enableDragDrop) {
            this.deselectingFlag = false;
            this.deselectRow(rowIndex);
        }
    }, this);

    this.on("rowdeselect", function () {
        this.uncheckHeader();
    });

    this.on("rowselect", function () {
        if (this.grid.store.getCount() === this.getSelections().length) {
            this.checkHeader();
        }
    });
    
    this.renderer = this.renderer.createDelegate(this);
});

// @source data/ColumnModel.js

Ext.grid.ColumnModel.override({
    defaultSortable: true, 
    
    isMenuDisabled : function (col) {
        var column = this.config[col];
        
        if (Ext.isEmpty(column)) {
            return true;
        }
        
        return !!column.menuDisabled;
    },
    
    isSortable : function (col) {
        var column = this.config[col];
        
        if (Ext.isEmpty(column)) {
            return false;
        }
    
        if (typeof this.config[col].sortable === "undefined") {
            return this.defaultSortable;
        }
        
        return this.config[col].sortable;
    },
    
    isHidden : function (colIndex) {        
        return colIndex >= 0 && this.config[colIndex].hidden;
    },

    isFixed : function (colIndex) {
        return colIndex >= 0 && this.config[colIndex].fixed;
    },
    
    setState : function (col, state) {
        state = Ext.applyIf(state, this.defaults);
        Ext.apply(this.lookup[col], state);
    }
});

Ext.grid.Column.override({
    forbidIdScoping : true
});

// @source data/GridView.js

Ext.grid.GridView.prototype.initEvents = Ext.grid.GridView.prototype.initEvents.createSequence(function () {
    this.addEvents("afterRender", "beforerowupdate");
});

Ext.grid.GridView.prototype.afterRender = Ext.grid.GridView.prototype.afterRender.createSequence(function () {
    this.fireEvent("afterRender", this);
});

Ext.grid.GridView.override({
    getCell: function (row, col) {
        var tds = this.getRow(row).getElementsByTagName("td"),
            ind = -1,
            i = 0;

        if (tds) {
            for (i; i < tds.length; i++) {
                if (Ext.fly(tds[i]).hasClass("x-grid3-col x-grid3-cell")) {
                    ind++;

                    if (ind === col) {
                        return tds[i];
                    }
                }
            }
        }
        return tds[col];
    },

    getColumnData: function () {
        var cs = [], 
            cm = this.cm, 
            colCount = cm.getColumnCount(),
            i = 0;
        
        for (i; i < colCount; i++) {
            var name = cm.getDataIndex(i);

            cs[i] = {
                name     : (!Ext.isDefined(name) ? this.ds.fields.get(i).name : name),
                renderer : cm.getRenderer(i),
                scope    : cm.getRendererScope(i),
                id       : cm.getColumnId(i),
                style    : this.getColumnStyle(i)
            };
            
            if (cs[i].scope && !cs[i].scope.grid) {
                cs[i].scope.grid = this.grid;
            }
        }
        return cs;
    },
    
    onUpdate : function (store, record) {
        this.fireEvent("beforerowupdate", this, store.indexOf(record), record);
        this.refreshRow(record);
    }
});

Ext.grid.HeaderDragZone.override({
    onBeforeDrag : function (data, e) {        
        return !e.getTarget(".x-grid3-add-row", 50);
    }
});

// @source data/DataView.js

Ext.DataView.prototype.initComponent = Ext.DataView.prototype.initComponent.createSequence(function () {
    this.initSelection();    
    
    if (this.store) {
        if (this.store.getCount() > 0) {
            this.on("render", this.doSelection, this, { single : true, delay : 100 });
        } else {
            this.store.on("load", this.doSelection, this, { single : true, delay : 100 });
        }
    }
});

Ext.DataView.prototype.onRender = Ext.DataView.prototype.onRender.createSequence(function () {
    this.getSelectionField().render(this.el.parent() || this.el);
});

Ext.DataView.override({

    getSelectionField : function () {
        if (!this.selectionField) {
            this.selectionField = new Ext.form.Hidden({ id: this.id + "_SN", name: this.id + "_SN" });
			this.on("beforedestroy", function () { 
                if (this.rendered) {
                    this.destroy();
                }
            }, this.selectionField);
        }

        return this.selectionField;
    },

    updateSelection : function () {
        var records = [];

        var selectedRecords = this.getSelectedRecords(),
            i = 0;

        for (i; i < selectedRecords.length; i++) {
            if (!Ext.isEmpty(selectedRecords[i])) {
                records.push({ RecordID: selectedRecords[i].id, RowIndex: this.store.indexOfId(selectedRecords[i].id) });
            }
        }

        this.hSelField.setValue(Ext.encode(records));
    },

    doSelection : function () {
        var data = this.selectedData,
            silent = true;

        if (!Ext.isEmpty(this.fireSelectOnLoad)) {
            silent = !this.fireSelectOnLoad;
        }

        if (!Ext.isEmpty(data)) {
            if (silent) {
                this.suspendEvents();
            }

            if (data.length > 0) {
                var indexes = [],
                    record,
                    i = 0;

                for (i; i < data.length; i++) {
                    if (!Ext.isEmpty(data[i].recordID)) {
                        record = this.store.getById(data[i].recordID);
                        
                        if (!Ext.isEmpty(record)) {
                            indexes.push(this.store.indexOf(record));
                        }
                    } else if (!Ext.isEmpty(data[i].rowIndex)) {
                        indexes.push(data[i].rowIndex);
                    }
                }
                this.select(indexes);

                if (silent) {
                    this.updateSelection();
                }
            }

            if (silent) {
                this.resumeEvents();
            }
        }
    },

    initSelection : function () {
        this.hSelField = this.getSelectionField();
        this.on("selectionchange", this.updateSelection, this);
    },

    getRowsValues : function (selectedOnly) {
        if (Ext.isEmpty(selectedOnly)) {
            selectedOnly = true;
        }

        var records = (selectedOnly ? this.getSelectedRecords() : this.store.getRange()) || [],
            values = [],
            i;

        for (i = 0; i < records.length; i++) {
            if (Ext.isEmpty(records[i])) {
                continue;
            }
            
            var obj = {}, dataR;

            if (this.store.metaId()) {
                obj[this.store.metaId()] = records[i].id;
            }

            dataR = Ext.apply(obj, records[i].data);
            dataR = this.store.prepareRecord(dataR, records[i], {});

            if (!Ext.isEmptyObj(dataR)) {
                values.push(dataR);
            }
        }

        return values;
    },

    submitData : function (selectedOnly) {
        this.store.submitData(this.getRowsValues(selectedOnly || false));
    }
});

// @source data/ListView.js

Ext.ListView.prototype.onResize = Ext.ListView.prototype.onResize.createSequence(function (w, h) {
    if (Ext.isNumber(h)) {
        this.innerBody.dom.parentNode.style.height = (h - this.innerHd.dom.parentNode.offsetHeight) + "px";
    }
});

Ext.ListView.override({
    //column can be index or dataindex
    setColumnHeader : function (column, header) {
        if (Ext.isString(column)) {
            Ext.each(this.columns, function (c, i) {
                if (c.dataIndex === column) {
                    column = i;
                    return false;
                }
            }, this);
        }
        column++;
        Ext.fly(this.id + "-xlhd-" + column).update(header);
    }
});

// @source data/CommandColumn.js

//Ext.grid.GridView.prototype.refreshRow = Ext.grid.GridView.prototype.refreshRow.createInterceptor(function (record) {
//    this.fireEvent("beforerowupdate", this, this.grid.store.indexOf(record), record);
//});

Ext.net.CommandColumn = function (config) {
    Ext.apply(this, config);
    
    if (!this.id) {
        this.id = Ext.id();
    }

    Ext.net.CommandColumn.superclass.constructor.call(this); 
};

Ext.extend(Ext.net.CommandColumn, Ext.util.Observable, {
    dataIndex    : "",
    header       : "",
    menuDisabled : true,
    sortable     : false,
    autoWidth    : false,

    init : function (grid) {
        this.grid = grid;

        var view = this.grid.getView(),
            func;

        view.rowSelectorDepth = 100;

        this.cache = [];

//        if (Ext.isEmpty(view.events) || Ext.isEmpty(view.events.beforerowupdate)) {
//            view.addEvents("beforerowupdate");
//        }
        
        this.commands = this.commands || [];

        if (this.commands) {
            this.shareMenus(this.commands, "initMenu");
            
            func = function () {
                this.insertToolbars();
                view.on("beforerefresh", this.removeToolbars, this);
                view.on("refresh", this.insertToolbars, this);
            };

            if (this.grid.rendered) {
                func.call(this);
            } else {
                this.grid.on("viewready", func, this);
            }

            view.on("beforerowupdate", this.removeToolbar, this);
            view.on("beforerowremoved", this.removeToolbar, this);
            view.on("rowsinserted", this.insertToolbar, this);
            view.on("rowupdated", this.rowUpdated, this);
        }

        var sm = grid.getSelectionModel();
        
        if (sm.id === "checker") {
            sm.onMouseDown = sm.onMouseDown.createInterceptor(this.onMouseDown, this);
        } else if (sm.selectRows) {
            sm.handleMouseDown = sm.handleMouseDown.createInterceptor(this.rmHandleMouseDown, this);
        } else {
            sm.handleMouseDown = sm.handleMouseDown.createInterceptor(this.handleMouseDown, this);
        }

        if (view.groupTextTpl && this.groupCommands) {
            this.shareMenus(this.groupCommands, "initGroupMenu");
            func = function () {
                this.insertGroupToolbars();
                view.on("beforerefresh", this.removeToolbars, this);
                view.on("refresh", this.insertGroupToolbars, this);
            };

            if (view.groupTextTpl && this.groupCommands) {
                view.groupTextTpl = '<div class="standart-view-group">' + view.groupTextTpl + '</div>';
            }

            if (this.grid.rendered) {
                func.call(this);
            } else {
                view.on("afterRender", func, this);
            }

            view.processEvent = view.processEvent.createInterceptor(this.interceptMouse, this);
        }
    },

    onMouseDown : function (e, t) {
        return this.interceptMouse("mousedown", e);
    },

    rmHandleMouseDown : function (g, rowIndex, e) {
        return this.interceptMouse("mousedown", e);
    },

    handleMouseDown : function (g, row, cell, e) {
        return this.interceptMouse("mousedown", e);
    },

    interceptMouse : function (name, e) {
        if (name !== "mousedown") {
            return;
        }
        
        var tb = e.getTarget('.x-toolbar', this.grid.view.mainBody);
        
        if (tb) {
            e.stopEvent();
            return false;
        }
    },

    renderer : function (value, meta, record, row, col, store) {
        meta.css = "row-cmd-cell";
        return "";
    },

    insertToolbar : function (view, firstRow, lastRow, row) {
        this.insertToolbars(firstRow, lastRow + 1, row);
    },

    rowUpdated : function (view, firstRow, record) {
        this.insertToolbars(firstRow, firstRow + 1);
    },

    select : function (row) {
        var classSelector = "x-grid3-td-" + this.id + ".row-cmd-cell",
            el = row ? Ext.fly(row) : this.grid.getEl();
        return el.query("td." + classSelector);
    },

    selectGroups : function () {
        return this.grid.getEl().query("div.x-grid-group div.x-grid-group-hd");
    },

    shareMenus : function (items, initMenu) {
        Ext.each(items, function (item) {
            if (item.menu) {
                if (item.menu.shared) {
                    item.menu.autoDestroy = false;

                    item.onMenuShow = Ext.emptyFn;

                    item.showMenu = function () {
                        if (this.rendered && this.menu) {
                            if (this.tooltip) {
                                Ext.QuickTips.getQuickTip().cancelShow(this.btnEl);
                            }
                            this.menu.show(this.el, this.menuAlign);

                            this.menu.ownerCt = this;
                            this.ignoreNextClick = 0;
                            this.el.addClass('x-btn-menu-active');
                            this.fireEvent('menushow', this, this.menu);
                        }
                        return this;
                    };

                    item.menu = Ext.ComponentMgr.create(item.menu, "menu");
                    this[initMenu](item.menu, null, true);
                } else {
                    this.shareMenus(item.menu.items || []);
                }
            }
        }, this);
    },

    insertGroupToolbars : function () {
        var groupCmd = this.selectGroups(),
            i;

        if (this.groupCommands) {
            for (i = 0; i < groupCmd.length; i++) {
                var toolbar = new Ext.Toolbar({
                    items: this.groupCommands,
                    enableOverflow: false
                }),
                    div = Ext.fly(groupCmd[i]).first("div");

                this.cache.push(toolbar);

                div.addClass("row-cmd-cell-ct");
                toolbar.render(div);

                var group = this.grid.view.findGroup(div),
                    groupId = group ? group.id.replace(/ext-gen[0-9]+-gp-/, "") : null,
                    records = this.getRecords(group.id);

                if (this.prepareGroupToolbar && this.prepareGroupToolbar(this.grid, toolbar, groupId, records) === false) {
                    toolbar.destroy();
                    continue;
                }

                toolbar.grid = this.grid;
                toolbar.groupId = groupId;
                toolbar._groupId = group.id;

                toolbar.items.each(function (button) {
                    if (button.on) {
                        button.toolbar = toolbar;
                        button.column = this;

                        if (button.standOut) {
                            button.on("mouseout", function () {
                                this.getEl().addClass("x-btn-over");
                            }, button);
                        }

                        if (!Ext.isEmpty(button.command, false)) {
                            button.on("click", function () {
                                this.toolbar.grid.fireEvent("groupcommand", this.command, this.toolbar.groupId, this.column.getRecords.apply(this.column, [this.toolbar._groupId]));
                            }, button);
                        }

                        if (button.menu && !button.menu.shared) {
                            this.initGroupMenu(button.menu, toolbar);
                        }
                    }
                }, this);
            }
        }
    },

    initGroupMenu : function (menu, toolbar, shared) {
        menu.items.each(function (item) {
            if (item.on) {
                item.toolbar = toolbar;
                item.column = this;

                if (!Ext.isEmpty(item.command, false)) {

                    if (shared) {
                        item.on("click", function () {
                            var pm = this.parentMenu;
                            
                            while (pm && !pm.shared) {
                                pm = pm.parentMenu;
                            }
                            
                            if (pm && pm.shared && pm.ownerCt && pm.ownerCt.toolbar) {
                                var toolbar = pm.ownerCt.toolbar;
                                toolbar.grid.fireEvent("groupcommand", this.command, toolbar.groupId, this.column.getRecords.apply(this.column, [toolbar._groupId]));
                            }
                        }, item);
                        item.getGroup = function () {
                            var pm = this.parentMenu;
                            
                            while (pm && !pm.shared) {
                                pm = pm.parentMenu;
                            }
                            
                            if (pm && pm.shared && pm.ownerCt && pm.ownerCt.toolbar) {
                                var toolbar = pm.ownerCt.toolbar;
                            
                                return {
                                    groupId: toolbar.groupId,
                                    records: this.column.getRecords.apply(this.column, [toolbar._groupId])
                                };
                            }
                        };
                    } else {
                        item.getGroup = function () {
                            return {
                                groupId: this.toolbar.groupId,
                                records: this.column.getRecords.apply(this.column, [this.toolbar._groupId])
                            };
                        };
                        item.on("click", function () {
                            this.toolbar.grid.fireEvent("groupcommand", this.command, this.toolbar.groupId, this.column.getRecords.apply(this.column, [this.toolbar._groupId]));
                        }, item);
                    }
                }

                if (item.menu) {
                    this.initGroupMenu(item.menu, toolbar, shared);
                }
            }
        }, this);
    },

    getRecords : function (groupId) {
        if (groupId) {
            var records = this.grid.store.queryBy(function (r) {
                    return r._groupId === groupId;
                });

            return records ? records.items : [];
        }
    },

    getAllGroupToolbars : function () {
        var groups = this.selectGroups(),
            toolbars = [],
            i;

        for (i = 0; i < groups.length; i++) {
            var div = Ext.fly(groups[i]).first("div"),
                el = div.last();

            if (!Ext.isEmpty(el)) {
                var cmp = Ext.getCmp(el.id);
                toolbars.push(cmp);
            }
        }

        return toolbars;
    },

    getGroupToolbar : function (groupId) {
        var groups = this.selectGroups(),
            i;

        for (i = 0; i < groups.length; i++) {
            var div = Ext.fly(groups[i]).first("div"),
                _group = this.grid.view.findGroup(div),
                _groupId = _group ? _group.id.replace(/ext-gen[0-9]+-gp-/, "") : null;

            if (_groupId === groupId) {
                var el = div.last();

                if (!Ext.isEmpty(el)) {
                    var cmp = Ext.getCmp(el.id);
                    return cmp;
                }
            }
        }

        return undefined;
    },

    insertToolbars : function (start, end, row) {
        var tdCmd = this.select(),
            width = 0;

        if (Ext.isEmpty(start) || Ext.isEmpty(end)) {
            start = 0;
            end = tdCmd.length;
        }

        if (this.commands) {
            var i = start;

            for (i; i < end; i++) {

                var toolbar = new Ext.Toolbar({
                    items          : this.commands,
                    enableOverflow : false,
                    buttonAlign    : this.buttonAlign
                }),
                    div;

                if (row) {
                    div = Ext.fly(this.select(row)[0]).first("div");
                } else {
                    div = Ext.fly(tdCmd[i]).first("div");
                }

                this.cache.push(toolbar);

                div.dom.innerHTML = "";
                div.addClass("row-cmd-cell-ct");

                toolbar.render(div);

                var record = this.grid.store.getAt(i);
                toolbar.record = record;

                if (this.prepareToolbar && this.prepareToolbar(this.grid, toolbar, i, record) === false) {
                    toolbar.destroy();
                    continue;
                }

                toolbar.grid = this.grid;
                toolbar.rowIndex = i;
                toolbar.record = record;

                toolbar.items.each(function (button) {
                    if (button.on) {
                        button.toolbar = toolbar;

                        if (button.standOut) {
                            button.on("mouseout", function () {
                                this.getEl().addClass("x-btn-over");
                            }, button);
                        }

                        if (!Ext.isEmpty(button.command, false)) {
                            button.on("click", function () {
                                this.toolbar.grid.fireEvent("command", this.command, this.toolbar.record, this.toolbar.record.store.indexOf(this.toolbar.record));
                            }, button);
                        }

                        if (button.menu && !button.menu.shared) {
                            this.initMenu(button.menu, toolbar);
                        }
                    }
                }, this);

                if (this.autoWidth) {
                    var tbTable = toolbar.getEl().first("table"),
                        tbWidth = tbTable.getComputedWidth();

                    width = tbWidth > width ? tbWidth : width;
                }
            }

            if (this.autoWidth && width > 0) {
                var cm = this.grid.getColumnModel();
                cm.setColumnWidth(cm.getIndexById(this.id), width + 4);
                this.grid.view.autoExpand();
            }
            
            if (this.grid.view.syncRows) {
                this.grid.view.syncRows(start);
            }
        }
    },

    initMenu : function (menu, toolbar, shared) {
        menu.items.each(function (item) {
            if (item.on) {
                item.toolbar = toolbar;

                if (shared) {
                    item.on("click", function () {
                        var pm = this.parentMenu;
                        while (pm && !pm.shared) {
                            pm = pm.parentMenu;
                        }
                        if (pm && pm.shared && pm.ownerCt && pm.ownerCt.toolbar) {
                            var toolbar = pm.ownerCt.toolbar;
                            toolbar.grid.fireEvent("command", this.command, toolbar.record, toolbar.record.store.indexOf(toolbar.record));
                        }
                    }, item);

                    item.getRecord = function () {
                        var pm = this.parentMenu;
                        while (pm && !pm.shared) {
                            pm = pm.parentMenu;
                        }
                        if (pm && pm.shared && pm.ownerCt && pm.ownerCt.toolbar) {
                            var toolbar = pm.ownerCt.toolbar;
                            return toolbar.record;
                        }
                    };
                } else {
                    if (!Ext.isEmpty(item.command, false)) {
                        item.on("click", function () {
                            this.toolbar.grid.fireEvent("command", this.command, this.toolbar.record, this.toolbar.rowIndex);
                        }, item);
                        
                        item.getRecord = function () {
                            return this.toolbar.record;
                        };
                    }
                }

                if (item.menu) {
                    this.initMenu(item.menu, toolbar, shared);
                }
            }
        }, this);
    },

    removeToolbar : function (view, rowIndex, record) {
        var i,
            l;

        for (i = 0, l = this.cache.length; i < l; i++) {
            if (this.cache[i].record && (this.cache[i].record.id === record.id)) {
                try {
                    this.cache[i].destroy();
                    this.cache.remove(this.cache[i]);
                } catch (ex) { }
                break;
            }
        }
    },

    removeToolbars : function () {
        var i,
            l;

        for (i = 0, l = this.cache.length; i < l; i++) {
            try {
                this.cache[i].destroy();
            } catch (ex) { }
        }

        this.cache = [];
    },

    getToolbar: function (rowIndex) {
        var tdCmd = this.select(),
            div = Ext.fly(tdCmd[rowIndex]).first("div"),
            el = div.first();

        if (!Ext.isEmpty(el)) {
            var cmp = Ext.getCmp(el.id);

            return cmp;
        }

        return undefined;
    },

    getAllToolbars : function () {
        var tdCmd = this.select(),
            toolbars = [],
            i = 0;

        for (i; i < tdCmd.length; i++) {
            var div = Ext.fly(tdCmd[i]).first("div"),
                el = div.first();

            if (!Ext.isEmpty(el)) {
                var cmp = Ext.getCmp(el.id);
                toolbars.push(cmp);
            }
        }

        return toolbars;
    },

    destroy : function () {
        var view = this.grid.getView();
        
        this.removeToolbars();
        view.un("refresh", this.insertToolbars, this);
        view.un("beforerowupdate", this.removeToolbar, this);
        view.un("beforerefresh", this.removeToolbars, this);
        view.un("beforerowremoved", this.removeToolbar, this);
        view.un("rowsinserted", this.insertToolbar, this);
        view.un("rowupdated", this.rowUpdated, this);
        view.un("refresh", this.insertGroupToolbars, this);
    }
});

// @source data/ImageCommandColumn.js

Ext.net.ImageCommandColumn = function (config) {
    Ext.apply(this, config);
    
    if (!this.id) {
        this.id = Ext.id();
    }

    //this.renderer = this.renderer.createDelegate(this);

    Ext.net.ImageCommandColumn.superclass.constructor.call(this);    
};

Ext.extend(Ext.net.ImageCommandColumn, Ext.util.Observable, {
    dataIndex    : "",
    header       : "",
    menuDisabled : true,
    sortable     : false,

    init : function (grid) {
        this.grid = grid;

        var view = this.grid.getView();
        
        if (this.grid.rendered) {
            view.mainBody.on("click", this.onClick, this);

            if (view.lockedBody) {
                view.lockedBody.on("click", this.onClick, this);
            }
        } else {
            this.grid.afterRender = grid.afterRender.createSequence(function () {
                view.mainBody.on("click", this.onClick, this);

                if (view.lockedBody) {
                    view.lockedBody.on("click", this.onClick, this);
                }
            }, this);
        }
        
        var sm = grid.getSelectionModel();
        
        this.commands = this.commands || [];
        
        if (sm.id === "checker") {
            sm.onMouseDown = sm.onMouseDown.createInterceptor(this.onMouseDown, this);
        } else if (sm.selectRows) {       
            sm.handleMouseDown = sm.handleMouseDown.createInterceptor(this.rmHandleMouseDown, this);
        } else {
            sm.handleMouseDown = sm.handleMouseDown.createInterceptor(this.handleMouseDown, this);
        }
        
        this.renderer = this.renderer.createDelegate(this);

        if (view.groupTextTpl && this.groupCommands) {
            view.processEvent = view.processEvent.createInterceptor(function (name, e) {
                if (name === "mousedown" && e.getTarget(".group-row-imagecommand")) {
                    return false;
                }
            });

            view.doGroupStart = view.doGroupStart.createInterceptor(function (buf, group, cs, store, colCount) {
                var preparedCommands = [], 
                    i,
                    groupCommands = this.commandColumn.groupCommands;
                    
                group.cls = (group.cls || "") + " group-imagecmd-ct";
                
                var groupId = group ? group.groupId.replace(/ext-gen[0-9]+-gp-/, "") : null;
                
                if (this.commandColumn.prepareGroupCommands) {  
                    groupCommands = Ext.net.clone(this.commandColumn.groupCommands);
                    this.commandColumn.prepareGroupCommands(this.grid, groupCommands, groupId, group);
                }
                
                for (i = 0; i < groupCommands.length; i++) {
                    var cmd = groupCommands[i];
                    
                    cmd.tooltip = cmd.tooltip || {};
                    
                    var command = {
                        command    : cmd.command,
                        cls        : cmd.cls,
                        iconCls    : cmd.iconCls,
                        hidden     : cmd.hidden,
                        text       : cmd.text,
                        style      : cmd.style,
                        qtext      : cmd.tooltip.text,
                        qtitle     : cmd.tooltip.title,
                        hideMode   : cmd.hideMode,
                        rightAlign : cmd.rightAlign || false
                    };                  
                    
                    if (this.commandColumn.prepareGroupCommand) {
                        this.commandColumn.prepareGroupCommand(this.grid, command, groupId, group);
                    }

                    if (command.hidden) {
                        var hideMode = command.hideMode || "display";
                        command.hideCls = "x-hide-" + hideMode;
                    }

                    if (command.rightAlign) {
                        command.align = "right-group-imagecommand";
                    } else {
                        command.align = "";
                    }

                    preparedCommands.push(command);
                }
                group.commands = preparedCommands;
            });
            
            view.groupTextTpl = '<div class="group-row-imagecommand-cell">' + view.groupTextTpl + '</div>' + this.groupCommandTemplate;
            view.commandColumn = this;
            view.processEvent  = view.processEvent.createInterceptor(this.interceptMouse, this);
        }
    },
    
    onMouseDown : function (e, t) {
        return this.interceptMouse("mousedown", e);
    },
    
    rmHandleMouseDown : function (g, rowIndex, e) {
        return this.interceptMouse("mousedown", e);
    },
    
    handleMouseDown : function (g, row, cell, e) {
        return this.interceptMouse("mousedown", e);
    },
    
    interceptMouse : function (name, e) {
        if ((name === "mousedown" && e.getTarget(".group-row-imagecommand", this.grid.view.mainBody)) ||
                e.getTarget(".row-imagecommand ", this.grid.view.mainBody)) {
            e.stopEvent();
            return false;
        }
    },

    renderer : function (value, meta, record, row, col, store) {
        meta.css = meta.css || "";
        meta.css += " row-imagecommand-cell";

        if (this.commands) {
            var preparedCommands = [],
                commands = this.commands;
            
            if (this.prepareCommands) {                
                commands = Ext.net.clone(this.commands);
                this.prepareCommands(this.grid, commands, record, row);
            }
            
            var i = 0;
                        
            for (i; i < commands.length; i++) {
                var cmd = commands[i];
                
                cmd.tooltip = cmd.tooltip || {};
                
                var command = {
                    command  : cmd.command,
                    cls      : cmd.cls,
                    iconCls  : cmd.iconCls,
                    hidden   : cmd.hidden,
                    text     : cmd.text,
                    style    : cmd.style,
                    qtext    : cmd.tooltip.text,
                    qtitle   : cmd.tooltip.title,
                    hideMode : cmd.hideMode
                };                
                
                if (this.prepareCommand) {
                    this.prepareCommand(this.grid, command, record, row);
                }

                if (command.hidden) {
                    var hideMode = command.hideMode || "display";
                    command.hideCls = "x-hide-" + hideMode;
                }
                
                if (Ext.isIE6 && Ext.isEmpty(cmd.text, false)) {
                    command.noTextCls = "no-row-imagecommand-text";
                }

                preparedCommands.push(command);
            }
            
            return this.getRowTemplate().apply({ commands : preparedCommands });
        }
        return "";
    },

    commandTemplate :
		'<div class="row-imagecommands">' +
		  '<tpl for="commands">' +
		     '<div cmd="{command}" class="row-imagecommand {cls} {noTextCls} {iconCls} {hideCls}" ' +
		     'style="{style}" ext:qtip="{qtext}" ext:qtitle="{qtitle}">' +
		        '<tpl if="text"><span ext:qtip="{qtext}" ext:qtitle="{qtitle}">{text}</span></tpl>' +
		     '</div>' +
		  '</tpl>' +
		'</div>',

    groupCommandTemplate :
		 '<tpl for="commands">' +
		    '<div cmd="{command}" class="group-row-imagecommand {cls} {iconCls} {hideCls} {align}" ' +
		      'style="{style}" ext:qtip="{qtext}" ext:qtitle="{qtitle}"><span ext:qtip="{qtext}" ext:qtitle="{qtitle}">{text}</span></div>' +
		 '</tpl>',

    getRowTemplate : function () {
        if (Ext.isEmpty(this.rowTemplate)) {
            this.rowTemplate = new Ext.XTemplate(this.commandTemplate);
        }

        return this.rowTemplate;
    },

    onClick : function (e, target) {
        var view = this.grid.getView(), 
            cmd,
            t = e.getTarget(".row-imagecommand");
            
        if (t) {
            cmd = Ext.fly(t).getAttributeNS("", "cmd");
            
            if (Ext.isEmpty(cmd, false)) {
                return;
            }
            
            var row = e.getTarget(".x-grid3-row");
            
            if (row === false) {
                return;
            }
            
            var colIndex = this.grid.view.findCellIndex(target.parentNode.parentNode);
            
            if (colIndex !== this.grid.getColumnModel().getIndexById(this.id)) {
                return;
            }

            this.grid.fireEvent("command", cmd, this.grid.store.getAt(row.rowIndex), row.rowIndex, colIndex);
        }

        t = e.getTarget(".group-row-imagecommand");
        
        if (t) {
            var group = view.findGroup(target),
                groupId = group ? group.id.replace(/ext-gen[0-9]+-gp-/, "") : null;
                
            cmd = Ext.fly(t).getAttributeNS("", "cmd");
            
            if (Ext.isEmpty(cmd, false)) {
                return;
            }

            this.grid.fireEvent("groupcommand", cmd, groupId, this.getRecords(group.id));
        }
    },

    getRecords : function (groupId) {
        if (groupId) {
            var records = this.grid.store.queryBy(function (record) {
                    return record._groupId === groupId;
                });
                
            return records ? records.items : [];
        }
        
        return [];
    },
    
    destroy : function () {
        this.grid.getView().mainBody.un("click", this.onClick, this);
        if (this.grid.getView().lockedBody) {
            this.grid.getView().lockedBody.un("click", this.onClick, this);
        }
    }
});

// @source data/CellCommands.js

Ext.net.CellCommands = function (config) {
    Ext.apply(this, config);
    Ext.net.CellCommands.superclass.constructor.call(this);    
};

Ext.extend(Ext.net.CellCommands, Ext.util.Observable, {
    commandTemplate :
		'<div class="cell-imagecommands <tpl if="rightValue === true">cell-imagecommand-right-value</tpl>">' +
		  '<tpl if="rightAlign === true && rightValue === false"><div class="cell-imagecommand-value">{value}</div></tpl>' +
		  '<tpl for="commands">' +
		     '<div cmd="{command}" class="cell-imagecommand <tpl if="parent.rightAlign === false">left-cell-imagecommand</tpl> {cls} {iconCls} {hideCls}" ' +
		     'style="{style}" ext:qtip="{qtext}" ext:qtitle="{qtitle}">' +
		        '<tpl if="text"><span ext:qtip="{qtext}" ext:qtitle="{qtitle}">{text}</span></tpl>' +
		     '</div>' +
		  '</tpl>' +
		  '<tpl if="rightAlign === false || rightValue === true"><div class="cell-imagecommand-value">{value}</div></tpl>' +
		'</div>',

    getTemplate : function () {
        if (Ext.isEmpty(this.template)) {
            this.template = new Ext.XTemplate(this.commandTemplate);
        }

        return this.template;
    },

    init : function (grid) {
        this.grid = grid;

        var view = this.grid.getView();
        
        var sm = grid.getSelectionModel();
        
        if (sm.id === "checker") {
            sm.onMouseDown = sm.onMouseDown.createInterceptor(this.onMouseDown, this);
        } else if (sm.selectRows) {       
            sm.handleMouseDown = sm.handleMouseDown.createInterceptor(this.rmHandleMouseDown, this);
        } else {
            sm.handleMouseDown = sm.handleMouseDown.createInterceptor(this.handleMouseDown, this);
        }
        
        this.grid.afterRender = grid.afterRender.createSequence(function () {
            view.mainBody.on("click", this.onClick, this);

            if (view.lockedBody) {
                view.lockedBody.on("click", this.onClick, this);
            }
        }, this);

        var cm = this.grid.getColumnModel(),
            i;
        
        for (i = 0; i < cm.config.length; i++) {
            var column = cm.config[i];
            
            if (!column.expandRow) {
                column.userRenderer = cm.getRenderer(i);
                column.renderer = this.renderer.createDelegate(this);
            }
        }
    },
    
    onMouseDown : function (e, t) {
        return this.interceptMouse(e);
    },
    
    rmHandleMouseDown : function (g, rowIndex, e) {
        return this.interceptMouse(e);
    },
    
    handleMouseDown : function (g, row, cell, e) {
        return this.interceptMouse(e);
    },
    
    interceptMouse : function (e) {
        if (e.getTarget('.cell-imagecommand ', this.grid.view.mainBody)) {
            e.stopEvent();
            return false;
        }
    },

    renderer : function (value, meta, record, row, col, store) {
        var column = this.grid.getColumnModel().config[col];

        if (column.commands && column.commands.length > 0 && column.isCellCommand) {
            var rightAlign = column.rightCommandAlign === false ? false : true,
                preparedCommands = [],
                commands = column.commands;
                
            if (column.prepareCommands) {                
                commands = Ext.net.clone(column.commands);
                column.prepareCommands(this.grid, commands, record, row, col, value);
            }
            
            var i = rightAlign ? (commands.length - 1) : 0;
                            
            for (i; rightAlign ? (i >= 0) : (i < commands.length); rightAlign ? i-- : i++) {
                var cmd = commands[i];
                
                cmd.tooltip = cmd.tooltip || {};
                
                var command = {
                    command  : cmd.command,
                    cls      : cmd.cls,
                    iconCls  : cmd.iconCls,
                    hidden   : cmd.hidden,
                    text     : cmd.text,
                    style    : cmd.style,
                    qtext    : cmd.tooltip.text,
                    qtitle   : cmd.tooltip.title,
                    hideMode : cmd.hideMode
                };

                if (column.prepareCommand) {
                    column.prepareCommand(this.grid, command, record, row, col, value);
                }

                if (command.hidden) {
                    command.hideCls = "x-hide-" + (command.hideMode || "display");
                }

                preparedCommands.push(command);
            }

            var userRendererValue = column.userRenderer(value, meta, record, row, col, store);

            return this.getTemplate().apply({
                commands   : preparedCommands,
                value      : userRendererValue,
                rightAlign : rightAlign,
                rightValue : column.align === "right"
            });
        } else {
            meta.css = meta.css || "";
            meta.css += " cell-no-imagecommand";
        }

        return column.userRenderer(value, meta, record, row, col, store);
    },

    onClick : function (e, target) {
        var view = this.grid.getView(),
            t = e.getTarget(".cell-imagecommand");

        if (t) {
            var cmd = Ext.fly(t).getAttributeNS("", "cmd");
            
            if (Ext.isEmpty(cmd, false)) {
                return;
            }
            
            var row = e.getTarget(".x-grid3-row");
            
            if (row === false) {
                return;
            }

            var col = view.findCellIndex(target.parentNode.parentNode),
                record = this.grid.store.getAt(row.rowIndex);

            this.grid.fireEvent("command", cmd, record, row.rowIndex, col);
        }
    }
});

// @source data/GridEditor.js

Ext.grid.GridEditor.override({
    setSize : function (w, h) {
        Ext.grid.GridEditor.superclass.setSize.call(this, w, h); 
        
        if (this.el) {            
            if (Ext.isIE7) {
                (function () {
                    this.el.setSize(w, h);
                    this.el.sync();
                }).defer(40, this);
            }
        }
    }
});

// @source data/init/End.js

if (typeof Sys !== "undefined") { 
    Sys.Application.notifyScriptLoaded();
}

// @source data/GridDD.js

Ext.grid.GridDragZone.override({
    getDragData : function (e) {
        var t = Ext.lib.Event.getTarget(e),
            rowIndex = this.view.findRowIndex(t);
        
        if (rowIndex !== false) {
            var sm = this.grid.selModel;
        
            if (!sm.isSelected(rowIndex) || e.hasModifier() || sm.keepSelectionOnClick === "always") {
                sm.handleMouseDown(this.grid, rowIndex, e);
            }
        
            return {
                grid       : this.grid, 
                ddel       : this.ddel, 
                rowIndex   : rowIndex, 
                selections : sm.getSelections()
            };
        }
        
        return false;
    }
});
