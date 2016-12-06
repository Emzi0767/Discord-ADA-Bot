using System;
using System.Collections.Generic;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Plugins;

namespace Emzi0767.Ada.Plugin.Tags
{
    public class TagPlugin : IAdaPlugin
    {
        public IAdaPluginConfig Config { get { return this.conf; } }
        public Type ConfigType { get { return typeof(TagPluginConfig); } }
        public string Name { get { return "Tag Plugin"; } }

        public static TagPlugin Instance { get; private set; }

        private TagPluginConfig conf;

        public void Initialize()
        {
            L.W("ADA TAG", "Initializing Tag plugin");
            Instance = this;
            L.W("ADA TAG", "Tag plugin initialized");
        }

        public void LoadConfig(IAdaPluginConfig config)
        {
            var cfg = config as TagPluginConfig;
            if (cfg != null)
                this.conf = cfg;
        }

        internal bool AddTag(ulong channel, string tagid, string content)
        {
            return this.conf.AddTag(channel, new Tag { Id = tagid, Contents = content });
        }

        internal bool EditTag(ulong channel, string tagid, string content)
        {
            return this.conf.EditTag(channel, new Tag { Id = tagid, Contents = content });
        }

        internal bool DeleteTag(ulong channel, string tagid)
        {
            return this.conf.DeleteTag(channel, tagid);
        }

        internal Tag GetTag(ulong channel, string tagid)
        {
            return this.conf.GetTag(channel, tagid);
        }

        internal IEnumerable<Tag> GetTags(ulong channel)
        {
            return this.conf.GetTags(channel);
        }
    }
}
