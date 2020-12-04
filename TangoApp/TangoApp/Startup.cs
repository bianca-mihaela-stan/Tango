using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TangoApp.Startup))]
namespace TangoApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
