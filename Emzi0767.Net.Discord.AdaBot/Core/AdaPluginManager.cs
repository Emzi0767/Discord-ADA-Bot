using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.AdaBot.Core
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

        public void Initialize()
        {
            L.W("ADA PLG", "Registering and initializing plugins");

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
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var @as = AppDomain.CurrentDomain.GetAssemblies();
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var pt = typeof(PluginAttribute);
            foreach (var t in ts)
            {
                var xpt = (PluginAttribute)Attribute.GetCustomAttribute(t, pt);
                if (xpt == null)
                    continue;

                L.W("ADA PLG", "Type {0} is a plugin", t.ToString());

                string initm = "Initialize";
                if (!string.IsNullOrWhiteSpace(xpt.InitializerMethod))
                    initm = xpt.InitializerMethod;

                var mtd = (MethodInfo)null;
                foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (m.Name == initm)
                    {
                        mtd = m;
                        break;
                    }
                }
                if (mtd == null)
                {
                    L.W("ADA PLG", "Plugin '{0}' failed to provide valid initializer method; skipping...", xpt.Name);
                }

                var plg = new AdaPlugin { Name = xpt.Name, Initializer = mtd, DeclaringAssembly = t.Assembly, EntryType = t };
                this.RegisteredPlugins.Add(xpt.Name, plg);
                L.W("ADA PLG", "Registered plugin {0} with initializer {1}", xpt.Name, initm);
                mtd.Invoke(null, null);
                L.W("ADA PLG", "Plugin '{0}' initialized", xpt.Name);
            }
            L.W("ADA PLG", "Registered and initialized {0:#,##0} plugins", this.RegisteredPlugins.Count);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (this.LoadedAssemblies.ContainsKey(args.Name))
                return this.LoadedAssemblies[args.Name];
            return null;
        }
    }
}
