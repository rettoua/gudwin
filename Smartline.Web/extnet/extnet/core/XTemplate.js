﻿
// @source core/XTemplate.js

Ext.net.XTemplate = function (config) {
    config = config || {};
    var html;
    
    this.proxyId = config.proxyId;
    
    if (config.el) {
        config.el = Ext.getDom(config.el);
        html = config.el.value || config.el.innerHTML;
    } else {
        html = config.html;
        
        if (Ext.isArray(html)) {
            html = html.join("");
        }
    }
    
    Ext.net.XTemplate.superclass.constructor.call(this, html, config.functions);
};

Ext.extend(Ext.net.XTemplate, Ext.XTemplate, {
    destroy : function () {
        var ns = this.ns || Ext.net.ResourceMgr.ns,
            id = this.itemId || this.proxyId;        
            
        if (ns && id) {                
            if (Ext.isObject(ns) && ns[id]) {
                try {
                    delete ns[id];
                } catch (e) {
                    ns[id] = undefined;
                }
            } else if (Ext.net.ResourceMgr.getCmp(ns + "." + id)) {
                try {
                    delete Ext.ns(ns)[id];
                } catch (f) {
                    Ext.ns(ns)[id] = undefined;
                }
            }
        } else if (window[this.proxyId]) {
            window[this.proxyId] = null;
        }
    }
});