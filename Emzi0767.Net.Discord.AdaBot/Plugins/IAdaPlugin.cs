using Emzi0767.Net.Discord.AdaBot.Config;

namespace Emzi0767.Net.Discord.AdaBot.Plugins
{
    public interface IAdaPlugin
    {
        string Name { get; }
        IAdaPluginConfig Config { get; }
        void Initialize();
        void LoadConfig(IAdaPluginConfig config);
    }
}
