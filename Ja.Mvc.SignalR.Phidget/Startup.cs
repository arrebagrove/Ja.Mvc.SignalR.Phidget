using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ja.Mvc.SignalR.Phidget.Startup))]
namespace Ja.Mvc.SignalR.Phidget
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            var hubConfiguration = new Microsoft.AspNet.SignalR.HubConfiguration();
            hubConfiguration.EnableDetailedErrors = true;
            // [10003] Maps SignalR hubs to the app builder pipeline at "/signalr".
            app.MapSignalR(hubConfiguration);

        }
    }
}
