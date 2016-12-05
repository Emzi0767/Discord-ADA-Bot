using System;
using System.Reflection;

namespace Emzi0767.Ada.Plugins
{
    internal class AdaPlugin
    {
        public IAdaPlugin Plugin { get; set; }
        public string Name { get { return this.Plugin.Name; } }
        public Assembly DeclaringAssembly { get { return this.Plugin.GetType().GetTypeInfo().Assembly; } }
        public Type EntryType { get { return this.Plugin.GetType(); } }
    }
}
