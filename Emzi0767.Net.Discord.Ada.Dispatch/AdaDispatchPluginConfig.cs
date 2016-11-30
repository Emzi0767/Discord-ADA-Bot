using Emzi0767.Net.Discord.AdaBot.Config;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.Ada.Dispatch
{
    public class AdaDispatchPluginConfig : IAdaPluginConfig
    {
        public IAdaPluginConfig DefaultConfig { get { return new AdaDispatchPluginConfig(); } }

        public void Load(JObject jo) { }

        public JObject Save()
        {
            return new JObject();
        }
    }
}
