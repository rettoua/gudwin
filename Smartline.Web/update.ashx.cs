using System.Collections.Specialized;
using System.IO;
using System.Web;

namespace Smartline.Web {
    /// <summary>
    /// Summary description for update1
    /// </summary>
    public class update : IHttpHandler {

        public void ProcessRequest(HttpContext context) {
            string fileName = GetFileName(context.Request.QueryString);
            context.Response.ContentType = "application/octet-stream";
            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            var mem = new MemoryStream(File.ReadAllBytes(HttpContext.Current.Server.MapPath("~") + fileName));
            mem.WriteTo(context.Response.OutputStream);
        }

        private string GetFileName(NameValueCollection queryString) {
            string fileName = queryString["f"];
            return string.IsNullOrWhiteSpace(fileName) ? "uploaded.hex" : fileName;
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}