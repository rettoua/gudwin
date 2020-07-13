using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

namespace Smartline.Web {
    /// <summary>
    /// Summary description for Download
    /// </summary>
    public class Download : IHttpHandler {
        private static string ScriptVersion = "";
        private const string Salt = "slava_ukraini_v6";

        public void ProcessRequest(HttpContext context) {
            //if (!string.IsNullOrEmpty(context.Request.QueryString["js"])) {
            //    string fileName = context.Request.QueryString["js"] + ".js";
            //    context.Response.ContentType = "text/javascript";
            //    context.Response.Write(System.IO.File.ReadAllText(GetScriptDirectoryPath() + fileName));
            //} else 
            if (!string.IsNullOrEmpty(context.Request.QueryString["css"])) {
                string fileName = context.Request.QueryString["css"] + ".css";
                context.Response.ContentType = "text/css";
                context.Response.Write(File.ReadAllText(HttpContext.Current.Server.MapPath("~") + "/style/" + fileName));
            }
        }

        public static void AddJScriptDynamicaly(string jsfile, System.Web.UI.Page page) {
            if (page.Header != null) {
                var js = new HtmlGenericControl("script");
                js.Attributes.Add("type", "text/javascript");
                js.Attributes.Add("src", string.Format("../Scripts/ver/{0}.js?{1}", jsfile, Salt));
                page.Header.Controls.Add(js);
            }
        }

        public static void AddJScriptDynamicaly(string jsfile, string directory, System.Web.UI.Page page) {
            if (page.Header != null) {
                var js = new HtmlGenericControl("script");
                js.Attributes.Add("type", "text/javascript");
                js.Attributes.Add("src", Path.Combine(directory, jsfile + ".js?" + Salt));
                page.Header.Controls.Add(js);
            }
        }

        public static void AddCssDynamicaly(string cssfile, System.Web.UI.Page page) {
            if (page.Header != null) {
                var css = new HtmlGenericControl("link");
                css.Attributes.Add("type", "text/css");
                css.Attributes.Add("rel", "stylesheet");
                css.Attributes.Add("href", string.Format("../Download.ashx?css={0}", cssfile));
                page.Header.Controls.Add(css);
            }
        }

        private static string GetScriptVersion() {
            if (!string.IsNullOrWhiteSpace(ScriptVersion)) {
                return ScriptVersion;
            }
            IEnumerable<DirectoryInfo> directories = from c in Directory.GetDirectories(GetScriptDirectoryPath())
                                                     select new DirectoryInfo(c);

            DirectoryInfo verFolder = directories.FirstOrDefault(o => o.Name.StartsWith("ver"));
            if (verFolder != null) {
                return (ScriptVersion = verFolder.Name);
            }
            return (ScriptVersion = "ver1");
        }

        private static string GetScriptDirectoryPath() {
            return HttpContext.Current.Server.MapPath("~") + "Scripts";
        }

        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}