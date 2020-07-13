module Core {

    export class EventArgs {
        public static get Empty(): EventArgs {
            return new EventArgs();
        }
    }

    export interface IEventHandler<T extends EventArgs> {
        handle(sender: any, e: T): void;
    }

    export class EventHandler<T extends EventArgs> implements IEventHandler<T> {
        private _handler: { (sender: any, e: T): void };

        constructor(handler: { (sender: any, e: T): void }) {
            this._handler = handler;
        }

        public handle(sender: any, e: T): void {
            this._handler(sender, e);
        }
    }

    export interface IDelegate<T extends EventArgs> {
        subscribe(eventHandler: IEventHandler<T>): void;
        unsubscribe(eventHandler: IEventHandler<T>): void;
    }

    export class Delegate<T extends EventArgs> implements IDelegate<T> {
        private _eventHandlers: Array<IEventHandler<T>>;

        constructor() {
            this._eventHandlers = new Array<IEventHandler<T>>();
        }

        public subscribe(eventHandler: IEventHandler<T>): void {
            if (this._eventHandlers.indexOf(eventHandler) == -1) {
                this._eventHandlers.push(eventHandler);
            }
        }

        public unsubscribe(eventHandler: IEventHandler<T>): void {
            var i = this._eventHandlers.indexOf(eventHandler);
            if (i != -1) {
                this._eventHandlers.splice(i, 1);
            }
        }

        public raise(sender: any, e: T): void {
            for (var i = 0; i < this._eventHandlers.length; i++) {
                this._eventHandlers[i].handle(sender, e);
            }
        }
    }
}
