using Discord;
using Emzi0767.Net.Discord.AdaBot.Commands;
using Emzi0767.Net.Discord.AdaBot.Commands.Permissions;

namespace Emzi0767.Net.Discord.Ada.AdvancedCommands
{
    public class AdvancedCommandsPermissionChecker : IAdaPermissionChecker
    {
        public string Id { get { return "ACPChecker"; } }

        public bool CanRun(AdaCommand cmd, IGuildUser user, IMessage message, IMessageChannel channel, IGuild guild, out string error)
        {
            var srv = guild.Id;
            var can = AdvancedCommandsPlugin.Instance.IsEnabled(cmd.Name, srv);
            error = "";
            if (can)
                return true;
            error = "This command was disabled on this server";
            return false;
        }
    }
}
