using System;
using System.Reflection;

namespace Emzi0767.Net.Discord.AdaBot.Plugins
{
    internal class AdaPlugin
    {
        public IAdaPlugin Plugin { get; set; }
        public string Name { get { return this.Plugin.Name; } }
        public Assembly DeclaringAssembly { get { return this.Plugin.GetType().Assembly; } }
        public Type EntryType { get { return this.Plugin.GetType(); } }
    }
}
