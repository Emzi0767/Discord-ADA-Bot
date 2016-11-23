using Discord;
using Discord.Commands;
using Discord.Commands.Permissions;
using Emzi0767.Net.Discord.AdaBot.Attributes;

namespace Emzi0767.Net.Discord.AdaBot.Core
{
    [Checker("CoreDebugChecker")]
    public class AdaDebugChecker : IPermissionChecker
    {
        public bool CanRun(Command command, User user, Channel channel, out string error)
        {
            error = "This is a debug command. It can be only ran by Emzi0767.";
            if (user.Id == 181875147148361728u && user.Name == "Emzi0767" && user.Discriminator == 1837u)
                return true;
            return false;
        }
    }
}
