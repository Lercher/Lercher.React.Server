using System;
using System.Linq;
using Owin;

namespace Lercher.ReactJS.Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseWelcomePage("/");
            app.Use<ReactJSMiddleware>();
        }
    }
}
