
// @source core/tips/ToolTip.js

Ext.ToolTip.override({
    initTarget : function (target) {
        var targetEl = Ext.net.getEl(target);

        if (!Ext.isEmpty(targetEl)) {
            this.initTargetEvents(targetEl);
        } else {
            var getTargetTask = new Ext.util.DelayedTask(function (task) {
                targetEl = Ext.net.getEl(target);

                if (!Ext.isEmpty(targetEl)) {
                    this.initTargetEvents(targetEl);
                    task.cancel();
                } else {
                    task.delay(500, undefined, this, [ task ]);
                }
            }, this);
            
            getTargetTask.delay(1, undefined, this, [ getTargetTask ]);
        }
    },

    initTargetEvents : function (targetEl) {
        this.target = targetEl;
        var t;
        
        if ((t = Ext.get(this.target))) {
            if (this.target) {
                this.target = Ext.get(this.target);
                this.target.un("mouseover", this.onTargetOver, this);
                this.target.un("mouseout", this.onTargetOut, this);
                this.target.un("mousemove", this.onMouseMove, this);
            }
        
            this.mon(t, {
                mouseover : this.onTargetOver,
                mouseout  : this.onTargetOut,
                mousemove : this.onMouseMove,
                scope     : this
            });
            this.target = t;
        }
        
        if (this.anchor) {
            this.anchorTarget = this.target;
        }
    }
});

Ext.QuickTips.init = Ext.QuickTips.init.createSequence(function () {
    var fn = function () {
            var tip = Ext.QuickTips.getQuickTip();
            if (tip) {
                tip.disabled = true;
            }
        };
        
    if (window.addEventListener) {
        window.addEventListener("unload", fn, false);
    } else if (window.attachEvent) {
        window.attachEvent("onunload", fn);
    } 
});