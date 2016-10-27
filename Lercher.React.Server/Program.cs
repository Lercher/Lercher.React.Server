using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ClearScript.V8;
using React;

namespace Lercher.React.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This ist Lercher React");

            var values = new string[100];
            for(var i = 0; i < values.Length; i++)
            {
                values[i] = string.Format("v{0}", i);
            }

            Initialize();

            ReactSiteConfiguration.Configuration
                .SetReuseJavaScriptEngines(false)
                .AddScript("Sample.jsx");

            var environment = ReactEnvironment.Current;
            var component = environment.CreateComponent("HelloWorld", new { name = "Daniel", values });
            var ng = new NamesGenerator(Guid.Empty, 1, Guid.Empty);  // stable names
            Inject(environment, "names", ng);

            // renderServerOnly omits the data-reactid attributes
            var html = component.RenderHtml(renderContainerOnly: false, renderServerOnly: true);
            Console.WriteLine(html);

            var sw = Stopwatch.StartNew();
            ng.reset();
            var html2 = component.RenderHtml(renderContainerOnly: false, renderServerOnly: true);
            sw.Stop();
            Debug.Assert(html == html2);
            Console.WriteLine("Generating took {0}", sw.Elapsed);


            if (System.Diagnostics.Debugger.IsAttached)
                Console.ReadLine();
        }

        private static void Inject(IReactEnvironment env, string name, object o)
        {
            // Engine is a protected property
            var EnginePI = env.GetType().GetProperty("Engine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var obj = EnginePI.GetValue(env);
            var engine = (JavaScriptEngineSwitcher.Core.IJsEngine) obj;
            engine.EmbedHostObject(name, o);
        }

        /*
        private static void InjectGreeter()
        {
            var container = AssemblyRegistration.Container;
            var factory = container.Resolve<IJavaScriptEngineFactory>();
            var engine = factory.GetEngineForCurrentThread();
            engine.EmbedHostObject("greeter", new Greeter());
        }
        */

        private static void Initialize()
        {
            Initializer.Initialize(registration => registration.AsSingleton());
            var container = AssemblyRegistration.Container;
            // Register some components that are normally provided by the integration library
            // (eg. React.AspNet or React.Web.Mvc6)
            container.Register<ICache, NullCache>();
            container.Register<IFileSystem, SimpleFileSystem>();
        }


    }
}
