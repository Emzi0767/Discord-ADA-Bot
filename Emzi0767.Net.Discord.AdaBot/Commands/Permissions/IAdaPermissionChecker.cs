using Discord;

namespace Emzi0767.Net.Discord.AdaBot.Commands.Permissions
{
    public interface IAdaPermissionChecker
    {
        string Id { get; }
        bool CanRun(AdaCommand cmd, IGuildUser user, IMessage message, IMessageChannel channel, IGuild guild, out string error);
    }
}
