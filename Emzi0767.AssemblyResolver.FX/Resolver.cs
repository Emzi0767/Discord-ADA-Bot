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

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return this.FireResolving(args.Name);
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
