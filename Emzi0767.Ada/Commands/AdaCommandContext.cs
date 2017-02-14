using Discord;
using Discord.Commands;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Core;
using Emzi0767.Ada.Sql;

namespace Emzi0767.Ada.Commands
{
    public class AdaCommandContext : CommandContext
    {
        public AdaUtilities Utilities { get; private set; }
        public AdaConfigurationManager Configuration { get; private set; }
        public AdaSqlManager SqlManager { get; private set; }

        internal AdaCommandContext(IDiscordClient client, IUserMessage msg, AdaUtilities utils, AdaConfigurationManager config, AdaSqlManager sqlman)
            : base(client, msg)
        {
            this.Utilities = utils;
            this.Configuration = config;
            this.SqlManager = sqlman;
        }
    }
}
