using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
                //cfg.AddJsx("Sample.jsx");
                cfg.WatchDirectory("../..", TimeSpan.FromSeconds(1));
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

            var rnd = new System.Random();
            ThreadStart wt = () =>
            {
                while (true)
                {
                    var v = 0.547238 + rnd.NextDouble();
                    var ts = TimeSpan.FromSeconds(v);
                    Thread.Sleep(ts);
                    using (var engine = runtime.ReactPool.Engine)
                    {
                        engine.AddService("names", ng);
                        ng.reset();
                        var r = runtime.RenderToStaticMarkup("HelloWorld", model, engine);
                        Console.WriteLine("rendered {0:n0} chars in the {1}-loop. Please Change the Js/Jsx files manually.", r.render.Length, ts);
                    }
                }
            };
            for (int i = 0; i < 50; i++)
            {
                var t = new Thread(wt);
                t.IsBackground = true;
                t.Start();
            }

            //Hello.SayHello();

            Console.Write("\nEnter ... ");
            Console.ReadLine();
        }
    }
}
