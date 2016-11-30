using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.AdaBot.Config
{
    internal class AdaConfigManager
    {
        private Dictionary<Type, IAdaPluginConfig> DeclaredConfigs { get; set; }

        internal AdaConfigManager()
        {
            L.W("ADA CFG", "Initializing ADA Config Manager");
            this.DeclaredConfigs = new Dictionary<Type, IAdaPluginConfig>();
            L.W("ADA CFG", "Done");
        }

        internal void Initialize()
        {
            L.W("ADA CFG", "Initializing ADA Plugin Configs");
            
            // load all configs

            L.W("ADA CFG", "Done");
        }
    }
}
