using Discord;

namespace Emzi0767.Ada.Commands.Permissions
{
    public class AdaDebugChecker : IAdaPermissionChecker
    {
        public string Id { get { return "CoreDebugChecker"; } }

        public bool CanRun(AdaCommand command, IGuildUser user, IMessage message, IMessageChannel channel, IGuild guild, out string error)
        {
            error = "This is a debug command. It can be only ran by Emzi0767.";
            if (user.Id == 181875147148361728u && user.Username == "Emzi0767" && user.Discriminator == "1837")
                return true;
            return false;
        }
    }
}
