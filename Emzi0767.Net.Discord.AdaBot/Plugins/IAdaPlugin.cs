namespace Emzi0767.Net.Discord.AdaBot.Plugins
{
    public interface IAdaPlugin
    {
        string Name { get; }
        void Initialize();
    }
}
