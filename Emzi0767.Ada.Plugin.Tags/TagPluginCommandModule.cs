using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Emzi0767.Ada.Attributes;
using Emzi0767.Ada.Commands;
using Emzi0767.Ada.Commands.Permissions;

namespace Emzi0767.Ada.Plugin.Tags
{
    public class TagPluginCommandModule : IAdaCommandModule
    {
        public string Name { get { return "Tag Plugin Controls"; } }

        [AdaCommand("newtag", "Creates a new tag.", Aliases = "mktag;definetag", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public async Task DefineTag(AdaCommandContext ctx,
            [AdaArgumentParameter("Name of tag to create.", true)] string name, 
            [AdaArgumentParameter("Contents of tag to create.", true)] params string[] contents)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var nam = name;
            var tag = string.Join(" ", contents);
            if (string.IsNullOrWhiteSpace(nam) || string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag needs to have a name and contents.");
            var wrk = TagPlugin.Instance.AddTag(chn.Id, nam, tag);
            if (!wrk)
                throw new ArgumentException("Failed to create a tag, a tag with given name already exists for this channel.");

            var embed = this.PrepareEmbed("Success", string.Concat("Created tag **", nam, "** for this channel."), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("edittag", "Edits an existing tag.", Aliases = "modtag;modifytag", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public async Task EditTag(AdaCommandContext ctx,
            [AdaArgumentParameter("Name of tag to edit.", true)] string name,
            [AdaArgumentParameter("New contents of the tag.", true)] params string[] contents)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var nam = name;
            var tag = string.Join(" ", contents);
            if (string.IsNullOrWhiteSpace(nam) || string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag needs to have a name and contents.");
            var wrk = TagPlugin.Instance.EditTag(chn.Id, nam, tag);
            if (!wrk)
                throw new ArgumentException("Failed to edit the tag, a tag with given name does not exist for this channel.");

            var embed = this.PrepareEmbed("Success", string.Concat("Edited tag **", nam, "** for this channel."), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("removetag", "Removes an existing tag.", Aliases = "rmtag;deletetag;deltag", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public async Task RemoveTag(AdaCommandContext ctx,
            [AdaArgumentParameter("Name of tag to delete.", true)] params string[] name)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var nam = string.Join(" ", name);
            if (string.IsNullOrWhiteSpace(nam))
                throw new ArgumentException("Need to specify a tag to remove.");
            var wrk = TagPlugin.Instance.DeleteTag(chn.Id, nam);
            if (!wrk)
                throw new ArgumentException("Invalid tag specified.");

            var embed = this.PrepareEmbed("Success", string.Concat("Removed tag **", nam, "** for this channel."), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("tag", "Displays contents of a specified tag.", CheckPermissions = false)]
        public async Task ShowTag(AdaCommandContext ctx,
            [AdaArgumentParameter("Name of tag to display.", true)] params string[] name)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var nam = string.Join(" ", name);
            if (string.IsNullOrWhiteSpace(nam))
                throw new ArgumentException("Need to specify a tag to display.");
            var tag = TagPlugin.Instance.GetTag(chn.Id, nam);
            if (tag == null)
                throw new ArgumentException("Invalid tag specified.");

            await chn.SendMessageAsync(tag.Contents);
        }

        [AdaCommand("dumptag", "Displays raw contents of a specified tag.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public async Task DumpTag(AdaCommandContext ctx,
            [AdaArgumentParameter("Name of tag to dump.", true)] params string[] name)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var nam = string.Join(" ", name);
            if (string.IsNullOrWhiteSpace(nam))
                throw new ArgumentException("Need to specify a tag to display.");
            var tag = TagPlugin.Instance.GetTag(chn.Id, nam);
            if (tag == null)
                throw new ArgumentException("Invalid tag specified.");

            await chn.SendMessageAsync(string.Concat("```\n", tag.Contents.Replace("```", "` ` `"), "\n```"));
        }

        [AdaCommand("tags", "Lists tags defined for this channel.", CheckPermissions = false)]
        public async Task ShowTags(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel as SocketTextChannel;
            var msg = ctx.Message;

            var tags = TagPlugin.Instance.GetTags(chn.Id);
            if (tags.Count() > 0)
            {
                var embed = this.PrepareEmbed("Tag Plugin", string.Concat("List of tags defined for ", chn.Mention, "."), EmbedType.Info);
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Defined tags";
                    x.Value = string.Join(", ", tags.Select(xtag => string.Concat("**", xtag.Id, "**")));
                });
                await chn.SendMessageAsync("", false, embed);
            }
            else
                throw new InvalidOperationException("There are no tags defined for this channel.");
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
