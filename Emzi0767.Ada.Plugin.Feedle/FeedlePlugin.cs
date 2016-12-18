using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Xml.Linq;
using Discord;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Plugins;

namespace Emzi0767.Ada.Plugin.Feedle
{
    public class FeedlePlugin : IAdaPlugin
    {
        public IAdaPluginConfig Config { get { return this.conf; } }
        public Type ConfigType { get { return typeof(FeedlePluginConfig); } }
        public string Name { get { return "ADA RSS Plugin"; } }
        private FeedlePluginConfig conf;

        public static FeedlePlugin Instance { get; private set; }

        public void Initialize()
        {
            L.W("ADA RSS", "Initializing Feedle");
            Instance = this;
            this.conf = new FeedlePluginConfig();
            var tmr = new Timer(new TimerCallback(FeedleTick), null, 5000, 300000);
            L.W("ADA RSS", "Done");
        }

        public void LoadConfig(IAdaPluginConfig config)
        {
            var cfg = config as FeedlePluginConfig;
            if (cfg != null)
                this.conf = cfg;
        }

        public void AddFeed(Uri uri, ulong channel)
        {
            AddFeed(uri, channel, null);
        }

        public void AddFeed(Uri uri, ulong channel, string tag)
        {
            this.conf.Feeds.Add(new Feed(uri, channel, tag));
            L.W("ADA RSS", "Added RSS feed for {0}: {1} with tag [{2}]", channel, uri, tag == null ? "<null>" : tag);

            UpdateConfig();
        }

        public void RemoveFeed(Uri uri, ulong channel)
        {
            RemoveFeed(uri, channel, null);
        }

        public void RemoveFeed(Uri uri, ulong channel, string tag)
        {
            var feed = this.conf.Feeds.FirstOrDefault(xf => xf.FeedUri == uri && xf.ChannelId == channel && xf.Tag == tag);
            this.conf.Feeds.Remove(feed);
            L.W("ADA RSS", "Removed RSS feed for {0}: {1} with tag [{2}]", channel, uri, tag == null ? "<null>" : tag);

            UpdateConfig();
        }

        internal IEnumerable<Feed> GetFeeds(ulong[] channels)
        {
            foreach (var feed in this.conf.Feeds)
                if (channels.Contains(feed.ChannelId))
                    yield return feed;
        }

        private void UpdateConfig()
        {
            L.W("ADA RSS", "Updating config");

            AdaBotCore.ConfigManager.UpdateConfig(this);
        }

        private void FeedleTick(object _)
        {
            var wc = new HttpClient();
            bool changed = false;
            foreach (var feed in this.conf.Feeds)
            {
                var rec = new List<string>();

                var uri_root_builder = new UriBuilder(feed.FeedUri);
                var ctx = wc.GetStringAsync(feed.FeedUri).GetAwaiter().GetResult();
                var rss = XDocument.Parse(ctx);
                var chn = rss.Root.Element("channel");
                var img = chn.Element("image");
                var thm = (string)null;
                if (img != null)
                    thm = img.Element("url").Value;
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
                        var embed = new EmbedBuilder();
                        embed.Title = string.Concat(feed.Tag != null ? string.Concat("[**", feed.Tag, "**] ") : "", itt);
                        embed.Url = itu.ToString();
                        embed.Timestamp = new DateTimeOffset(itd.ToUniversalTime());
                        embed.Color = new Color(255, 127, 0);
                        embed.ThumbnailUrl = thm ?? AdaBotCore.AdaClient.CurrentUser.AvatarUrl;
                        if (feed.Initialized) AdaBotCore.AdaClient.SendEmbed(embed, feed.ChannelId);
                    }
                }

                feed.Initialized = true;
                if (changed)
                    feed.RecentUris = rec;
            }
            if (changed)
                UpdateConfig();
        }
    }
}
