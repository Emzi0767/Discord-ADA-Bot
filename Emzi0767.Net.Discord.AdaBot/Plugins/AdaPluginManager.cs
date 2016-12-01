using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.AdaBot.Plugins
{
    internal class AdaPluginManager
    {
        private Dictionary<string, AdaPlugin> RegisteredPlugins { get; set; }
        private Dictionary<string, Assembly> LoadedAssemblies { get; set; }
        public int PluginCount { get { return this.RegisteredPlugins.Count; } }

        public AdaPluginManager()
        {
            L.W("ADA PLG", "Initializing Plugin manager");
            this.RegisteredPlugins = new Dictionary<string, AdaPlugin>();
            L.W("ADA PLG", "Initializer");
        }

        public void LoadAssemblies()
        {
            L.W("ADA PLG", "Loading all plugin assemblies");
            this.LoadedAssemblies = new Dictionary<string, Assembly>();
            var a = Assembly.GetExecutingAssembly();
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            l = Path.Combine(l, "plugins");
            if (Directory.Exists(l))
            {
                var x = Directory.GetFiles(l, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (var xx in x)
                {
                    L.W("ADA PLG", "Loaded file '{0}'", xx);
                    var xa = Assembly.Load(File.ReadAllBytes(xx));
                    this.LoadedAssemblies.Add(xa.FullName, xa);
                }
            }
            L.W("ADA PLG", "Registering dependency resolver");
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            L.W("ADA PLG", "Done");
        }

        public void Initialize()
        {
            L.W("ADA PLG", "Registering and initializing plugins");
            var @as = AppDomain.CurrentDomain.GetAssemblies();
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var pt = typeof(IAdaPlugin);
            foreach (var t in ts)
            {
                if (!pt.IsAssignableFrom(t) || !t.IsClass || t.IsAbstract)
                    continue;

                L.W("ADA PLG", "Type {0} is a plugin", t.ToString());
                var iplg = (IAdaPlugin)Activator.CreateInstance(t);
                var plg = new AdaPlugin { Plugin = iplg };
                this.RegisteredPlugins.Add(plg.Name, plg);
                L.W("ADA PLG", "Registered plugin '{0}'", plg.Name);
                plg.Plugin.Initialize();
                plg.Plugin.LoadConfig(AdaBotCore.ConfigManager.GetConfig(iplg));
                L.W("ADA PLG", "Plugin '{0}' initialized", plg.Name);
            }
            this.UpdateAllConfigs();
            L.W("ADA PLG", "Registered and initialized {0:#,##0} plugins", this.RegisteredPlugins.Count);
        }

        internal void UpdateAllConfigs()
        {
            foreach (var plg in this.RegisteredPlugins)
            {
                AdaBotCore.ConfigManager.UpdateConfig(plg.Value.Plugin);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (this.LoadedAssemblies.ContainsKey(args.Name))
                return this.LoadedAssemblies[args.Name];
            return null;
        }
    }
}
