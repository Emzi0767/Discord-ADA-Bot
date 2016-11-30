using System;
using System.Collections.Generic;
using System.Linq;
using Emzi0767.Net.Discord.AdaBot.Config;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.Ada.Feedle
{
    internal class FeedlePluginConfig : IAdaPluginConfig
    {
        public IAdaPluginConfig DefaultConfig
        {
            get
            {
                return new FeedlePluginConfig
                {
                    Feeds = new List<Feed>()
                };
            }
        }

        public FeedlePluginConfig()
        {
            this.Feeds = new List<Feed>();
        }

        public List<Feed> Feeds { get; private set; }

        public void Load(JObject jo)
        {
            var ja = (JArray)jo["feeds"];
            foreach (var xjt in ja)
            {
                var xjo = (JObject)xjt;

                var tag = (string)xjo["tag"];
                var uri_ = (string)xjo["uri"];
                var uri = new Uri(uri_);
                var chn = (ulong)xjo["channel"];
                var ris = (JArray)xjo["recent"];
                this.Feeds.Add(new Feed(uri, chn, tag) { RecentUris = ris.Select(xjv => (string)xjv).ToList() });
            }
        }

        public JObject Save()
        {
            var ja = new JArray();

            foreach (var feed in this.Feeds)
            {
                var xjo = new JObject();
                xjo.Add("tag", feed.Tag);
                xjo.Add("uri", feed.FeedUri.ToString());
                xjo.Add("channel", feed.ChannelId);
                xjo.Add("recent", new JArray(feed.RecentUris));
                ja.Add(xjo);
            }

            var jo = new JObject();
            jo.Add("feeds", ja);
            return jo;
        }
    }
}
