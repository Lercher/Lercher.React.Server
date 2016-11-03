using System;
using System.Threading.Tasks;
using Lercher.ReactJS.Core;
using Microsoft.Owin;
using System.Collections.Generic;

namespace Lercher.ReactJS.Host
{
    public class ReactJSMiddleware : OwinMiddleware
    {
        private readonly PathString route;
        private readonly ReactRuntime runtime;
        private const string fn = "../../model.json";

        public ReactJSMiddleware(OwinMiddleware next) : base(next)
        {
            route = new PathString("/react");

            runtime = new ReactRuntime(cfg => {
                cfg.WatchDirectory("../..", TimeSpan.FromSeconds(1));
                cfg.PreprocessorFunction = "reactPreprocessor";
                cfg.PostprocessorFunction = "reactPostprocessor";
            });
        }

        public async override Task Invoke(IOwinContext context)
        {

            PathString rest;
            if (context.Request.Path.StartsWithSegments(route, out rest))
                await ReactOn(context, rest);
            else
                await Next.Invoke(context);
        }

        private async Task ReactOn(IOwinContext context, PathString rest)
        {
            Console.WriteLine("{0} React call: {1}", DateTime.Now, rest);

            IFormCollection form = null;
            if (context.Request.Method == "POST")
                form = await context.Request.ReadFormAsync();

            var modelJson = loadJson();
            using (var engine = runtime.Engine)
            {
                var ng = new NamesGenerator(Guid.Empty, 1, engine.ScriptsHash);
                engine.AddHostService("names", ng);
                engine.SetContext(ConvertToDictionary(form));
                var r = runtime.RenderToStaticMarkup("HelloWorld", modelJson, engine);
                context.Response.ContentType = "text/html";
                context.Response.Write(r.render);
                saveJson(r.modelAsJson);
            }
        }

        private static IDictionary<string, string> ConvertToDictionary(IFormCollection form)
        {
            if (form == null) return null;
            var r = new Dictionary<string, string>();
            foreach (var kv in form)
                r[kv.Key] = kv.Value[0]; // won't accept multiple duplicate names!
            return r;
        }

        private static string loadJson()
        {
            if (!System.IO.File.Exists(fn))
            {
                var values = new string[100];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = string.Format("v{0}", i);
                }
                var model = new { name = "Daniel", values };
                var modelJson = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
                saveJson(modelJson);
            }
            return System.IO.File.ReadAllText(fn);
        }

        private static void saveJson(string modelJson)
        {
            System.IO.File.WriteAllText(fn, modelJson);
        }
    }
}
