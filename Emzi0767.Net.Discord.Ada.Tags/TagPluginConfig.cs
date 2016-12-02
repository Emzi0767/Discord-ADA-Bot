using System.Collections.Generic;
using Emzi0767.Net.Discord.AdaBot;
using Emzi0767.Net.Discord.AdaBot.Config;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.Ada.Tags
{
    public class TagPluginConfig : IAdaPluginConfig
    {
        public IAdaPluginConfig DefaultConfig { get { return new TagPluginConfig(); } }

        private Dictionary<ulong, Dictionary<string, Tag>> Tags { get; set; }

        public TagPluginConfig()
        {
            this.Tags = new Dictionary<ulong, Dictionary<string, Tag>>();
        }

        public bool AddTag(ulong channel, Tag tag)
        {
            if (!this.Tags.ContainsKey(channel))
                this.Tags[channel] = new Dictionary<string, Tag>();

            if (this.Tags[channel].ContainsKey(tag.Id))
                return false;

            this.Tags[channel][tag.Id] = tag;
            AdaBotCore.ConfigManager.UpdateConfig(TagPlugin.Instance);
            return true;
        }

        public bool EditTag(ulong channel, Tag tag)
        {
            if (!this.Tags.ContainsKey(channel))
                return false;

            if (!this.Tags[channel].ContainsKey(tag.Id))
                return false;

            this.Tags[channel][tag.Id] = tag;
            AdaBotCore.ConfigManager.UpdateConfig(TagPlugin.Instance);
            return true;
        }

        public bool DeleteTag(ulong channel, string id)
        {
            if (!this.Tags.ContainsKey(channel))
                return false;

            if (!this.Tags[channel].ContainsKey(tag.Id))
                return false;

            this.Tags[channel].Remove(id);
            AdaBotCore.ConfigManager.UpdateConfig(TagPlugin.Instance);
            return true;
        }

        public Tag GetTag(ulong channel, string id)
        {
            if (!this.Tags.ContainsKey(channel))
                return null;

            if (!this.Tags[channel].ContainsKey(id))
                return null;

            return this.Tags[channel][id];
        }

        public IEnumerable<Tag> GetTags(ulong channel)
        {
            if (!this.Tags.ContainsKey(channel))
                yield break;

            foreach (var tagset in this.Tags[channel])
                yield return tagset.Value;
        }

        public void Load(JObject jo)
        {
            foreach (var kvp in jo)
            {
                var ch = ulong.Parse(kvp.Key);
                var chdef = (JObject)kvp.Value;
                var chdic = new Dictionary<string, Tag>();
                foreach (var tag in chdef)
                {
                    chdic.Add(tag.Key, new Tag { Id = tag.Key, Contents = (string)tag.Value });
                }
                this.Tags.Add(ch, chdic);
            }
        }

        public JObject Save()
        {
            var jo = new JObject();
            foreach (var chdef in this.Tags)
            {
                var xjo = new JObject();
                foreach (var tag in chdef.Value)
                    xjo.Add(tag.Key, tag.Value.Contents);
                jo.Add(chdef.Key.ToString(), xjo);
            }
            return jo;
        }
    }
}