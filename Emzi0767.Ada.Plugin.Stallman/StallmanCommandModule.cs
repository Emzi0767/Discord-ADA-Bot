using System.Threading.Tasks;
using Discord;
using Emzi0767.Ada.Attributes;
using Emzi0767.Ada.Commands;
using Emzi0767.Ada.Commands.Permissions;

namespace Emzi0767.Ada.Plugin.Stallman
{
    public class StallmanCommandModule : IAdaCommandModule
    {
        public string Name { get { return "GNU/Stallman Commands"; } }

        [AdaCommand("stallmanenable", "Enables the GNU/Stallman plugin for this guild.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageGuild)]
        public async Task EnableStallman(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            
            StallmanPlugin.Instance.Enable(gld.Id);

            var embed = this.PrepareEmbed("Success", "GNU/Stallman plugin was enabled for this guild.", EmbedType.Success);
            await ctx.Channel.SendMessageAsync("", false, embed);
        }

        [AdaCommand("stallmandisable", "Disables the GNU/Stallman plugin for this guild.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageGuild)]
        public async Task DisableStallman(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;

            StallmanPlugin.Instance.Disable(gld.Id);

            var embed = this.PrepareEmbed("Success", "GNU/Stallman plugin was disabled for this guild.", EmbedType.Success);
            await ctx.Channel.SendMessageAsync("", false, embed);
        }

        private EmbedBuilder PrepareEmbed(EmbedType type)
        {
            var embed = new EmbedBuilder();
            switch (type)
            {
                case EmbedType.Info:
                    embed.Color = new Color(0, 127, 255);
                    break;

                case EmbedType.Success:
                    embed.Color = new Color(127, 255, 0);
                    break;

                case EmbedType.Warning:
                    embed.Color = new Color(255, 255, 0);
                    break;

                case EmbedType.Error:
                    embed.Color = new Color(255, 127, 0);
                    break;

                default:
                    embed.Color = new Color(255, 255, 255);
                    break;
            }
            embed.ThumbnailUrl = AdaBotCore.AdaClient.CurrentUser.AvatarUrl;
            return embed;
        }

        private EmbedBuilder PrepareEmbed(string title, string desc, EmbedType type)
        {
            var embed = this.PrepareEmbed(type);
            embed.Title = title;
            embed.Description = desc;
            return embed;
        }

        private enum EmbedType : uint
        {
            Unknown,
            Success,
            Error,
            Warning,
            Info
        }
    }
}
