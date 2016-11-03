using System;
using System.Threading.Tasks;
using Lercher.ReactJS.Core;
using Microsoft.Owin;

namespace Lercher.ReactJS.Host
{
    public class ReactJSMiddleware : OwinMiddleware
    {
        private readonly PathString route;
        private readonly ReactRuntime runtime;

        public ReactJSMiddleware(OwinMiddleware next) : base(next)
        {
            route = new PathString("/react");

            runtime = new ReactRuntime(cfg => {
                cfg.WatchDirectory("../..", TimeSpan.FromSeconds(1));
            });
        }

        public async override Task Invoke(IOwinContext context)
        {

            PathString rest;
            if (context.Request.Path.StartsWithSegments(route, out rest))
                ReactOn(context, rest);
            else
                await Next.Invoke(context);
        }

        private void ReactOn(IOwinContext context, PathString rest)
        {
            Console.WriteLine("{0} React call: {1}", DateTime.Now, rest);

            var values = new string[100];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = string.Format("v{0}{1}", i, rest);
            }
            var model = new { name = "Daniel", values };
            var modelJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            using(var engine = runtime.Engine)
            {
                var ng = new NamesGenerator(Guid.Empty, 1, Guid.Empty);
                engine.AddService("names", ng);
                var r = runtime.RenderToStaticMarkup("HelloWorld", modelJson, engine);
                context.Response.ContentType = "text/html";
                context.Response.Write(r.render);
            }
        }
    }
}
