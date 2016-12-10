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
        public int ConfigCount { get { return this.DeclaredConfigs.Count; } }
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

        internal IEnumerable<KeyValuePair<ulong, AdaGuildConfig>> GetGuildConfigs()
        {
            if (this.GuildConfigs == null)
                yield break;

            foreach (var kvp in this.GuildConfigs)
                yield return kvp;
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
                gcf.DeleteCommands = gconf["delete_commands"] != null ? (bool?)gconf["delete_commands"] : null;
                gcf.CommandPrefix = gconf["command_prefix"] != null ? (string)gconf["command_prefix"] : null;
                gcf.MuteRole = gconf["mute_role"] != null ? (ulong?)gconf["mute_role"] : null;
                var jma = gconf["mod_actions"] != null ? (JArray)gconf["mod_actions"] : new JArray();
                foreach (var xjma in jma)
                {
                    var xma = (JObject)xjma;
                    var ma = new AdaModAction
                    {
                        ActionType = (AdaModActionType)(byte)xma["type"],
                        Issued = (DateTime)xma["issued"],
                        Issuer = (ulong)xma["issuer"],
                        Reason = (string)xma["reason"],
                        Until = (DateTime)xma["until"],
                        UserId = (ulong)xma["user"]
                    };
                    gcf.ModActions.Add(ma);
                }

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
                if (kvp.Value.DeleteCommands != null)
                    gconf.Add("delete_commands", kvp.Value.DeleteCommands);
                if (!string.IsNullOrWhiteSpace(kvp.Value.CommandPrefix))
                    gconf.Add("command_prefix", kvp.Value.CommandPrefix);
                if (kvp.Value.MuteRole != null)
                    gconf.Add("mute_role", kvp.Value.MuteRole.Value);
                var jma = new JArray();
                foreach (var ma in kvp.Value.ModActions)
                {
                    var xjma = new JObject();
                    xjma.Add("type", (byte)ma.ActionType);
                    xjma.Add("issued", ma.Issued);
                    xjma.Add("issuer", ma.Issuer);
                    xjma.Add("reason", ma.Reason ?? string.Empty);
                    xjma.Add("until", ma.Until);
                    xjma.Add("user", ma.UserId);
                    jma.Add(xjma);
                }
                gconf.Add("mod_actions", jma);

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
