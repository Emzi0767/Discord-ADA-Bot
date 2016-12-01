using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Discord;
using Emzi0767.Net.Discord.AdaBot;
using Emzi0767.Net.Discord.AdaBot.Config;
using Emzi0767.Net.Discord.AdaBot.Plugins;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.Ada.Feedle
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
            var tmr = new Timer(new TimerCallback(FeedleTick), null, 0, 300000);
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
            var wc = new WebClient();
            wc.Encoding = new UTF8Encoding(false);
            bool changed = false;
            foreach (var feed in this.conf.Feeds)
            {
                var rec = feed.RecentUris;
                feed.RecentUris = new List<string>();

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

                    feed.RecentUris.Add(itu.ToString());
                    if (!rec.Contains(itu.ToString()))
                    {
                        changed = true;
                        var embed = new EmbedBuilder();
                        embed.Title = string.Concat(feed.Tag != null ? string.Concat("[**", feed.Tag, "**] ") : "", itt);
                        embed.Url = itu.ToString();
                        embed.Timestamp = new DateTimeOffset(itd.ToUniversalTime());
                        embed.Author = new EmbedAuthorBuilder();
                        embed.Author.IconUrl = AdaBotCore.AdaClient.CurrentUser.AvatarUrl;
                        embed.Author.Name = "ADA, a bot by Emzi0767";
                        embed.Color = new Color(255, 127, 0);
                        AdaBotCore.AdaClient.SendEmbed(embed, feed.ChannelId);
                    }
                }
            }
            if (changed)
                UpdateConfig();
        }
    }
}
