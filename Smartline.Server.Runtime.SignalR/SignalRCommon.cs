using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Owin;

namespace Smartline.Server.Runtime.SignalR {
    public sealed class SignalRCommon {
        public static IAppBuilder App = null;

        public void Configuration(IAppBuilder app) {
            app.Map("/signalr", map => {
                map.UseCors(CorsOptions.AllowAll);

                var hubConfiguration = new HubConfiguration {
                    EnableDetailedErrors = true,
                    EnableJSONP = true
                };

                map.RunSignalR(hubConfiguration);
            });
        }
    }
}