using System;
using Emzi0767.Ada.Config;

namespace Emzi0767.Ada.Plugins
{
    public interface IAdaPlugin
    {
        string Name { get; }
        IAdaPluginConfig Config { get; }
        Type ConfigType { get; }
        void Initialize();
        void LoadConfig(IAdaPluginConfig config);
    }
}
