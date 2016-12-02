using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.Ada.Tags
{
    internal class TagPluginConfig
    {
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
            this.Save();
            return true;
        }

        public bool EditTag(ulong channel, Tag tag)
        {
            if (!this.Tags.ContainsKey(channel))
                return false;

            if (!this.Tags[channel].ContainsKey(tag.Id))
                return false;

            this.Tags[channel][tag.Id] = tag;
            this.Save();
            return true;
        }

        public bool DeleteTag(ulong channel, string id)
        {
            if (!this.Tags.ContainsKey(channel))
                return false;

            if (!this.Tags[channel].ContainsKey(id))
                return false;

            this.Tags[channel].Remove(id);
            this.Save();
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

        private void Save()
        {
            var jo = new JObject();
            foreach (var chdef in this.Tags)
            {
                var xjo = new JObject();
                foreach (var tag in chdef.Value)
                    xjo.Add(tag.Key, tag.Value.Contents);
                jo.Add(chdef.Key.ToString(), xjo);
            }

            var json = jo.ToString();
            var a = Assembly.GetEntryAssembly();
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            l = Path.Combine(l, "tags.json");
            File.WriteAllText(l, json, new UTF8Encoding(false));
        }

        public void Load()
        {
            var a = Assembly.GetEntryAssembly();
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            l = Path.Combine(l, "tags.json");
            if (!File.Exists(l))
                return;
            var json = File.ReadAllText(l, new UTF8Encoding(false));

            var jo = JObject.Parse(json);
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
    }
}