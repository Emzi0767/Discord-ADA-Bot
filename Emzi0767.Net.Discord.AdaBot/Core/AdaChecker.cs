using Discord;
using Discord.Commands;
using Discord.Commands.Permissions;
using Emzi0767.Net.Discord.AdaBot.Attributes;

namespace Emzi0767.Net.Discord.AdaBot.Core
{
    [Checker("CoreAdminChecker")]
    public class AdaChecker : IPermissionChecker
    {
        public bool CanRun(Command command, User user, Channel channel, out string error)
        {
            //error = "";
            //if (user.ServerPermissions.Administrator)
            //    return true;
            //error = "Insufficient Permissions";
            //return false;

            error = "";
            var cmd_ = command.Text;
            var cmd = AdaBotCore.Handler.GetCommand(cmd_);
            var prm = cmd.RequiredPermission;
            var chp = user.GetPermissions(channel);
            if (prm == AdaPermission.None && user.ServerPermissions.Administrator)
                return true;
            if ((chp.RawValue & (uint)prm) == (uint)prm)
                return true;
            error = "Insufficient Permissions";
            return false;
        }
    }
}
