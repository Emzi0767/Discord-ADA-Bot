using System.Collections.Generic;
using Emzi0767.Ada.Config;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Ada.Plugin.AdvancedCommands
{
    public class AdvancedCommandsPluginConfig : IAdaPluginConfig
    {
        public IAdaPluginConfig DefaultConfig { get { return new AdvancedCommandsPluginConfig(); } }

        private Dictionary<ulong, Dictionary<string, bool>> CommandConfiguration { get; set; }

        public AdvancedCommandsPluginConfig()
        {
            this.CommandConfiguration = new Dictionary<ulong, Dictionary<string, bool>>();
        }

        public bool IsEnabled(string command, ulong guild)
        {
            if (!this.CommandConfiguration.ContainsKey(guild))
                return true;

            var sd = this.CommandConfiguration[guild];
            if (!sd.ContainsKey(command))
                return true;

            return sd[command];
        }

        public void SetEnabled(string command, ulong guild, bool state)
        {
            if (!CommandConfiguration.ContainsKey(guild))
                CommandConfiguration[guild] = new Dictionary<string, bool>();

            var sd = CommandConfiguration[guild];
            sd[command] = state;

            AdaBotProgram.ConfigurationManager.UpdateConfig(AdvancedCommandsPlugin.Instance);
        }

        public void SetEnabled(string[] commands, ulong guild, bool state)
        {
            if (!CommandConfiguration.ContainsKey(guild))
                CommandConfiguration[guild] = new Dictionary<string, bool>();

            var sd = CommandConfiguration[guild];
            foreach (var command in commands)
                sd[command] = state;

            AdaBotProgram.ConfigurationManager.UpdateConfig(AdvancedCommandsPlugin.Instance);
        }

        public void Load(JObject jo)
        {
            foreach (var kvp in jo)
            {
                var srv = ulong.Parse(kvp.Key);
                var conf = (JObject)kvp.Value;
                var dconf = new Dictionary<string, bool>();
                foreach (var xkvp in conf)
                    dconf[xkvp.Key] = (bool)xkvp.Value;
                this.CommandConfiguration[srv] = dconf;
            }
        }

        public JObject Save()
        {
            var jo = new JObject();
            foreach (var xsd in CommandConfiguration)
            {
                var xjo = new JObject();
                foreach (var xcc in xsd.Value)
                    xjo.Add(xcc.Key, xcc.Value);
                jo.Add(xsd.Key.ToString(), xjo);
            }
            return jo;
        }
    }
}
