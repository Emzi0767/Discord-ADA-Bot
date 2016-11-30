using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.AdaBot.Config
{
    public interface IAdaPluginConfig
    {
        /// <summary>
        /// Gets the default configuration.
        /// </summary>
        IAdaPluginConfig DefaultConfig { get; }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <param name="jo">JSON to load from.</param>
        void Load(JObject jo);

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <returns>Saved JSON.</returns>
        JObject Save();
    }
}
