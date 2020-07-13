using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Drawing;

namespace Smartline.Web {
    /// <summary>
    /// Summary description for img
    /// </summary>
    public class img : IHttpHandler {
        public void ProcessRequest(HttpContext context) {
            string percentStr = context.Request.QueryString["perc"];
            if (!string.IsNullOrEmpty(percentStr)) {
                int percentage = Convert.ToInt32(percentStr);
                context.Response.ContentType = "image/png";
                var grey = Smartline.Web.Properties.Resources.gas_grey;
                int newWidth = (51 * percentage) / 100;
                int widthOffset = 51 - newWidth;

                if (percentage > 20) {
                    using (Graphics gr = Graphics.FromImage(grey)) {
                        gr.DrawImage(Smartline.Web.Properties.Resources.gas, new Rectangle(widthOffset, 0, newWidth, 6), new Rectangle(widthOffset, 0, newWidth, 6),
                                     GraphicsUnit.Pixel);
                    }
                } else {
                    using (Graphics gr = Graphics.FromImage(grey)) {
                        gr.DrawImage(Smartline.Web.Properties.Resources.gas_red, new Rectangle(widthOffset, 0, newWidth, 6), new Rectangle(widthOffset, 0, newWidth, 6),
                                     GraphicsUnit.Pixel);
                    }
                }

                MemoryStream mem = new MemoryStream();
                grey.Save(mem, ImageFormat.Png);
                mem.Position = 0;
                mem.WriteTo(context.Response.OutputStream);
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}