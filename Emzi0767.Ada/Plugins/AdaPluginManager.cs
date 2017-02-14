using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Discord.Commands;
using Emzi0767.Ada.Commands;
using Emzi0767.Ada.Extensions;

namespace Emzi0767.Ada.Plugins
{
    public class AdaPluginManager
    {
        public IEnumerable<Type> Modules { get { return this.FoundModules.AsEnumerable(); } }

        private List<Type> FoundModules { get; set; }
        private Dictionary<string, Assembly> LoadedAssemblies { get; set; }

        public AdaPluginManager()
        {
            this.FoundModules = new List<Type>();
        }

        internal void Initialize()
        {
            L.W("ADA PLUGIN", "Locating assemblies");
            var a = typeof(AdaPluginManager).GetTypeInfo().Assembly;
            var l = Path.GetDirectoryName(a.Location);

            L.W("ADA PLUGIN", "Looking for references");
            FrameworkAssemblyLoader.ResolvingAssembly += ResolvingAssembly;
            var asfs = Directory.GetFiles(Path.Combine(l, "references"), "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var asf in asfs)
            {
                var xa = FrameworkAssemblyLoader.LoadFile(asf);
                this.LoadedAssemblies.Add(xa.GetName().Name, xa);
            }

            L.W("ADA PLUGIN", "Registering core modules");
            var mt = typeof(ModuleBase<AdaCommandContext>);
            var ea = Assembly.GetEntryAssembly();
            var cms = ea
                .DefinedTypes
                .Select(xti => xti.AsType())
                .Where(xt => !xt.IsNested && xt.HasParentType(mt));
            this.FoundModules.AddRange(cms);

            L.W("ADA PLUGIN", "Looking for modules");
            asfs = Directory.GetFiles(Path.Combine(l, "plugins"), "*.dll", SearchOption.TopDirectoryOnly);
            foreach (var asf in asfs)
            {
                var xa = FrameworkAssemblyLoader.LoadFile(asf);
                this.LoadedAssemblies.Add(xa.GetName().Name, xa);

                var xts = xa.DefinedTypes
                    .Select(xti => xti.AsType())
                    .Where(xt => !xt.IsNested && xt.HasParentType(mt));
                this.FoundModules.AddRange(xts);
            }
        }

        private Assembly ResolvingAssembly(string assembly_name)
        {
            if (this.LoadedAssemblies.ContainsKey(assembly_name))
                return this.LoadedAssemblies[assembly_name];

            return null;
        }
    }
}
