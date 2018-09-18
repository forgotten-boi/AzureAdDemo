using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PocAadClient.Startup))]
namespace PocAadClient
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            ConfigureAuth(app);
        }
    }
}
