using System;
using System.Collections.Generic;
using System.Linq;
using Emzi0767.Net.Discord.AdaBot.Plugins;
using Emzi0767.Tools.MicroLogger;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.AdaBot.Config
{
    public class AdaConfigManager
    {
        private Dictionary<Type, IAdaPluginConfig> DeclaredConfigs { get; set; }

        internal AdaConfigManager()
        {
            L.W("ADA CFG", "Initializing ADA Config Manager");
            this.DeclaredConfigs = new Dictionary<Type, IAdaPluginConfig>();
            L.W("ADA CFG", "Done");
        }

        public void UpdateConfig(IAdaPlugin plugin)
        {
            this.DeclaredConfigs[plugin.Config.GetType()] = plugin.Config;
            this.WriteConfigs();
        }

        internal void Initialize()
        {
            L.W("ADA CFG", "Initializing ADA Plugin Configs");
            var jconfig = AdaBotCore.AdaClient.ConfigJson;
            var confnode = (JArray)jconfig["conf_manager"];
            var confs = new Dictionary<string, JObject>();
            foreach (var xconf in confnode)
            {
                var type = (string)xconf["type"];
                var conf = (JObject)xconf["config"];
                confs.Add(type, conf);
            }

            var @as = AppDomain.CurrentDomain.GetAssemblies();
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var pt = typeof(IAdaPluginConfig);
            foreach (var t in ts)
            {
                if (!pt.IsAssignableFrom(t) || !t.IsClass || t.IsAbstract)
                    continue;

                L.W("ADA PLG", "Type {0} is a plugin config", t.ToString());
                var iplg = (IAdaPluginConfig)Activator.CreateInstance(t);
                var icfg = iplg.DefaultConfig;
                if (confs.ContainsKey(t.ToString()))
                    icfg.Load(confs[t.ToString()]);
                this.DeclaredConfigs.Add(t, icfg);
            }
            L.W("ADA CFG", "Done");
        }

        internal IAdaPluginConfig GetConfig(IAdaPlugin plugin)
        {
            if (this.DeclaredConfigs.ContainsKey(plugin.Config.GetType()))
                return this.DeclaredConfigs[plugin.Config.GetType()];
            return null;
        }

        internal void WriteConfigs()
        {
            var confs = new JArray();
            foreach (var kvp in this.DeclaredConfigs)
            {
                var conf = new JObject();
                conf.Add("type", kvp.Key.ToString());
                conf.Add("config", kvp.Value.Save());
                confs.Add(conf);
            }
            var jconf = AdaBotCore.AdaClient.ConfigJson;
            if (jconf["conf_manager"] != null)
                jconf.Remove("conf_manager");
            jconf.Add("conf_manager", confs);
            AdaBotCore.AdaClient.WriteConfig();
        }
    }
}
