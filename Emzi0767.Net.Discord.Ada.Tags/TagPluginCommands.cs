using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Core;

namespace Emzi0767.Net.Discord.Ada.Tags
{
    [CommandHandler]
    public static class TagPluginCommands
    {
        [Command("newtag", "Creates a new tag. This command can only be used by guild administrators.", Aliases = "mktag;definetag", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public static async Task DefineTag(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;

            await msg.Delete();

            var nam = ea.Args[0].Substring(0, ea.Args[0].IndexOf(' '));
            var tag = ea.Args[0].Substring(ea.Args[0].IndexOf(' ') + 1);
            if (string.IsNullOrWhiteSpace(nam) || string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag needs to have a name and contents.");
            var wrk = TagPlugin.AddTag(chn.Id, nam, tag);
            if (!wrk)
                throw new ArgumentException("Failed to create a tag, a tag with given name already exists for this channel.");
            
            await chn.SendMessage(string.Concat("**ADA**: Tag **", nam, "** was successfully created."));
        }

        [Command("edittag", "Edits an existing tag. This command can only be used by guild administrators.", Aliases = "modtag;modifytag", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public static async Task EditTag(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;

            await msg.Delete();

            var nam = ea.Args[0].Substring(0, ea.Args[0].IndexOf(' '));
            var tag = ea.Args[0].Substring(ea.Args[0].IndexOf(' ') + 1);
            if (string.IsNullOrWhiteSpace(nam) || string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("Tag needs to have a name and contents.");
            var wrk = TagPlugin.EditTag(chn.Id, nam, tag);
            if (!wrk)
                throw new ArgumentException("Failed to edit a tag, a tag with given name does not exist for this channel.");

            await chn.SendMessage(string.Concat("**ADA**: Tag **", nam, "** was successfully edited."));
        }

        [Command("removetag", "Removes an existing tag. This command can only be used by guild administrators.", Aliases = "rmtag;deletetag;deltag", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public static async Task RemoveTag(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;

            await msg.Delete();

            var nam = ea.Args[0];
            if (string.IsNullOrWhiteSpace(nam))
                throw new ArgumentException("Need to specify a tag to remove.");
            var wrk = TagPlugin.DeleteTag(chn.Id, nam);
            if (!wrk)
                throw new ArgumentException("Invalid tag specified.");

            await chn.SendMessage(string.Concat("**ADA**: Tag **", nam, "** was successfully removed."));
        }

        [Command("tag", "Displays contents of a specified tag.", CheckPermissions = false)]
        public static async Task ShowTag(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;

            await msg.Delete();

            var nam = ea.Args[0];
            if (string.IsNullOrWhiteSpace(nam))
                throw new ArgumentException("Need to specify a tag to display.");
            var tag = TagPlugin.GetTag(chn.Id, nam);
            if (tag == null)
                throw new ArgumentException("Invalid tag specified.");

            await chn.SendMessage(tag.Contents);
        }

        [Command("dumptag", "Dumps contents of a specified tag.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public static async Task DumpTag(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;

            await msg.Delete();

            var nam = ea.Args[0];
            if (string.IsNullOrWhiteSpace(nam))
                throw new ArgumentException("Need to specify a tag to display.");
            var tag = TagPlugin.GetTag(chn.Id, nam);
            if (tag == null)
                throw new ArgumentException("Invalid tag specified.");

            await chn.SendMessage(string.Concat("```\n", tag.Contents.Replace("```", "` ` `"), "\n```"));
        }

        [Command("tags", "Lists tags defined for this channel.", CheckPermissions = false)]
        public static async Task ShowTags(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;

            await msg.Delete();

            var tags = TagPlugin.GetTags(chn.Id);
            if (tags.Count() > 0)
            {
                await chn.SendMessage(string.Concat("**ADA**: Tags defined for ", chn.Mention, ": ", string.Join(", ", tags.Select(xtag => string.Concat("**", xtag.Id, "**")))));
            }
            else
                throw new InvalidOperationException("There are no tags defined for this channel.");
        }
    }
}
