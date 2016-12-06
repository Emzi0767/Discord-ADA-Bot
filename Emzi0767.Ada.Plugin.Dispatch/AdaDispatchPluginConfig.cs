using Emzi0767.Ada.Config;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Ada.Plugin.Dispatch
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
