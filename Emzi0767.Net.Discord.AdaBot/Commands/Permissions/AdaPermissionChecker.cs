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
            if (prm == AdaPermission.None && user.GuildPermissions.Administrator)
                return true;
            if ((user.GuildPermissions.RawValue & (uint)prm) == (uint)prm)
                return true;
            error = "Insufficient Permissions";
            return false;
        }
    }
}
