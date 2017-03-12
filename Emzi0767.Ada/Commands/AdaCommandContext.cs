using Discord;
using Discord.Commands;

namespace Emzi0767.Ada.Commands
{
    public class AdaCommandContext : CommandContext
    {
        internal AdaCommandContext(IDiscordClient client, IUserMessage msg)
            : base(client, msg)
        {
        }
    }
}
