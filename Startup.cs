using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BoardSquares.Startup))]
namespace BoardSquares
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
