﻿using System.Reflection;
using Emzi0767.AssemblyResolver;

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
