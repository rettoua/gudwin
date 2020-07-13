using System.Web.Optimization;

namespace Smartline.Web.Add_Start {
    public class BundleConfig {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkID=303951
        public static void RegisterBundles(BundleCollection bundles) {
            bundles.Add(new ScriptBundle("~/bundles/Global")
                .IncludeDirectory("~/Scripts", "*.js", false)
                .IncludeDirectory("~/Scripts/layer", "*.js", true)
                .IncludeDirectory("~/ts/core", "*.js", false)
                .IncludeDirectory("~/ts/models", "*.js", true)
                .IncludeDirectory("~/ts/controllers", "*.js", true)
                .IncludeDirectory("~/ts/views", "*.js", true)
                .IncludeDirectory("~/Scripts/ver", "*.js", false)
                );

            bundles.Add(new StyleBundle("~/bundles/GlobalStyle").IncludeDirectory("~/style", "*.css", true));
        }
    }
}
