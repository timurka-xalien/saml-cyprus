using Cameyo.SamlPoc.Services;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Owin;
using System.Web;

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
