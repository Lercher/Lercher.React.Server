using System;
using System.IO;
using System.Linq;
using System.Timers;

namespace Lercher.ReactJS.Core
{
    internal class ScriptsWatcher
    {
        private readonly TimeSpan gracePeriod;
        private readonly string path;
        private FileSystemWatcher fsw;
        private Action notification;
        private readonly Timer tmr = new Timer();


        public ScriptsWatcher(string path, TimeSpan gracePeriod)
        {
            this.path = path;
            this.gracePeriod = gracePeriod;
        }

        public void EnumerateJsAndJsx(ReactConfiguration reactConfiguration)
        {
            foreach (var fn in System.IO.Directory.GetFiles(path, "*.js*"))
            {
                if (HasExtension(fn, "js"))
                    reactConfiguration.AddJs(fn);
                if (HasExtension(fn, "jsx"))
                    reactConfiguration.AddJsx(fn);
            }
        }

        private void Fsw_Renamed(object sender, RenamedEventArgs e)
        {
            WasTouched(e.FullPath);
        }

        private void Fsw_Changed(object sender, FileSystemEventArgs e)
        {
            WasTouched(e.FullPath);
        }

        private void WasTouched(string fn)
        {
            Console.WriteLine("Changed file {0}", fn);
            if (HasExtension(fn, "js") || HasExtension(fn, "jsx"))
            {
                // rearm the timer for gracePeriod
                tmr.Stop();
                tmr.Interval = gracePeriod.TotalMilliseconds;
                tmr.Start();
            }
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            tmr.Enabled = false;
            if (notification != null) notification();
        }

        private static bool HasExtension(string fn, string js)
        {
            var ext = Path.GetExtension(fn);
            if (ext.StartsWith(".")) ext = ext.Substring(1);
            return 0 == StringComparer.InvariantCultureIgnoreCase.Compare(ext, js);
        }

        public void StartNotify(Action reconfigure)
        {
            fsw = new FileSystemWatcher() { Path = path, Filter = "*.js*" };
            fsw.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            fsw.Changed += Fsw_Changed;
            fsw.Renamed += Fsw_Renamed;
            fsw.EnableRaisingEvents = false;

            tmr.Enabled = false;
            tmr.Elapsed += Tmr_Elapsed;

            notification = reconfigure;
            fsw.EnableRaisingEvents = true;
        }

        public void StopNotify()
        {
            fsw.Dispose();
            tmr.Dispose();       
        }
    }
}
