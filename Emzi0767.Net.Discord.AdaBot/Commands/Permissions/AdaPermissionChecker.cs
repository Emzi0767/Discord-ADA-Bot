using Discord;

namespace Emzi0767.Net.Discord.AdaBot.Commands.Permissions
{
    public class AdaPermissionChecker : IAdaPermissionChecker
    {
        public string Id {  get { return "CoreAdminChecker"; } }

        public bool CanRun(AdaCommand command, IGuildUser user, IMessage message, IMessageChannel channel, IGuild guild, out string error)
        {
            error = "";
            var prm = command.RequiredPermission;
            var chn = channel as IGuildChannel;
            if (chn == null)
                return false;
            var chp = user.GetPermissions(chn);
            if (prm == AdaPermission.None && user.GuildPermissions.Administrator)
                return true;
            if ((chp.RawValue & (uint)prm) == (uint)prm)
                return true;
            error = "Insufficient Permissions";
            return false;
        }
    }
}
