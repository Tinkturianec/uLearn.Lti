using Microsoft.Owin;
using Owin;
using uLearn.Lti;

[assembly: OwinStartup(typeof(Startup))]
namespace uLearn.Lti
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
