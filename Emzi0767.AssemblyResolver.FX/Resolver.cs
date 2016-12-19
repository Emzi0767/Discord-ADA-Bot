using System;
using System.Reflection;

namespace Emzi0767.AssemblyResolver
{
    public delegate Assembly AssemblyResolveEventHandler(string name);

    public class Resolver
    {
        public Resolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public Assembly LoadFromFile(string filename)
        {
            return Assembly.LoadFile(filename);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var an = new AssemblyName(args.Name);
            return this.FireResolving(an.Name);
        }

        private Assembly FireResolving(string name)
        {
            if (this.Resolving != null)
                return this.Resolving(name);
            return null;
        }

        public event AssemblyResolveEventHandler Resolving;
    }
}
