using System;
using System.Web;
using System.Web.SessionState;

namespace Smartline.Web {
    /// <summary>
    /// Summary description for SessionRefresher
    /// </summary>
    public class SessionRefresh : IHttpHandler, IRequiresSessionState {
        public void ProcessRequest(HttpContext context) {
            context.Session["SessionRefresh"] = DateTime.Now;
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}