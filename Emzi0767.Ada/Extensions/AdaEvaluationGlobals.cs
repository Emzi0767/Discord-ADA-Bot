using Discord.WebSocket;
using Emzi0767.Ada.Core;

namespace Emzi0767.Ada.Extensions
{
    public class AdaEvaluationGlobals
    {
        public SocketUserMessage Message { get; set; }
        public SocketTextChannel Channel { get { return this.Message.Channel as SocketTextChannel; } }
        public SocketGuild Guild { get { return this.Channel.Guild; } }
        public SocketGuildUser User { get { return this.Message.Author as SocketGuildUser; } }

        public AdaUtilities Utilities { get; set; }
        public AdaClient Client { get { return this.Utilities.AdaClient; } }
        public DiscordSocketClient Discord { get { return this.Client.DiscordClient; } }
    }
}
