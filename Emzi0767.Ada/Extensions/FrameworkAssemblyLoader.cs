using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emzi0767.AssemblyResolver;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace Emzi0767.Ada.Extensions
{
    public static class FrameworkAssemblyLoader
    {
        private static Resolver AssemblyResolver { get; set; }

        static FrameworkAssemblyLoader()
        {
            AssemblyResolver = new Resolver();
            AssemblyResolver.Resolving += Resolver_Resolving;
        }

        public static Assembly LoadFile(string filename)
        {
            return AssemblyResolver.LoadFromFile(filename);
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            var rid = RuntimeEnvironment.GetRuntimeIdentifier();
            var ass = DependencyContext.Default.GetRuntimeAssemblyNames(rid);

            return ass.Select(xan => Assembly.Load(xan)).ToArray();
        }

        private static Assembly Resolver_Resolving(string name)
        {
            return ResolveAssemblyFire(name);
        }

        private static Assembly ResolveAssemblyFire(string name)
        {
            if (ResolvingAssembly != null)
                return ResolvingAssembly(name);
            return null;
        }

        public static event AssemblyResolveEventHandler ResolvingAssembly;
    }
}
