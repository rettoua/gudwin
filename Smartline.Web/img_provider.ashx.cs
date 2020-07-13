using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Smartline.Web {
    /// <summary>
    /// Summary description for img_provider
    /// </summary>
    public class img_provider : IHttpHandler {

        public void ProcessRequest(HttpContext context) {
            
            context.Response.ContentType = "image/png";

            var g = Properties.Resources.arrow_copy;
            MemoryStream mem = new MemoryStream();
            g.Save(mem, ImageFormat.Png);
            mem.Position = 0;
            mem.WriteTo(context.Response.OutputStream);
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}