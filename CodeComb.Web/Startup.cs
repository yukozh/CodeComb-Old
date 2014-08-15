using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CodeComb.Web.Startup))]
namespace CodeComb.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
            
        }
    }
}
