using Cameyo.SamlPoc.Services;
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

            SamlConfigurationManager.Configure(SamlIdentityProvidersRepository.GetInstance());
        }
    }
}
