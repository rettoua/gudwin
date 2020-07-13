var Core;
(function (Core) {
    var EventArgs = /** @class */ (function () {
        function EventArgs() {
        }
        Object.defineProperty(EventArgs, "Empty", {
            get: function () {
                return new EventArgs();
            },
            enumerable: true,
            configurable: true
        });
        return EventArgs;
    }());
    Core.EventArgs = EventArgs;
    var EventHandler = /** @class */ (function () {
        function EventHandler(handler) {
            this._handler = handler;
        }
        EventHandler.prototype.handle = function (sender, e) {
            this._handler(sender, e);
        };
        return EventHandler;
    }());
    Core.EventHandler = EventHandler;
    var Delegate = /** @class */ (function () {
        function Delegate() {
            this._eventHandlers = new Array();
        }
        Delegate.prototype.subscribe = function (eventHandler) {
            if (this._eventHandlers.indexOf(eventHandler) == -1) {
                this._eventHandlers.push(eventHandler);
            }
        };
        Delegate.prototype.unsubscribe = function (eventHandler) {
            var i = this._eventHandlers.indexOf(eventHandler);
            if (i != -1) {
                this._eventHandlers.splice(i, 1);
            }
        };
        Delegate.prototype.raise = function (sender, e) {
            for (var i = 0; i < this._eventHandlers.length; i++) {
                this._eventHandlers[i].handle(sender, e);
            }
        };
        return Delegate;
    }());
    Core.Delegate = Delegate;
})(Core || (Core = {}));
