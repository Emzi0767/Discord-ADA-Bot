using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Emzi0767.Ada.Sql;
using Newtonsoft.Json;

namespace Emzi0767.Ada.Config
{
    public class AdaConfigurationManager
    {
        public AdaBotConfiguration BotConfiguration { get; private set; }
        public AdaSqlManager SqlManager { get; private set; }

        private ConcurrentDictionary<ulong, AdaGuildConfiguration> GuildCongfigurationStore { get; set; }

        internal AdaConfigurationManager()
        {
            this.BotConfiguration = null;
            this.GuildCongfigurationStore = new ConcurrentDictionary<ulong, AdaGuildConfiguration>();
            this.SqlManager = null;
        }

        internal void Initialize()
        {
            L.W("ADA CFG", "Initializing ADA Plugin Configs");

            var json = File.ReadAllText("config.json", new UTF8Encoding(false));
            this.BotConfiguration = JsonConvert.DeserializeObject<AdaBotConfiguration>(json);
        }

        internal AdaSqlManager CreateSqlManager()
        {
            this.SqlManager = new AdaSqlManager(this.BotConfiguration.PostgreSQL);
            AdaGuildConfiguration.SqlManager = this.SqlManager;

            return this.SqlManager;
        }

        internal async Task CacheGuildAsync(SocketGuild gld)
        {
            var gconf = new AdaGuildConfiguration(gld);
            await gconf.InitializeAsync();
            this.GuildCongfigurationStore[gld.Id] = gconf;
        }

        internal void UncacheGuild(SocketGuild gld)
        {
            var cfg = (AdaGuildConfiguration)null;
            this.GuildCongfigurationStore.TryRemove(gld.Id, out cfg);
        }

        public AdaGuildConfiguration GetConfiguration(SocketGuild gld)
        {
            if (this.GuildCongfigurationStore.ContainsKey(gld.Id))
                return this.GuildCongfigurationStore[gld.Id];

            return null;
        }
    }
}
