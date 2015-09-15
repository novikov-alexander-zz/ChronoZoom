using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Chronozoom.UI.Startup))]

namespace Chronozoom.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Watch this: http://www.asp.net/signalr/overview/releases/upgrading-signalr-1x-projects-to-20
            app.MapSignalR();
        }
    }
}
