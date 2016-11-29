using Discord;
using Discord.Commands;
using Discord.Commands.Permissions;
using Emzi0767.Net.Discord.AdaBot.Attributes;

namespace Emzi0767.Net.Discord.Ada.AdvancedCommands
{
    [AdaPermissionChecker("ACPChecker")]
    public class AdvancedCommandsPermissionChecker : IPermissionChecker
    {
        public bool CanRun(Command command, User user, Channel channel, out string error)
        {
            var srv = channel.Server.Id;
            var can = AdvancedCommandsPlugin.GetEnabledState(command.Text, srv);
            error = "";
            if (can)
                return true;
            error = "This command was disabled on this server";
            return false;
        }
    }
}
