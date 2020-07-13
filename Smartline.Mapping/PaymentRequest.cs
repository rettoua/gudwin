namespace Smartline.Mapping {
    public class PaymentRequest {

        public int T { get { return AccountingHelper.PaymentRequest; } }

        public string PublicKey { get; set; }

        public int Amount { get; set; }

        public string Currency { get { return "UAH"; } }

        public string Description { get; set; }

        public ulong OrderId { get; set; }

        public string Type { get { return PaymentType.buy.ToString(); } }

        public string ServerUrl { get; set; }

        public string ResultUrl { get; set; }

        public PayWay PayWay { get { return PayWay.card; } }

        public string Signature { get; set; }

        public static string GetId(PaymentRequest pr) {
            return GetId(pr.OrderId);
        }

        public static string GetId(ulong orderId) {
            return string.Format("pr_{0}", orderId);
        }
    }

    public enum PaymentType {
        buy = 0,
        donate = 1,
        subscribe = 2
    }

    public enum PayWay {
        card = 0,
        delayed = 1
    }
}