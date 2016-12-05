using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emzi0767.Ada.Plugins;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Ada.Config
{
    public class AdaConfigManager
    {
        private Dictionary<ulong, AdaGuildConfig> GuildConfigs { get; set; }
        private Dictionary<Type, IAdaPluginConfig> DeclaredConfigs { get; set; }

        internal AdaConfigManager()
        {
            L.W("ADA CFG", "Initializing ADA Config Manager");
            this.DeclaredConfigs = new Dictionary<Type, IAdaPluginConfig>();
            this.GuildConfigs = new Dictionary<ulong, AdaGuildConfig>();
            L.W("ADA CFG", "Done");
        }

        public void UpdateConfig(IAdaPlugin plugin)
        {
            this.DeclaredConfigs[plugin.ConfigType] = plugin.Config;
            this.WriteConfigs();
        }

        public AdaGuildConfig GetGuildConfig(ulong guild_id)
        {
            if (this.GuildConfigs.ContainsKey(guild_id))
                return this.GuildConfigs[guild_id];
            return new AdaGuildConfig();
        }

        internal void SetGuildConfig(ulong guild_id, AdaGuildConfig conf)
        {
            this.GuildConfigs[guild_id] = conf;
            this.WriteConfigs();
        }

        internal void Initialize()
        {
            L.W("ADA CFG", "Initializing ADA Plugin Configs");
            var jconfig = AdaBotCore.AdaClient.ConfigJson;

            var gconfs = (JObject)jconfig["guild_config"];
            foreach (var kvp in gconfs)
            {
                var guild = ulong.Parse(kvp.Key);
                var gconf = (JObject)kvp.Value;

                var gcf = new AdaGuildConfig();
                gcf.ModLogChannel = gconf["modlog"] != null ? (ulong?)gconf["modlog"] : null;

                this.GuildConfigs[guild] = gcf;
            }

            var confnode = (JArray)jconfig["conf_manager"];
            var confs = new Dictionary<string, JObject>();
            foreach (var xconf in confnode)
            {
                var type = (string)xconf["type"];
                var conf = (JObject)xconf["config"];
                confs.Add(type, conf);
            }

            var @as = AdaBotCore.PluginManager.PluginAssemblies;
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var pt = typeof(IAdaPluginConfig);
            foreach (var t in ts)
            {
                if (!pt.IsAssignableFrom(t.AsType()) || !t.IsClass || t.IsAbstract)
                    continue;

                L.W("ADA PLG", "Type {0} is a plugin config", t.ToString());
                var iplg = (IAdaPluginConfig)Activator.CreateInstance(t.AsType());
                var icfg = iplg.DefaultConfig;
                if (confs.ContainsKey(t.ToString()))
                    icfg.Load(confs[t.ToString()]);
                this.DeclaredConfigs.Add(t.AsType(), icfg);
            }
            L.W("ADA CFG", "Done");
        }

        internal IAdaPluginConfig GetConfig(IAdaPlugin plugin)
        {
            if (this.DeclaredConfigs.ContainsKey(plugin.ConfigType))
                return this.DeclaredConfigs[plugin.ConfigType];
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

            var gconfs = new JObject();
            foreach (var kvp in this.GuildConfigs)
            {
                var gconf = new JObject();
                if (kvp.Value.ModLogChannel != null)
                    gconf.Add("modlog", kvp.Value.ModLogChannel.Value);

                gconfs.Add(kvp.Key.ToString(), gconf);
            }

            var jconf = AdaBotCore.AdaClient.ConfigJson;

            if (jconf["conf_manager"] != null)
                jconf.Remove("conf_manager");
            jconf.Add("conf_manager", confs);

            if (jconf["guild_config"] != null)
                jconf.Remove("guild_config");
            jconf.Add("guild_config", gconfs);

            AdaBotCore.AdaClient.WriteConfig();
        }
    }
}
