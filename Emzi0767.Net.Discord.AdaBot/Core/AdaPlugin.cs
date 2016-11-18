using System;
using System.Reflection;

namespace Emzi0767.Net.Discord.AdaBot.Core
{
    internal class AdaPlugin
    {
        public string Name { get; set; }
        public Assembly DeclaringAssembly { get; set; }
        public Type EntryType { get; set; }
        public MethodInfo Initializer { get; set; }
    }
}
