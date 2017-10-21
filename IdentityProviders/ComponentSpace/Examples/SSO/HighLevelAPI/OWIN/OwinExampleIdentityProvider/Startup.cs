using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OwinExampleIdentityProvider.Startup))]
namespace OwinExampleIdentityProvider
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
