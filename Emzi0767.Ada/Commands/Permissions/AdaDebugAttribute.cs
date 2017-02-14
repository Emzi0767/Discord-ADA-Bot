using System.Threading.Tasks;
using Discord.Commands;

namespace Emzi0767.Ada.Commands.Permissions
{
    public class AdaDebugAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            await Task.Yield();

            if (context.User.Id == 181875147148361728u)
                return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Insufficient permissions");
        }
    }
}
