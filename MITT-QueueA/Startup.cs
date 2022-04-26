using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MITT_QueueA.Startup))]
namespace MITT_QueueA
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
