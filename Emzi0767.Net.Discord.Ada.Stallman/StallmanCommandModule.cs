using System.Threading.Tasks;
using Discord;
using Emzi0767.Net.Discord.AdaBot;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Commands;
using Emzi0767.Net.Discord.AdaBot.Commands.Permissions;

namespace Emzi0767.Net.Discord.Ada.Stallman
{
    public class StallmanCommandModule : IAdaCommandModule
    {
        public string Name { get { return "GNU/Stallman Commands"; } }

        [AdaCommand("stallmanenable", "Enables the GNU/Stallman plugin for this guild. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageGuild)]
        public async Task EnableStallman(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;

            await ctx.Message.DeleteAsync();
            StallmanPlugin.Instance.Enable(gld.Id);

            var embed = this.PrepareEmbed("Success", "GNU/Stallman plugin was enabled for this guild.", EmbedType.Success);
            await ctx.Channel.SendMessageAsync("", false, embed);
        }

        [AdaCommand("stallmandisable", "Disables the GNU/Stallman plugin for this guild. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageGuild)]
        public async Task DisableStallman(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;

            await ctx.Message.DeleteAsync();
            StallmanPlugin.Instance.Disable(gld.Id);

            var embed = this.PrepareEmbed("Success", "GNU/Stallman plugin was disabled for this guild.", EmbedType.Success);
            await ctx.Channel.SendMessageAsync("", false, embed);
        }

        private EmbedBuilder PrepareEmbed(string title, string desc, EmbedType type)
        {
            var embed = new EmbedBuilder();
            embed.Title = title;
            embed.Description = desc;
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.IconUrl = AdaBotCore.AdaClient.CurrentUser.AvatarUrl;
            embed.Author.Name = "ADA, a bot by Emzi0767";
            var ecolor = new Color(255, 255, 255);
            switch (type)
            {
                case EmbedType.Info:
                    ecolor = new Color(0, 127, 255);
                    break;

                case EmbedType.Success:
                    ecolor = new Color(127, 255, 0);
                    break;

                case EmbedType.Warning:
                    ecolor = new Color(255, 255, 0);
                    break;

                case EmbedType.Error:
                    ecolor = new Color(255, 127, 0);
                    break;
            }
            embed.Color = ecolor;
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
