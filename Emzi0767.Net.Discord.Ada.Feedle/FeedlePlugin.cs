using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Emzi0767.Net.Discord.AdaBot;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Tools.MicroLogger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.Ada.Feedle
{
    [Plugin("ADA RSS Plugin")]
    public class FeedlePlugin
    {
        private static List<Feed> ActiveFeeds { get; set; }

        public static void Initialize()
        {
            L.W("ADA RSS", "Initializing Feedle");
            ActiveFeeds = new List<Feed>();

            var a = Assembly.GetEntryAssembly();
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            l = Path.Combine(l, "feedle.json");
            if (File.Exists(l))
            {
                var jo = JObject.Parse(File.ReadAllText(l, new UTF8Encoding(false)));
                var ja = (JArray)jo["feedle_config"];
                foreach (var xjt in ja)
                {
                    var xjo = (JObject)xjt;

                    var tag = (string)xjo["tag"];
                    var uri_ = (string)xjo["uri"];
                    var uri = new Uri(uri_);
                    var chn = (ulong)xjo["channel"];
                    var ris = (JArray)xjo["recent"];
                    ActiveFeeds.Add(new Feed(uri, chn, tag) { RecentUris = ris.Select(xjv => (string)xjv).ToList() });
                }
            }

            var tmr = new Timer(new TimerCallback(FeedleTick), null, 0, 300000);
            L.W("ADA RSS", "Done");
        }

        public static void AddFeed(Uri uri, ulong channel)
        {
            AddFeed(uri, channel, null);
        }

        public static void AddFeed(Uri uri, ulong channel, string tag)
        {
            ActiveFeeds.Add(new Feed(uri, channel, tag));
            L.W("ADA RSS", "Added RSS feed for {0}: {1} with tag [{2}]", channel, uri, tag == null ? "<null>" : tag);

            UpdateConfig();
        }

        public static void RemoveFeed(Uri uri, ulong channel)
        {
            RemoveFeed(uri, channel, null);
        }

        public static void RemoveFeed(Uri uri, ulong channel, string tag)
        {
            var feed = ActiveFeeds.FirstOrDefault(xf => xf.FeedUri == uri && xf.ChannelId == channel && xf.Tag == tag);
            ActiveFeeds.Remove(feed);
            L.W("ADA RSS", "Removed RSS feed for {0}: {1} with tag [{2}]", channel, uri, tag == null ? "<null>" : tag);

            UpdateConfig();
        }

        internal static IEnumerable<Feed> GetFeeds(ulong[] channels)
        {
            foreach (var feed in ActiveFeeds)
                if (channels.Contains(feed.ChannelId))
                    yield return feed;
        }

        private static void UpdateConfig()
        {
            L.W("ADA RSS", "Updating config");

            var jo = new JObject();
            var ja = new JArray();
            jo.Add("feedle_config", ja);
            foreach (var feed in ActiveFeeds)
            {
                var xjo = new JObject();
                ja.Add(xjo);

                xjo.Add("uri", feed.FeedUri.ToString());
                xjo.Add("channel", feed.ChannelId);
                xjo.Add("tag", feed.Tag);
                xjo.Add("recent", new JArray(feed.RecentUris));
            }
            var a = Assembly.GetEntryAssembly();
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            l = Path.Combine(l, "feedle.json");
            File.WriteAllText(l, jo.ToString(Formatting.None), new UTF8Encoding(false));
        }

        private static void FeedleTick(object _)
        {
            var wc = new WebClient();
            wc.Encoding = new UTF8Encoding(false);
            bool changed = false;
            foreach (var feed in ActiveFeeds)
            {
                var rec = new List<string>();

                var uri_root_builder = new UriBuilder(feed.FeedUri);
                var ctx = wc.DownloadString(feed.FeedUri);
                var rss = XDocument.Parse(ctx);
                var chn = rss.Root.Element("channel");
                var its = chn.Elements("item").Reverse();
                foreach (var it in its)
                {
                    var itt = (string)it.Element("title");
                    var itl = (string)it.Element("link");
                    var itp = (string)it.Element("pubDate");
                    if (itl.StartsWith("/"))
                        uri_root_builder.Path = itl;
                    else
                        uri_root_builder = new UriBuilder(itl);
                    var itu = uri_root_builder.Uri;
                    var itd = DateTime.Parse(itp, CultureInfo.InvariantCulture);

                    rec.Add(itu.ToString());
                    if (!feed.RecentUris.Contains(itu.ToString()))
                    {
                        changed = true;
                        var sb = new StringBuilder();
                        sb.AppendFormat("{0}**{1}**", feed.Tag == null ? "" : string.Concat("[", feed.Tag, "] "), itt).AppendLine();
                        sb.AppendFormat("Published on {0:yyyy-MM-dd HH:mm} UTC", itd.ToUniversalTime()).AppendLine();
                        sb.AppendLine(itu.ToString());
                        AdaBotCore.AdaClient.SendMessage(sb.ToString(), feed.ChannelId);
                    }
                }

                if (changed)
                    feed.RecentUris = rec;
            }
            if (changed)
                UpdateConfig();
        }
    }
}
