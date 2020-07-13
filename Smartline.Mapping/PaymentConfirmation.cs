using System;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Smartline.Mapping {
    public enum TransactionState {
        Waiting = 1,
        Pending = 2,
        Commited = 3,
        Done = 4
    }

    public class PaymentConfirmation {

        /// <summary>
        /// uses for finding payment documents in couchbase
        /// </summary>
        public int T { get { return AccountingHelper.PaymentConfirmation; } }

        [JsonProperty("userId")]
        public int UserId { get { return Convert.ToInt32(Description); } }

        [JsonProperty("paymentTime")]
        public DateTime PaymentTime { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("receiverCommission")]
        public double ReceiverCommission { get; set; }

        [JsonProperty("senderPhone")]
        public string SenderPhone { get; set; }

        [JsonProperty("transactionId")]
        public int TransactionId { get; set; }

        /// <summary>
        /// sandbox status means test mode
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("orderId")]
        public ulong OrderId { get; set; }

        [JsonProperty("paymentType")]
        public string PaymentType { get; set; }

        /// <summary>
        /// represent userid
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("acc_amount_before")]
        public double AccountAmountBefore { get; set; }

        [JsonProperty("acc_amount_after")]
        public double AccountAmountAfter { get; set; }

        [JsonProperty("publicKey")]
        public string PublicKey { get; set; }

        [JsonProperty("state")]
        public TransactionState State { get; set; }

        public static PaymentConfirmation Parse(NameValueCollection values) {
            var confirmation = new PaymentConfirmation { State = TransactionState.Waiting };

            foreach (string key in values.Keys) {
                switch (key) {
                    case "signature":
                    confirmation.Signature = values[key];
                    break;
                    case "receiver_commission":
                    confirmation.ReceiverCommission = double.Parse(values[key], System.Globalization.CultureInfo.InvariantCulture);
                    break;
                    case "sender_phone":
                    confirmation.SenderPhone = values[key];
                    break;
                    case "transaction_id":
                    confirmation.TransactionId = Convert.ToInt32(values[key]);
                    break;
                    case "status":
                    confirmation.Status = values[key];
                    break;
                    case "order_id":
                    confirmation.OrderId = Convert.ToUInt64(values[key]);
                    break;
                    case "type":
                    confirmation.PaymentType = values[key];
                    break;
                    case "description":
                    confirmation.Description = values[key];
                    break;
                    case "currency":
                    confirmation.Currency = values[key];
                    break;
                    case "amount":
                    confirmation.Amount = double.Parse(values[key], System.Globalization.CultureInfo.InvariantCulture);
                    break;
                    case "public_key":
                    confirmation.PublicKey = values[key];
                    break;
                }
            }
            return confirmation;
        }

        public string GetId() {
            return GetId(this);
        }

        public static string GetId(PaymentConfirmation pc) {
            return string.Format("pc_{0}", pc.TransactionId);
        }

    }


}
