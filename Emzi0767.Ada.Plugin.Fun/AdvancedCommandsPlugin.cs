using System;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Plugins;

namespace Emzi0767.Ada.Plugin.AdvancedCommands
{
    public class AdvancedCommandsPlugin : IAdaPlugin
    {
        internal static AdvancedCommandsPlugin Instance { get; private set; }
        
        public string Name { get { return "Advanced Commands Plugin"; } }
        public IAdaPluginConfig Config { get { return this.conf; } }
        public Type ConfigType { get { return typeof(AdvancedCommandsPluginConfig); } }

        private AdvancedCommandsPluginConfig conf;

        public void Initialize()
        {
            L.W("ADA DAC", "Loading Advanced Commands Config");
            Instance = this;
            L.W("ADA DAC", "Done");
        }

        public void LoadConfig(IAdaPluginConfig config)
        {
            var cfg = config as AdvancedCommandsPluginConfig;
            if (cfg != null)
                this.conf = cfg;
        }

        public bool IsEnabled(string command, ulong guild)
        {
            return this.conf.IsEnabled(command, guild);
        }

        public void SetEnabled(string command, ulong guild, bool state)
        {
            this.conf.SetEnabled(command, guild, state);
            L.W("ADA DAC", "Command config updated");
        }

        public void SetEnabled(string[] commands, ulong guild, bool state)
        {
            this.conf.SetEnabled(commands, guild, state);
            L.W("ADA DAC", "Command config updated");
        }
    }
}
