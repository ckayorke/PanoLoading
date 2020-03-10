using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PanoLoading.Startup))]
namespace PanoLoading
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
