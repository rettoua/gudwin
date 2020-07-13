var Core;
(function (Core) {
    var PaymentManager = /** @class */ (function () {
        function PaymentManager() {
        }
        PaymentManager.MakePayment = function () {
            var _this = this;
            this.ShowMask();
            DR.CreatePayment($App.SettingsManager.Settings.UserId, App.fldAmount.getValue(), {
                success: function (r) { return _this.PaymentCreated(r); },
                failure: function (e) { return _this.HideMask(); }
            });
        };
        PaymentManager.PaymentCreated = function (pr) {
            window.open(pr);
            this.HideMask();
            App.wndRecharge.hide();
        };
        PaymentManager.ShowMask = function () {
            App.wndRecharge.body.mask('Обработка данных...');
        };
        PaymentManager.HideMask = function () {
            App.wndRecharge.body.unmask();
        };
        return PaymentManager;
    }());
    Core.PaymentManager = PaymentManager;
    var PaymentHistory = /** @class */ (function () {
        function PaymentHistory() {
        }
        PaymentHistory.loadPayments = function (from, to) {
            var _this = this;
            PaymentHistory.showPaymentMask();
            DR.GetPayments($App.SettingsManager.Settings.UserId, from, to, {
                success: function (items) { _this.onPaymentsLoaded(items); _this.hidePaymentMask(); },
                failure: function (e) { _this.hidePaymentMask(); }
            });
        };
        PaymentHistory.onPaymentsLoaded = function (items) {
            App.gridPaymentTransactions.store.loadData(items);
            this.updatePaymentsTotal();
        };
        PaymentHistory.updatePayments = function (items) {
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
        };
        PaymentHistory.showPaymentMask = function () {
            App.gridPaymentTransactions.body.mask('Загрузка данных...');
        };
        PaymentHistory.hidePaymentMask = function () {
            App.gridPaymentTransactions.body.unmask();
        };
        PaymentHistory.loadWriteOffs = function (from, to) {
            var _this = this;
            this.showWriteOffMask();
            DR.GetWriteOffs($App.SettingsManager.Settings.UserId, from, to, {
                success: function (items) { _this.onWriteOffsLoaded(items); _this.hideWriteOffMask(); },
                failure: function (e) { _this.hideWriteOffMask(); }
            });
        };
        PaymentHistory.onWriteOffsLoaded = function (items) {
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                item.trackerName = $App.RepositoryManager.Repositories[item.trackerUid].Record.get('Name');
            }
            App.gridWriteOffTransaction.store.loadData(items);
            this.updateWriteOffsTotal();
        };
        PaymentHistory.showWriteOffMask = function () {
            App.gridWriteOffTransaction.body.mask('Загрузка данных...');
        };
        PaymentHistory.hideWriteOffMask = function () {
            App.gridWriteOffTransaction.body.unmask();
        };
        PaymentHistory.updateWriteOffLoadButton = function () {
            var validValue = App.fldWriteOffFrom.getValue() && App.fldWriteOffTo.getValue();
            App.bntWriteOff.setDisabled(!validValue);
        };
        PaymentHistory.updatePaymentsLoadButton = function () {
            var validValue = App.fldHistoryFrom.getValue() && App.fldHistoryTo.getValue();
            App.btnShowPayments.setDisabled(!validValue);
        };
        PaymentHistory.updateWriteOffsTotal = function () {
            this.updateTotal(App.gridWriteOffTransaction, App.FieldContainerWriteOffs);
        };
        PaymentHistory.updatePaymentsTotal = function () {
            this.updateTotal(App.gridPaymentTransactions, App.FieldContainerPayments);
        };
        PaymentHistory.updateTotal = function (grid, container) {
            if (!grid.view.rendered) {
                return;
            }
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
            container.items.each(function (field) {
                var column = grid.headerCt.down('component[dataIndex="' + field.name + '"]');
                field.setVisible(column.isVisible());
            });
            container.suspendLayout = false;
            container.updateLayout();
        };
        PaymentHistory._isLoading = false;
        PaymentHistory._loadingIndex = 0;
        return PaymentHistory;
    }());
    Core.PaymentHistory = PaymentHistory;
})(Core || (Core = {}));
