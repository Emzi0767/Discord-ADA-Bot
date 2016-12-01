using System.Collections.Generic;
using Emzi0767.Net.Discord.AdaBot.Config;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.Ada.Stallman
{
    public class StallmanPluginConfig : IAdaPluginConfig
    {
        public IAdaPluginConfig DefaultConfig { get { return new StallmanPluginConfig(); } }

        internal List<ulong> DisabledGuilds { get; set; }

        public StallmanPluginConfig()
        {
            this.DisabledGuilds = new List<ulong>();
        }

        public void Load(JObject jo)
        {
            var disabled = (JArray)jo["disabled"];
            foreach (var xd in disabled)
                this.DisabledGuilds.Add((ulong)xd);
        }

        public JObject Save()
        {
            var disabled = new JArray();
            foreach (var xd in this.DisabledGuilds)
                disabled.Add(xd);
            var jo = new JObject();
            jo.Add("disabled", disabled);
            return jo;
        }
    }
}
