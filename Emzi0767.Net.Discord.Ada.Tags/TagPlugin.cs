using System.Collections.Generic;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.Ada.Tags
{
    [Plugin("Tag Plugin")]
    public static class TagPlugin
    {
        private static TagPluginConfig Config { get; set; }

        public static void Initialize()
        {
            L.W("ADA TAG", "Initializing Tag plugin");
            LoadConfig();
            L.W("ADA TAG", "Done");
        }

        private static void LoadConfig()
        {
            Config = new TagPluginConfig();
            Config.Load();
        }

        internal static bool AddTag(ulong channel, string tagid, string content)
        {
            return Config.AddTag(channel, new Tag { Id = tagid, Contents = content });
        }

        internal static bool EditTag(ulong channel, string tagid, string content)
        {
            return Config.EditTag(channel, new Tag { Id = tagid, Contents = content });
        }

        internal static bool DeleteTag(ulong channel, string tagid)
        {
            return Config.DeleteTag(channel, tagid);
        }

        internal static Tag GetTag(ulong channel, string tagid)
        {
            return Config.GetTag(channel, tagid);
        }

        internal static IEnumerable<Tag> GetTags(ulong channel)
        {
            return Config.GetTags(channel);
        }
    }
}
