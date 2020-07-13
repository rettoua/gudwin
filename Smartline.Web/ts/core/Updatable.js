var Core;
(function (Core) {
    var Updatable = /** @class */ (function () {
        function Updatable() {
            this._updateCounter = 0;
        }
        Object.defineProperty(Updatable.prototype, "IsUpdating", {
            get: function () {
                return this._updateCounter != 0;
            },
            enumerable: true,
            configurable: true
        });
        Updatable.prototype.BeginUpdate = function () {
            this._updateCounter++;
        };
        Updatable.prototype.EndUpdate = function () {
            this._updateCounter--;
        };
        return Updatable;
    }());
    Core.Updatable = Updatable;
})(Core || (Core = {}));
