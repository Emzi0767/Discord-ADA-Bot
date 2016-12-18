using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emzi0767.Ada.Extensions;

namespace Emzi0767.Ada.Core
{
    public class AdaSocketManager
    {
        public IWebSocketProvider SocketProvider { get; private set; }
        private Dictionary<string, Assembly> LoadedAssemblies { get; set; }

        public void Initialize()
        {
            L.W("ADA SCK", "Initializing Socket manager");
            var a = typeof(AdaSocketManager).GetTypeInfo().Assembly;
            var l = a.Location;
            l = Path.GetDirectoryName(l);
            var m = Path.Combine(l, "providers");
            this.LoadedAssemblies = new Dictionary<string, Assembly>();
            FrameworkAssemblyLoader.ResolvingAssembly += ResolvingProvider;

            if (Directory.Exists(m))
            {
                var x = Directory.GetFiles(m, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (var xx in x)
                {
                    L.W("ADA SCK", "Loaded provider file '{0}'", xx);
                    var xa = FrameworkAssemblyLoader.LoadFile(xx);
                    this.LoadedAssemblies[xa.GetName().Name] = xa;
                }

                var tpi = typeof(IWebSocketProvider);
                var tpp = this.LoadedAssemblies.SelectMany(xa => xa.Value.ExportedTypes).FirstOrDefault(xt => tpi.IsAssignableFrom(xt) && !xt.GetTypeInfo().IsInterface);
                if (tpp != null)
                {
                    var sp = (IWebSocketProvider)Activator.CreateInstance(tpp);
                    this.SocketProvider = sp;
                    L.W("ADA SCK", "Loaded socket provider: '{0}'", sp.GetType().ToString());
                }
            }

            L.W("ADA SCK", "Done");
        }

        private Assembly ResolvingProvider(string assembly_name)
        {
            if (this.LoadedAssemblies.ContainsKey(assembly_name))
                return this.LoadedAssemblies[assembly_name];
            return null;
        }
    }
}
