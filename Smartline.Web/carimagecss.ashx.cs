using System.Text;
using System.Web;
using Smartline.Mapping;

namespace Smartline.Web {
    /// <summary>
    /// Summary description for carimagecss
    /// </summary>
    public class carimagecss : IHttpHandler {

        public void ProcessRequest(HttpContext context) {
            context.Response.ContentType = "text/css";
            string css = GetCssFile();
            context.Response.Write(css);
        }

        public bool IsReusable {
            get {
                return false;
            }
        }

        private string GetCssFile() {
            var sb = new StringBuilder();
            foreach (CarImage carImage in CarImage.Images) {
                sb.Append(string.Format(".car-icon-{0}{{", carImage.Name));
                sb.Append("background-repeat:no-repeat;");
                sb.Append("background-size: 100% auto;");
                sb.Append(string.Format("background-image:url('Resources/car_{0}.png');", carImage.Name));
                sb.Append("}");
            }
            return sb.ToString();
        }
    }
}