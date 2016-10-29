using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ClearScript.V8;

namespace Lercher.ReactJS.Core
{
    public static class Hello
    {
        public static void SayHello()
        {
            Console.WriteLine("Hello from Core");
            ThreadPool.SetMinThreads(8, 200);
            var sl = new ScriptLoader();
            sl.AddUrl("https://unpkg.com/react@15/dist/react.min.js");
            sl.AddUrl("https://unpkg.com/react-dom@15/dist/react-dom.min.js");
            var sm = sl.WaitForScriptManager();
            sm.AddScriptContent("var sm = 3; var square = ( a => a*a );", "", 100);
            using (var pool = new JsEnginePool(sm))
            {
                int n = 10;
                var cd = new CountdownEvent(n);
                for (var i = 0; i < n; i++)
                {
                    var ii = i;
                    ThreadPool.QueueUserWorkItem(
                        (o) =>
                        {
                            using (var e = pool.Engine)
                            {
                                e.Calc2(ii);
                            }
                            cd.Signal();
                        }, null);
                }
                cd.Wait();
                Console.WriteLine("completed");
            }
        }
    }
}
