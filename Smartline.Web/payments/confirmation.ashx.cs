using System.Collections.Specialized;
using System.Web;
using Smartline.Server.Runtime.Accounting;

namespace Smartline.Web.payments {
    /// <summary>
    /// Summary description for confirmation
    /// </summary>
    public class confirmation : IHttpHandler {

        public void ProcessRequest(HttpContext context) {
            Process(context);
        }

        public void Process(HttpContext context) {
            NameValueCollection values = context.Request.Form;
            PaymentManager.Confirmation(values);
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}