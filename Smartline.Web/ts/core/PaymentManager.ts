module Core {
    declare var $App: Application;
    declare var DR;
    declare var App;
    declare var $;
    declare var common;
    declare var Ext;

    interface PaymentRequest {
        Payment: Payment;
        PublicKey: string;
        ResultUrl: string;
        ServerUrl: string;
        Signature: string;
        PayWay: string;
    }

    interface Payment {
        Amount: number;
        Currency: string;
        Description: string;
        OrderId: number;
        Type: string;
    }

    export class PaymentManager {

        public static MakePayment(): void {
            this.ShowMask();
            DR.CreatePayment($App.SettingsManager.Settings.UserId, App.fldAmount.getValue(), {
                success: (r) => this.PaymentCreated(r),
                failure: (e) => this.HideMask()
            });
        }

        private static PaymentCreated(pr: string): void {
            window.open(pr);
            this.HideMask();
            App.wndRecharge.hide();
        }

        private static ShowMask(): void {
            App.wndRecharge.body.mask('Обработка данных...');
        }

        private static HideMask(): void {
            App.wndRecharge.body.unmask();
        }
    }

    export class PaymentHistory {
        private static _isLoading: boolean = false;
        private static _loadingIndex: number = 0;

        public static loadPayments(from: Date, to: Date): void {
            PaymentHistory.showPaymentMask();
            DR.GetPayments($App.SettingsManager.Settings.UserId, from, to, {
                success: (items) => { this.onPaymentsLoaded(items); this.hidePaymentMask(); },
                failure: (e) => { this.hidePaymentMask(); }
            });
        }

        private static onPaymentsLoaded(items: any): void {
            App.gridPaymentTransactions.store.loadData(items);
            this.updatePaymentsTotal();
        }

        private static updatePayments(items: any): any {
            var total = 0;
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                item.paymentTime = Ext.Date.format(common.parseISO8601(item.paymentTime), 'd-m-Y H:i');
                total += item.amount;
            }
            for (var j = 0; j < 30; j++) {
                items.push(items[0]);
            }
            return { items: items, total: total };
        }

        private static showPaymentMask(): void {
            App.gridPaymentTransactions.body.mask('Загрузка данных...');
        }

        private static hidePaymentMask(): void {
            App.gridPaymentTransactions.body.unmask();
        }

        public static loadWriteOffs(from: Date, to: Date): void {
            this.showWriteOffMask();
            DR.GetWriteOffs($App.SettingsManager.Settings.UserId, from, to, {
                success: (items) => { this.onWriteOffsLoaded(items); this.hideWriteOffMask(); },
                failure: (e) => { this.hideWriteOffMask(); }
            });
        }

        private static onWriteOffsLoaded(items: any): void {
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                item.trackerName = $App.RepositoryManager.Repositories[item.trackerUid].Record.get('Name');
            }
            App.gridWriteOffTransaction.store.loadData(items);
            this.updateWriteOffsTotal();
        }

        private static showWriteOffMask(): void {
            App.gridWriteOffTransaction.body.mask('Загрузка данных...');
        }

        private static hideWriteOffMask(): void {
            App.gridWriteOffTransaction.body.unmask();
        }

        public static updateWriteOffLoadButton(): void {
            var validValue = App.fldWriteOffFrom.getValue() && App.fldWriteOffTo.getValue();
            App.bntWriteOff.setDisabled(!validValue);
        }

        public static updatePaymentsLoadButton(): void {
            var validValue = App.fldHistoryFrom.getValue() && App.fldHistoryTo.getValue();
            App.btnShowPayments.setDisabled(!validValue);
        }

        public static updateWriteOffsTotal(): void {
            this.updateTotal(App.gridWriteOffTransaction, App.FieldContainerWriteOffs);
        }

        public static updatePaymentsTotal(): void {
            this.updateTotal(App.gridPaymentTransactions, App.FieldContainerPayments);
        }

        public static updateTotal(grid: any, container: any): void {
            if (!grid.view.rendered) { return; }

            var field, value, width, data = { amount: 0 }, c, cs = grid.headerCt.getVisibleGridColumns();

            for (var j = 0, jlen = grid.store.getCount(); j < jlen; j++) {
                var r = grid.store.getAt(j);

                for (var i = 0, len = cs.length; i < len; i++) {
                    c = cs[i];
                    if (c.dataIndex == 'amount') {
                        data[c.dataIndex] += r.get(c.dataIndex);
                    }
                }
            }

            container.suspendLayout = true;
            for (var i = 0; i < cs.length; i++) {
                c = cs[i];

                field = container.down('component[name="' + c.dataIndex + '"]');

                container.remove(field, false);
                container.insert(i, field);
                width = c.getWidth();
                field.setWidth(width - 1);
                if (c.dataIndex == 'amount') {
                    value = data[c.dataIndex];
                    field.setValue(c.renderer ? (c.renderer)(value, {}, {}, 0, i, grid.store, grid.view) : value);
                }
            }

            container.items.each(field => {
                var column = grid.headerCt.down('component[dataIndex="' + field.name + '"]');
                field.setVisible(column.isVisible());
            });

            container.suspendLayout = false;
            container.updateLayout();
        }
    }
}