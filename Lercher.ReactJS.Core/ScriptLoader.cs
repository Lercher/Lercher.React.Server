using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace Lercher.ReactJS.Core
{

    // Load scripts like React as described in https://facebook.github.io/react/docs/installation.html @CDN 

    /// <summary>
    /// Parallel asynchronous loding of scripts from the internet
    /// </summary>
    public class ScriptLoader
    {
        private readonly ScriptRepository ScriptRepository = new ScriptRepository();
        private readonly CountdownEvent cd = new CountdownEvent(1);
        private int? sequence = 0;
        private bool frozen = false;

        /// <summary>
        /// Compose a URL from a format string and one parameter, load that resource and add its content to the repository.
        /// This is usefull if you have a CDN that expects version numbers in the URL.
        /// </summary>
        /// <param name="urlFmt">A string containing zero or more {0} placeholders</param>
        /// <param name="p0">valu for the {0} placeholders</param>
        public void AddUrl(string urlFmt, string p0)
        {
            var url = string.Format(urlFmt, p0);
            AddUrl(url);
        }

        /// <summary>
        /// Load  a resource and add its content to the repository.
        /// </summary>
        /// <param name="url">The URL of the resource to load</param>
        public void AddUrl(string url)
        {
            if (frozen) throw new ApplicationException("can't load a resource from a URL, after the loader is frozen (i.e. called GetRepository).");
            Console.WriteLine("Requesting {0} ...", url);
            sequence++;
            cd.AddCount();
#pragma warning disable S2930 // "IDisposables" should be disposed
            var client = new WebClient();
#pragma warning restore S2930 // "IDisposables" should be disposed
            client.DownloadStringCompleted += Client_DownloadStringCompleted;
            client.BaseAddress = url;
            client.Encoding = System.Text.Encoding.UTF8;
            client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
            client.DownloadStringAsync(new Uri(url), sequence);
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            var seq = e.UserState as int?;
            var client = sender as WebClient;
            ScriptRepository.AddScriptContent(e.Result, client.BaseAddress, seq.Value);
            client.DownloadStringCompleted -= Client_DownloadStringCompleted;
            client.Dispose();
            cd.Signal();
        }

        /// <summary>
        /// Freeze the loader, i.e. wait for all URLs to be resolved and get the resulting <see cref="ScriptRepository"/> where you cann add static scripts.
        /// </summary>
        /// <returns>A <see cref="ScriptRepository"/> with the contents of all requested resources in order of URL adding.</returns>
        public ScriptRepository GetRepository()
        {
            cd.Signal();
            cd.Wait();
            ScriptRepository.Sequence = sequence.Value + 1;
            frozen = true;
            return ScriptRepository;
        }
    }
}
