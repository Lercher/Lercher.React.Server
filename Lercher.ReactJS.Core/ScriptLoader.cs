using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace Lercher.ReactJS.Core
{
    // Load scripts like React as described in https://facebook.github.io/react/docs/installation.html @CDN
    public class ScriptLoader
    {
        private readonly ScriptRepository ScriptRepository = new ScriptRepository();
        private readonly CountdownEvent cd = new CountdownEvent(1);
        private int? sequence = 0;

        public void AddUrl(string urlFmt, string p0)
        {
            var url = string.Format(urlFmt, p0);
            AddUrl(url);
        }

        public void AddUrl(string url)
        {
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

        public ScriptRepository GetRepository()
        {
            cd.Signal();
            cd.Wait();
            ScriptRepository.Sequence = sequence.Value + 1;
            return ScriptRepository;
        }
    }
}
