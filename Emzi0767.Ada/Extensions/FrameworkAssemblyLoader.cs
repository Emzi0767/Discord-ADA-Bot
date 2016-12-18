using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Emzi0767.Ada.Extensions
{
    public static class FrameworkAssemblyLoader
    {
        public static bool UsingCore { get; private set; }
        private static Func<string, Assembly> LoadAssembly { get; set; }

        static FrameworkAssemblyLoader()
        {
            var type = Type.GetType("System.Runtime.Loader.AssemblyLoadContext");
            if (type != null)
                UsingCore = true;

            var ldrmtd = (MethodInfo)null;
            if (UsingCore)
            {
                var prop = type.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);
                var ldr = prop.GetValue(null);
                var ldt = ldr.GetType();
                ldrmtd = ldt.GetMethod("LoadFromAssemblyPath", BindingFlags.Public | BindingFlags.Static);
            }
            else
            {
                type = Type.GetType("System.Reflection.Assembly");
                ldrmtd = type.GetMethod("LoadFile", BindingFlags.Public | BindingFlags.Static);
            }

            if (ldrmtd == null)
                throw new InvalidOperationException("Could not determine .NET Assembly loading method");

            var mtdarg = Expression.Parameter(typeof(string));
            LoadAssembly = Expression.Lambda<Func<string, Assembly>>(Expression.Call(null, ldrmtd, mtdarg), mtdarg).Compile();
        }

        public static Assembly LoadFile(string filename)
        {
            return LoadAssembly(filename);
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
