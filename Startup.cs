using Microsoft.Owin;
using System.Threading.Tasks;
using Owin;

[assembly: OwinStartupAttribute(typeof(TilerFront.Startup))]
namespace TilerFront
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
