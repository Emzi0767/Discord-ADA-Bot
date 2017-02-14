using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Emzi0767.Ada.Commands.Permissions
{
    public class AdaPermissionRequiredAttribute : PreconditionAttribute
    {
        public AdaPermission Permission { get; private set; }

        public AdaPermissionRequiredAttribute(AdaPermission permission)
        {
            this.Permission = permission;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            await Task.Yield();

            var prm = this.Permission;
            var prl = (ulong)prm;
            var chn = context.Channel as SocketGuildChannel;
            var usr = context.User as SocketGuildUser;

            if (chn == null || usr == null)
                return PreconditionResult.FromError("Invalid user or channel specified");

            var chp = usr.GetPermissions(chn);
            if (prm == AdaPermission.None && usr.GuildPermissions.Administrator)
                return PreconditionResult.FromSuccess();

            if ((chp.RawValue & prl) == prl || (usr.GuildPermissions.RawValue & prl) == prl)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Insufficient permissions");
        }
    }
}
