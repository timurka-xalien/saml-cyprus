using Cameyo.SamlPoc.WebApp.Services;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Cameyo.SamlPoc.Startup))]
namespace Cameyo.SamlPoc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            SamlConfigurationManager.Configure(new SamlIdentityProvidersRepository());
        }
    }
}
