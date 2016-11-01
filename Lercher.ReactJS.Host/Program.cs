using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lercher.ReactJS.Core;

namespace Lercher.ReactJS.Host
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This ist Lercher.ReactJS.Host");

            var values = new string[100];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = string.Format("v{0}", i);
            }
            var model = new { name = "Daniel", values };
            var modelJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            var runtime = new ReactRuntime(cfg => {
                cfg.AddJsx("Sample.jsx");
            });

            var ng = new NamesGenerator(Guid.Empty, 1, Guid.Empty);
            using (var engine = runtime.ReactPool.Engine)
            {
                engine.AddService("names", ng);
                var r = runtime.RenderToStaticMarkup("HelloWorld", modelJson, engine);
                var r1 = runtime.RenderToStaticMarkup("HelloWorld", model, engine);
                Console.WriteLine(r.render);
            }

            using (var engine = runtime.ReactPool.Engine)
            {
                engine.AddService("names", ng);
                ng.reset();
                var sw = Stopwatch.StartNew();
                var r2 = runtime.RenderToStaticMarkup("HelloWorld", modelJson, engine); // quicker, 7ms
                sw.Stop();
                Console.WriteLine("Generating JSON took {0}", sw.Elapsed);
            }

            using (var engine = runtime.ReactPool.Engine)
            {
                engine.AddService("names", ng);
                ng.reset();
                var sw = Stopwatch.StartNew();
                var r3 = runtime.RenderToStaticMarkup("HelloWorld", model, engine); // can use host methods, 14ms.
                sw.Stop();
                Console.WriteLine("Generating HOST took {0}", sw.Elapsed);
            }

            // -------------------------------------------------------------------------------------------------------
            runtime.DropAndRefreshAllScripts();

            using (var engine = runtime.ReactPool.Engine)
            {
                engine.AddService("names", ng);
                ng.reset();
                var sw = Stopwatch.StartNew();
                var r4 = runtime.RenderToStaticMarkup("HelloWorld", model, engine); // can use host methods, 14ms.
                sw.Stop();
                Console.WriteLine("Generating HOST took {0}", sw.Elapsed);
            }

            //Hello.SayHello();

            Console.Write("\nEnter ... ");
            Console.ReadLine();
        }
    }
}
