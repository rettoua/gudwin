using System;
using System.Collections.Specialized;
using System.Text;
using Enyim.Caching.Memcached;
using Ext.Net;
using Smartline.Mapping;

namespace Smartline.Server.Runtime.Accounting {
    public class PaymentManager {
        private const string PublicKey = "i10489885293";
        private const string PrivateKey = "a6H8k91YSCRXCaXCAp8ExMWo5Mb5KU3y6dJ33Mwu";

        private const string ResultUrl = "http://213.111.88.222/payments/paid.aspx";
        private const string ServerUrl = "http://213.111.88.222/payments/confirmation.ashx";

        public static PaymentRequest CreatePaymentRequest(int userUid, int amount) {
            var paymentRequest = new PaymentRequest();
            paymentRequest.Amount = amount;
            paymentRequest.OrderId = Increments.GeneratePaymentId();
            paymentRequest.Description = userUid + "";
            paymentRequest.PublicKey = PublicKey;
            paymentRequest.ResultUrl = ResultUrl;
            paymentRequest.ServerUrl = ServerUrl;
            paymentRequest.Signature = GenerateSignature(paymentRequest);
            return paymentRequest;
        }

        private static string GenerateSignature(PaymentRequest paymentRequest) {
            string sign = PrivateKey + paymentRequest.Amount + paymentRequest.Currency + PublicKey;
            sign += paymentRequest.OrderId;
            sign += paymentRequest.Type;
            sign += paymentRequest.Description;
            sign += paymentRequest.ResultUrl;
            sign += paymentRequest.ServerUrl;

            return Sign(sign);
        }

        private static string Sign(string sign) {
            byte[] bytes = Sha1(sign);
            string base64 = Convert.ToBase64String(bytes);
            return base64;
        }

        private static byte[] Sha1(string text) {
            var bytes = Encoding.UTF8.GetBytes(text);
            using (var sha = System.Security.Cryptography.SHA1.Create()) {
                var hash = sha.ComputeHash(bytes);
                return hash;
            }
        }

        public static void SavePaymentRequest(PaymentRequest paymentRequest) {
            string serialized = JSON.Serialize(paymentRequest);
            CouchbaseManager.SaveToAccountingBucket(StoreMode.Set, PaymentRequest.GetId(paymentRequest), serialized);
        }

        public static void SavePaymentConfirmation(PaymentConfirmation paymentConfirmation) {
            string id = PaymentConfirmation.GetId(paymentConfirmation);
            if (CouchbaseManager.AccountingDocumentExist(id)) { return; }
            string serialized = JSON.Serialize(paymentConfirmation);
            CouchbaseManager.SaveToAccountingBucket(StoreMode.Set, id, serialized);
        }

        /// <summary>
        /// sandbox flag means test mode
        /// </summary>
        public static string GetPaymentUrl(PaymentRequest request) {
            var uri = string.Format("https://www.liqpay.com/api/pay?&public_key={0}&sandbox=1&amount={1}&currency={2}&description={3}&order_id={4}&type={5}&pay_way={6}&server_url={7}&result_url={8}&signature={9}",
                request.PublicKey,
                request.Amount,
                request.Currency,
                request.Description,
                request.OrderId,
                request.Type,
                request.PayWay,
                request.ServerUrl,
                request.ResultUrl,
                request.Signature);
            var uri1 = Uri.EscapeUriString(uri);
            return uri1;
        }

        public static void Confirmation(NameValueCollection values) {
            try {
                var pc = PaymentConfirmation.Parse(values);
                pc.PaymentTime = DateTime.Now;
                string sign = PrivateKey + pc.Amount.ToString(".00") + pc.Currency + PublicKey + pc.OrderId + pc.PaymentType +
                              pc.Description + pc.Status + pc.TransactionId + pc.SenderPhone;
                string signature = Sign(sign);
                if (pc.Signature == signature) {
                    SavePaymentConfirmation(pc);
                } else {
                    //TODO: do something if signature is not valid
                }
            } catch (Exception ex) {

            }
        }
    }
}