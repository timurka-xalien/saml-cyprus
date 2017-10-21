using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OwinExampleServiceProvider.Startup))]
namespace OwinExampleServiceProvider
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
