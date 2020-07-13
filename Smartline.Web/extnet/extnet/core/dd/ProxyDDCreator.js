﻿
// @source core/dd/ProxyDDCreator.js

Ext.net.ProxyDDCreator = function (config) {
    Ext.net.ProxyDDCreator.superclass.constructor.call(this, config);    
    this.config = config || {};
    
    if (!Ext.isEmpty(this.config.target, false)) {
        var targetEl = Ext.net.getEl(this.config.target);

        if (!Ext.isEmpty(targetEl)) {
            this.initDDControl(targetEl);
        } else {
            this.task = new Ext.util.DelayedTask(function () {
                targetEl = Ext.net.getEl(this.config.target);

                if (!Ext.isEmpty(targetEl)) {
                    this.task.cancel();
                    this.initDDControl(targetEl);                    
                } else {
                    this.task.delay(500);
                }
            }, this);
            
            this.task.delay(1);
        }
    }
};

Ext.extend(Ext.net.ProxyDDCreator, Ext.util.Observable, {
    initDDControl : function (target) {
        target = Ext.net.getEl(target);
        
        if (target.isComposite) {
            this.ddControl = [];
            target.each(function (targetEl) {
                this.ddControl.push(this.createControl(Ext.apply(Ext.net.clone(this.config), { id : Ext.id(targetEl) })));
            }, this);
        } else {
            this.ddControl = this.createControl(Ext.apply(Ext.net.clone(this.config), { id : Ext.id(target) }));
        }
    },
    
    createControl: function (config) {
        var ddControl;
        
        if (config.group) {
            ddControl = new config.type(config.id, config.group, config.config);
            Ext.apply(ddControl, config.config);
        } else {
            ddControl = new config.type(config.id, config.config);
        }        
        
        return ddControl;
    },
    
    lock : function () {
        Ext.each(this.ddControl, function (dd) {
            if (dd && dd.lock) {
                dd.lock();
            }
        });
    },
    
    unlock : function () {
        Ext.each(this.ddControl, function (dd) {
            if (dd && dd.unlock) {
                dd.unlock();
            }
        });
    },
    
    unreg : function () {
        Ext.each(this.ddControl, function (dd) {
            if (dd && dd.unreg) {
                dd.unreg();
            }
        });
    },
    
    destroy : function () {
        Ext.each(this.ddControl, function (dd) {
            if (dd && dd.unreg) {
                dd.unreg();
            }
        });
    }
});