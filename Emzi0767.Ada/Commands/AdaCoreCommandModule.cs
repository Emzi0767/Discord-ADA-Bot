using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Emzi0767.Ada.Commands.Permissions;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Core;
using Emzi0767.Ada.Extensions;
using Emzi0767.Ada.Plugins;
using Emzi0767.Ada.Sql;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.PlatformAbstractions;

namespace Emzi0767.Ada.Commands
{
    [Name("core")]
    internal class AdaCoreCommandModule : AdaModuleBase
    {
        public AdaCoreCommandModule(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
            : base(cfg, plm, sql, util)
        { }

        #region Role Management
        [Group("role")]
        [Alias("roles")]
        [Summary("Role management commands")]
        public class RoleCommands : AdaModuleBase
        {
            public RoleCommands(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
                : base(cfg, plm, sql, util)
            { }

            [Command("create")]
            [Summary("Creates a new role")]
            [Alias("make", "new", "mk")]
            [AdaPermissionRequired(AdaPermission.ManageRoles)]
            public async Task Create([Remainder, Summary("Name of the role to create")] string name)
            {
                var gld = this.Context.Guild as SocketGuild;

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name cannot be empty.");

                var grl = await gld.CreateRoleAsync(name);

                var embed = this.Utilities.BuildEmbed(this.Context, "Role creation successful", string.Concat("Role ", grl.Mention, " created successfully."), AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }

            [Command("delete")]
            [Summary("Deletes a role")]
            [Alias("remove", "del", "rm")]
            [AdaPermissionRequired(AdaPermission.ManageRoles)]
            public async Task Delete([Summary("Role to delete")] SocketRole role)
            {
                var grl = role;
                if (grl == null)
                    throw new ArgumentException("You must specify a role you want to delete.");

                await grl.DeleteAsync();

                var embed = this.Utilities.BuildEmbed(this.Context, "Role deletion successful", string.Concat("Role `", grl.Name, "` deleted successfully."), AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }

            [Command("edit")]
            [Summary("Edits a role")]
            [Alias("modify", "mod")]
            [AdaPermissionRequired(AdaPermission.ManageRoles)]
            public async Task Edit([Summary("Role to edit")] SocketRole role,
                [Summary("Properties to modify; format is property=value")] params string[] properties)
            {
                var grl = role;
                if (grl == null)
                    throw new ArgumentException("You must specify a role you want to modify.");

                var par = properties
                    .Select(xrs => xrs.Split('='))
                    .ToDictionary(xrs => xrs[0], xrs => xrs[1]);

                var gpr = par.ContainsKey("permissions") ? ulong.Parse(par["permissions"]) : 0;
                var gcl = par.ContainsKey("color") ? Convert.ToUInt32(par["color"], 16) : 0;
                var ghs = par.ContainsKey("hoist") ? par["hoist"] == "true" : false;
                var gps = par.ContainsKey("position") ? int.Parse(par["position"]) : 0;
                var gmt = par.ContainsKey("mention") ? par["mention"] == "true" : false;

                await grl.ModifyAsync(x =>
                {
                    if (par.ContainsKey("color"))
                        x.Color = new Color(gcl);
                    if (par.ContainsKey("hoist"))
                        x.Hoist = ghs;
                    if (par.ContainsKey("permissions"))
                        x.Permissions = new GuildPermissions(gpr);
                    if (par.ContainsKey("position"))
                        x.Position = gps;
                    if (par.ContainsKey("mention"))
                        x.Mentionable = gmt;
                });

                var embed = this.Utilities.BuildEmbed(this.Context, "Role modification successful", string.Concat("Role ", grl.Mention, " edited successfully."), AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }

            [Command("information")]
            [Summary("Displays information about a role")]
            [Alias("info", "inf")]
            [AdaPermissionRequired(AdaPermission.ManageRoles)]
            public async Task Information([Summary("Role to display information of")] SocketRole role)
            {
                var grl = role;
                if (grl == null)
                    throw new ArgumentException("You must specify a role of which information you want to display.");

                var embed = this.Utilities.BuildEmbed(this.Context, "Role information", string.Concat("Information about ", grl.Mention, "."), AdaUtilities.EmbedColour.Info);

                embed = this.Utilities.CreateEmbedField(embed, "Name", grl.Name);
                embed = this.Utilities.CreateEmbedField(embed, "ID", grl.Id.ToString());
                embed = this.Utilities.CreateEmbedField(embed, "Colour", grl.Color.RawValue.ToString("X6"));
                embed = this.Utilities.CreateEmbedField(embed, "Hoisted", grl.IsHoisted ? "Yes" : "No");
                embed = this.Utilities.CreateEmbedField(embed, "Mentionable", grl.IsMentionable ? "Yes" : "No");
                embed = this.Utilities.CreateEmbedField(embed, "Permissions", this.Utilities.GetPermissionString((AdaPermission)grl.Permissions.RawValue));

                await this.ReplyAsync("", false, embed);
            }

            [Command("list")]
            [Summary("Lists all roles defined in this guild")]
            [AdaPermissionRequired(AdaPermission.ManageRoles)]
            public async Task List()
            {
                var gld = this.Context.Guild as SocketGuild;

                var grl = gld.Roles;
                if (grl == null)
                    return;

                var embed = this.Utilities.BuildEmbed(this.Context, "Role list", string.Concat("Listing of all ", grl.Count.ToString("#,##0"), " role", grl.Count > 1 ? "s" : "", " in this Guild."), AdaUtilities.EmbedColour.Info);
                this.Utilities.CreateEmbedField(embed, "Defined roles", string.Join(", ", grl.Select(xr => xr.Mention)), false);

                await this.ReplyAsync("", false, embed);
            }

            [Command("addmembers")]
            [Summary("Adds specified members to a role")]
            [Alias("addmember", "addusers", "adduser")]
            [AdaPermissionRequired(AdaPermission.ManageRoles)]
            public async Task AddMembers([Summary("Role to add members to")] SocketRole role,
                [Summary("Members to add to specified role")] params SocketGuildUser[] users)
            {
                var grl = role;
                if (grl == null)
                    throw new ArgumentException("You must specify a role to add members to.");

                var usrs = users;
                if (usrs.Length == 0)
                    throw new ArgumentException("You must specify members to add to the role.");

                foreach (var usm in usrs)
                    await usm.AddRolesAsync(grl);

                var embed = this.Utilities.BuildEmbed(this.Context, "Members added successfully", string.Concat("Member", usrs.Count() > 1 ? "s were" : " was", " added to ", grl.Mention, " successfully."), AdaUtilities.EmbedColour.Success);
                this.Utilities.CreateEmbedField(embed, "Added members", string.Join(", ", users.Select(xus => xus.Mention)), false);

                await this.ReplyAsync("", false, embed);
            }

            [Command("removemembers")]
            [Summary("Removes specified members from a role")]
            [Alias("removemember", "removeusers", "removeuser")]
            [AdaPermissionRequired(AdaPermission.ManageRoles)]
            public async Task RemoveMembers([Summary("Role to remove members from")] SocketRole role,
            [Summary("Members to remove from specified role")] params SocketGuildUser[] users)
            {
                var grl = role;
                if (grl == null)
                    throw new ArgumentException("You must specify a role to remove users from.");

                var usrs = users;
                if (usrs.Length == 0)
                    throw new ArgumentException("You must specify users you want to remove from the role.");

                foreach (var usm in usrs)
                    await usm.RemoveRolesAsync(grl);

                var embed = this.Utilities.BuildEmbed(this.Context, "Members removed successfully", string.Concat("User", usrs.Count() > 1 ? "s were" : " was", " removed from ", grl.Mention, "."), AdaUtilities.EmbedColour.Success);
                this.Utilities.CreateEmbedField(embed, "Removed members", string.Join(", ", users.Select(xus => xus.Mention)), false);

                await this.ReplyAsync("", false, embed);
            }
        }
        #endregion

        #region Member Management
        [Group("member")]
        [Alias("members", "user", "users")]
        [Summary("Member management commands")]
        internal class MemberCommands : AdaModuleBase
        {
            public MemberCommands(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
                : base(cfg, plm, sql, util)
            { }

            [Command("report")]
            [Summary("Reports a member to moderators")]
            public async Task Report([Summary("Member to report")] SocketGuildUser user,
                [Remainder, Summary("Reason for report")] string reason)
            {
                var gld = this.Context.Guild as SocketGuild;
                var usr = this.Context.User as SocketGuildUser;
                
                var rsn = reason;
                if (string.IsNullOrWhiteSpace(rsn))
                    throw new ArgumentException("You must specify a reason");

                if (!this.Utilities.CheckModLogConfiguration(gld))
                    throw new InvalidOperationException("This guild is not properly configured");

                var embed = this.Utilities.BuildEmbed(this.Context, "User reported successfully", string.Concat(user.Mention, " was reported to the moderators."), AdaUtilities.EmbedColour.Success);

                await this.Utilities.ReportUserAsync(user, usr, reason);
                await this.ReplyAsync("", false, embed);
            }

            [Command("mute")]
            [Summary("Mutes members")]
            [AdaPermissionRequired(AdaPermission.KickMembers)]
            public async Task Mute([Summary("Duration of the mute")] TimeSpan? duration, 
                [Summary("Members to mute")] params SocketGuildUser[] members)
            {
                var gld = this.Context.Guild as SocketGuild;

                if (members.Length < 1)
                    throw new ArgumentException("You must specify members");

                var gconf = this.ConfigurationManager.GetConfiguration(gld);
                var mrl = gconf.MuteRole;
                if (mrl == null)
                    throw new InvalidOperationException("This guild is not properly configured");

                foreach (var usm in members)
                {
                    await usm.AddRolesAsync(mrl);
                    await this.Utilities.CreateModLogAsync(usm, ModLogEntryType.UserMute, duration, this.Context.User as SocketGuildUser);
                }

                var embed = this.Utilities.BuildEmbed(this.Context, "Users muted", "Users were successfully muted.", AdaUtilities.EmbedColour.Success);
                this.Utilities.CreateEmbedField(embed, "Muted users", string.Join(", ", members.Select(xm => xm.Mention)), false);
                await this.ReplyAsync("", false, embed);
            }

            [Command("unmute")]
            [Summary("Unmutes members")]
            [AdaPermissionRequired(AdaPermission.KickMembers)]
            public async Task Unmute([Summary("Members to unmyte")] params SocketGuildUser[] members)
            {
                var gld = this.Context.Guild as SocketGuild;

                if (members.Length < 1)
                    throw new ArgumentException("You must specify members");

                var gconf = this.ConfigurationManager.GetConfiguration(gld);
                var mrl = gconf.MuteRole;
                if (mrl == null)
                    throw new InvalidOperationException("This guild is not properly configured");

                foreach (var usm in members)
                {
                    await usm.RemoveRolesAsync(mrl);
                    await this.Utilities.CreateModLogAsync(usm, ModLogEntryType.UserUnmute, null, this.Context.User as SocketGuildUser);
                }

                var embed = this.Utilities.BuildEmbed(this.Context, "Users unmuted", "Users were successfully unmuted.", AdaUtilities.EmbedColour.Success);
                this.Utilities.CreateEmbedField(embed, "Unmuted users", string.Join(", ", members.Select(xm => xm.Mention)), false);
                await this.ReplyAsync("", false, embed);
            }

            [Command("kick")]
            [Summary("Kicks members")]
            [AdaPermissionRequired(AdaPermission.KickMembers)]
            public async Task Kick([Summary("Members to kick")] params SocketGuildUser[] members)
            {
                if (members.Length < 1)
                    throw new ArgumentException("You must specify members");

                foreach (var usm in members)
                {
                    await usm.KickAsync();
                    await this.Utilities.CreateModLogAsync(usm, ModLogEntryType.UserKick, null, this.Context.User as SocketGuildUser);
                }

                var embed = this.Utilities.BuildEmbed(this.Context, "Users kicked", "Users were successfully kicked.", AdaUtilities.EmbedColour.Success);
                this.Utilities.CreateEmbedField(embed, "Kicked users", string.Join(", ", members.Select(xm => xm.Mention)), false);
                await this.ReplyAsync("", false, embed);
            }

            [Command("softban")]
            [Summary("Softbans members")]
            [AdaPermissionRequired(AdaPermission.KickMembers)]
            public async Task Softban([Summary("Members to ban")] params SocketGuildUser[] members)
            {
                var gld = this.Context.Guild as SocketGuild;

                if (members.Length < 1)
                    throw new ArgumentException("You must specify members");

                foreach (var usr in members)
                {
                    await gld.AddBanAsync(usr, 7);
                    await gld.RemoveBanAsync(usr);
                }

                var embed = this.Utilities.BuildEmbed(this.Context, "Members softbanned", string.Concat("The following members were softbanned: ", string.Join(", ", members.Select(xu => xu.Mention))), AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }

            [Command("ban")]
            [Summary("Bans members")]
            [AdaPermissionRequired(AdaPermission.BanMembers)]
            public async Task Ban([Summary("Duration of the ban")] TimeSpan? duration, 
                [Summary("Members to ban")] params SocketGuildUser[] members)
            {
                var gld = this.Context.Guild as SocketGuild;

                if (members.Length < 1)
                    throw new ArgumentException("You must specify members");

                foreach (var usm in members)
                {
                    await gld.AddBanAsync(usm);
                    await this.Utilities.CreateModLogAsync(usm, ModLogEntryType.UserBan, duration, this.Context.User as SocketGuildUser);
                }

                var embed = this.Utilities.BuildEmbed(this.Context, "Users banned", "Users were successfully banned.", AdaUtilities.EmbedColour.Success);
                this.Utilities.CreateEmbedField(embed, "Banned users", string.Join(", ", members.Select(xm => xm.Mention)), false);
                await this.ReplyAsync("", false, embed);
            }

            [Command("unban")]
            [Summary("Unbans members")]
            [AdaPermissionRequired(AdaPermission.BanMembers)]
            public async Task Unban([Summary("Ids of members to unban")] params ulong[] members)
            {
                var gld = this.Context.Guild as SocketGuild;
                
                if (members.Length < 1)
                    throw new ArgumentException("You must specify members");

                var bans = await gld.GetBansAsync();

                foreach (var ban in bans)
                {
                    if (!members.Contains(ban.User.Id))
                        continue;

                    await gld.RemoveBanAsync(ban.User);
                    await this.Utilities.CreateModLogAsync(gld, ban.User, ModLogEntryType.UserUnban, null, this.Context.User as SocketGuildUser);
                }

                var embed = this.Utilities.BuildEmbed(this.Context, "Users unbanned", "Users were successfully unbanned.", AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }

            [Command("prune")]
            [Summary("Prunes inactive members")]
            [AdaPermissionRequired(AdaPermission.KickMembers)]
            public async Task Prune([Summary("Number of inactivity days")] int days = 30)
            {
                var gld = this.Context.Guild as SocketGuild;

                var usp = await gld.PruneUsersAsync(days);
                var embed = this.Utilities.BuildEmbed(this.Context, "Pruned successfully", string.Concat(usp.ToString("#,##0"), " user", usp > 1 ? "s were" : " was", " pruned."), AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }

            [Command("reason")]
            [Summary("Sets a reason and moderator for specified moderator action")]
            [AdaPermissionRequired(AdaPermission.ManageGuild)]
            public async Task Reason([Summary("Case to modify")] long caseid, 
                [Remainder, Summary("Reason for taking the action")] string reason)
            {
                var gld = this.Context.Guild as SocketGuild;
                var usr = this.Context.User as SocketGuildUser;

                var mle = await this.Utilities.GetModLog(gld, caseid);
                await this.Utilities.ModifyModLogAsync(mle, mle.ActionType, usr, reason);
            }

            [Command("information")]
            [Summary("Displays information about a member")]
            [Alias("info")]
            public async Task Information([Summary("Member to display information of")] SocketGuildUser member)
            {
                var gld = this.Context.Guild as SocketGuild;

                var embed = this.Utilities.BuildEmbed(this.Context, "Member information", string.Concat("Information about ", member.Mention, "."), AdaUtilities.EmbedColour.Info);
                if (!string.IsNullOrWhiteSpace(member.GetAvatarUrl(AvatarFormat.WebP)))
                    embed.ThumbnailUrl = member.GetAvatarUrl(AvatarFormat.WebP);

                this.Utilities.CreateEmbedField(embed, "Username", string.Concat("**", member.Username, "**#", member.DiscriminatorValue));
                this.Utilities.CreateEmbedField(embed, "Id", member.Id.ToString());
                this.Utilities.CreateEmbedField(embed, "Nickname", member.Nickname ?? member.Username);
                this.Utilities.CreateEmbedField(embed, "Status", member.Status.ToString());
                this.Utilities.CreateEmbedField(embed, "Game", member.Game != null ? member.Game.Value.Name : "<not playing anything>");
                this.Utilities.CreateEmbedField(embed, "Roles", string.Join(", ", member.Roles.Select(xr => xr.Mention)));

                await this.ReplyAsync("", false, embed);
            }
        }
        #endregion

        #region Guild Management
        [Group("guild")]
        [Summary("Guild management commands")]
        public class GuildCommands : AdaModuleBase
        {
            public GuildCommands(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
                : base(cfg, plm, sql, util)
            { }

            [Command("information")]
            [Summary("Displays information about this guild")]
            [Alias("info", "inf")]
            [AdaPermissionRequired(AdaPermission.ManageGuild)]
            public async Task Information()
            {
                var gld = this.Context.Guild as SocketGuild;
                var gconf = this.ConfigurationManager.GetConfiguration(gld);

                var embed = this.Utilities.BuildEmbed(this.Context, "Guild information", string.Concat("Information about `", gld.Name, "`."), AdaUtilities.EmbedColour.Info);
                if (!string.IsNullOrWhiteSpace(gld.IconUrl))
                    embed.ThumbnailUrl = gld.IconUrl;

                this.Utilities.CreateEmbedField(embed, "Name", gld.Name);
                this.Utilities.CreateEmbedField(embed, "ID", gld.Id.ToString());
                this.Utilities.CreateEmbedField(embed, "Voice region", gld.VoiceRegionId);
                this.Utilities.CreateEmbedField(embed, "Owner", gld.Owner.Mention);
                this.Utilities.CreateEmbedField(embed, "Default channel", gld.DefaultChannel.Mention);
                this.Utilities.CreateEmbedField(embed, "Moderation log", gconf.ModerationLog != null ? gconf.ModerationLog.Mention : "Not configured");
                this.Utilities.CreateEmbedField(embed, "Mute role", gconf.MuteRole != null ? gconf.MuteRole.Mention : "Not configured");
                this.Utilities.CreateEmbedField(embed, "Command prefix", string.Concat("`", this.Utilities.AdaClient.GetPrefix(gld), "`"));

                await this.ReplyAsync("", false, embed);
            }

            [Command("rename")]
            [Summary("Renames the current guild")]
            [AdaPermissionRequired(AdaPermission.ManageGuild)]
            public async Task Rename([Remainder] string new_name)
            {
                var gld = this.Context.Guild as SocketGuild;

                await gld.ModifyAsync(x => x.Name = new_name);
                var embed = this.Utilities.BuildEmbed(this.Context, "Guild rename successful", string.Concat("Guild renamed to `", new_name, "`."), AdaUtilities.EmbedColour.Success);

                await this.ReplyAsync("", false, embed);
            }

            [Group("configuration")]
            [Summary("Guild configuration commands")]
            [Alias("config", "conf")]
            public class GuildConfigurationCommands : AdaModuleBase
            {
                public GuildConfigurationCommands(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
                    : base(cfg, plm, sql, util)
                { }

                [Command("moderationlog")]
                [Summary("Sets the channel to be used for logging moderator actions")]
                [Alias("modlog")]
                [AdaPermissionRequired(AdaPermission.ManageGuild)]
                public async Task ModerationLog([Summary("Channel to use")] SocketTextChannel channel)
                {
                    var gld = this.Context.Guild as SocketGuild;

                    var cfg = this.ConfigurationManager.GetConfiguration(gld);
                    cfg.ModerationLog = channel;

                    var embed = this.Utilities.BuildEmbed(this.Context, "Setting saved", string.Concat("Moderation log set to ", channel.Mention, "."), AdaUtilities.EmbedColour.Success);
                    await this.ReplyAsync("", false, embed);
                }

                [Command("muterole")]
                [Summary("Sets the role to be used for muting members")]
                [AdaPermissionRequired(AdaPermission.ManageGuild)]
                [Priority(10)]
                public async Task MuteRole([Remainder, Summary("Role to use")] string role)
                {
                    var gld = this.Context.Guild as SocketGuild;

                    var grl = gld.Roles.FirstOrDefault(xr => xr.Name == role) as SocketRole;
                    if (grl == null)
                    {
                        var embed = this.Utilities.BuildEmbed(this.Context, "Setting error", string.Concat("No role with specified name was found."), AdaUtilities.EmbedColour.Error);
                        await this.ReplyAsync("", false, embed);
                        return;
                    }

                    await this.MuteRole(grl);
                }

                [Command("muterole")]
                [Summary("Sets the role to be used for muting members")]
                [AdaPermissionRequired(AdaPermission.ManageGuild)]
                [Priority(0)]
                public async Task MuteRole([Summary("Role to use")] SocketRole role)
                {
                    var gld = this.Context.Guild as SocketGuild;

                    var cfg = this.ConfigurationManager.GetConfiguration(gld);
                    cfg.MuteRole = role;

                    var embed = this.Utilities.BuildEmbed(this.Context, "Setting saved", string.Concat("Mute role set to ", role.Mention, "."), AdaUtilities.EmbedColour.Success);
                    await this.ReplyAsync("", false, embed);
                }

                [Command("commandprefix")]
                [Summary("Sets the bot's command prefix for this guild")]
                [Alias("prefix")]
                [AdaPermissionRequired(AdaPermission.ManageGuild)]
                public async Task CommandPrefix([Remainder, Summary("The prefix to use")] string prefix = "")
                {
                    var gld = this.Context.Guild as SocketGuild;

                    var cfg = this.ConfigurationManager.GetConfiguration(gld);
                    cfg.CommandPrefix = prefix;

                    var embed = this.Utilities.BuildEmbed(this.Context, "Setting saved", string.Concat("Command prefix set to `", this.Utilities.AdaClient.GetPrefix(gld), "`."), AdaUtilities.EmbedColour.Success);
                    await this.ReplyAsync("", false, embed);
                }
            }
        }
        #endregion

        #region Channel Management
        [Group("channels")]
        [Summary("Channel management commands")]
        [Alias("channel")]
        public class ChannelCommands : AdaModuleBase
        {
            public ChannelCommands(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
                : base(cfg, plm, sql, util)
            { }

            [Command("purge")]
            [Summary("Purges a channel")]
            [AdaPermissionRequired(AdaPermission.ManageChannels)]
            public async Task Purge([Summary("Channel to purge")] SocketTextChannel channel, [Summary("Number of messages to remove")] int count = 100)
            {
                var chp = channel;

                var dcount = 0;
                for (var i = 0; i < count; i += 100)
                {
                    var msgs = await chp.GetMessagesAsync(Math.Min(100, count - i)).Flatten();
                    dcount += msgs.Count();
                    await chp.DeleteMessagesAsync(msgs);
                }

                var embed = this.Utilities.BuildEmbed(this.Context, "Purge successful", string.Concat("Deleted ", dcount.ToString("#,##0"), " message", dcount > 1 ? "s" : "", " from channel ", chp.Mention, "."), AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }
        }
        #endregion

        #region Bot Management
        [Group("bot")]
        [Summary("Bot managements commands")]
        [Alias("ada")]
        public class BotCommands : AdaModuleBase
        {
            public BotCommands(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
                : base(cfg, plm, sql, util)
            { }

            [Command("nickname")]
            [Summary("Changes bot's nickname")]
            [Alias("nick", "name")]
            [AdaPermissionRequired(AdaPermission.ManageNicknames)]
            public async Task Nick([Remainder, Summary("New nickanme")] string nickname = "")
            {
                var gld = this.Context.Guild as SocketGuild;

                var ada = gld.CurrentUser;
                await ada.ModifyAsync(x => x.Nickname = nickname);

                var embed = this.Utilities.BuildEmbed(this.Context, "Nickname changed", string.Concat("Nickname set to `", nickname == "" ? ada.Username : nickname, "`."), AdaUtilities.EmbedColour.Success);
                await this.ReplyAsync("", false, embed);
            }

            [Command("about")]
            [Summary("Displays information about the bot")]
            [Alias("information", "info")]
            public async Task About()
            {
                var embed = this.Utilities.BuildEmbed(this.Context, "About ADA", "ADA stands for Automated/Advanced Discord Administrator. ADA is a Discord bot written in C# by Emzi0767. It's designed to simplify and automate certain administrative tasks. For more information, visit [ADA's GitHub page](https://emzi0767.github.io/discord/ada/).", AdaUtilities.EmbedColour.Info);
                this.Utilities.CreateEmbedField(embed, "Source Code", "ADA is fully Open Source. The code is available on [Emzi's GitHub](https://github.com/Emzi0767/Discord-ADA-Bot).", false);
                this.Utilities.CreateEmbedField(embed, "Invite", string.Concat("Want to invite ADA to your server? Simply follow [this invite link](https://discordapp.com/oauth2/authorize?client_id=", this.Context.Client.CurrentUser.Id.ToString(), "&scope=bot&permissions=2146958463)."), false);

                await this.ReplyAsync("", false, embed);
            }

            [Command("invitelink")]
            [Summary("Displays the bot's invite link, which allows you to invite ADA to your guild")]
            [Alias("invite")]
            public async Task InviteLink()
            {
                var embed = this.Utilities.BuildEmbed(this.Context, "ADA Invite link", string.Concat("Want to invite ADA to your server? Simply follow [this invite link](https://discordapp.com/oauth2/authorize?client_id=", this.Context.Client.CurrentUser.Id.ToString(), "&scope=bot&permissions=2146958463)."), AdaUtilities.EmbedColour.Info);

                await this.ReplyAsync("", false, embed);
            }
        }
        #endregion

        #region Miscellaneous Commands
        [Command("help")]
        [Summary("Displays ADA's command help")]
        public async Task Help([Remainder, Summary("Module to display help of")] string module = "")
        {
            var gld = this.Context.Guild as SocketGuild;

            var embed = (EmbedBuilder)null;

            if (string.IsNullOrWhiteSpace(module))
            {
                var cmdms = this.Utilities.AdaClient.DiscordCommands.Modules
                    .Select(xm => string.Concat("`", xm.Aliases.First() == "" ? xm.Name : xm.Aliases.First(), "`"));
                var ms = string.Join(", ", cmdms);

                embed = this.Utilities.BuildEmbed(this.Context, "ADA Help", string.Concat("Available modules: ", ms, ".\n\nFor more detailed help, visit [ADA's Command documentation](https://emzi0767.github.io/discord/ada/doc.html) page."), AdaUtilities.EmbedColour.Info);
            }
            else
            {
                var mod = this.Utilities.AdaClient.DiscordCommands.Modules
                    .FirstOrDefault(xm => xm.Name == module || xm.Aliases.Contains(module));

                if (mod == null)
                    throw new ArgumentException("Specified module does not exist");

                var cmn = mod.Aliases.First() == "" ? mod.Name : mod.Aliases.First();

                var cmd = mod.Commands.Select(xc => string.Concat("`", this.Utilities.GetQualifiedName(xc, gld), "` ", xc.Summary));
                var cmh = string.Join("\n", cmd);

                embed = this.Utilities.BuildEmbed(this.Context, "ADA Help", string.Concat("All commands defined in `", cmn, "`.\n\nFor more detailed help, visit [ADA's Command documentation](https://emzi0767.github.io/discord/ada/doc.html) page."), AdaUtilities.EmbedColour.Info);
                this.Utilities.CreateEmbedField(embed, "Commands", cmh, false);
            }

            await this.ReplyAsync("", false, embed);
        }
        #endregion

        #region Debug Commands
        [Group("debug")]
        [Summary("Debug commands")]
        public class DebugCommands : AdaModuleBase
        {
            public DebugCommands(AdaConfigurationManager cfg, AdaPluginManager plm, AdaSqlManager sql, AdaUtilities util)
                : base(cfg, plm, sql, util)
            { }

            [Command("say")]
            [Summary("Says something as the bot")]
            [AdaDebug]
            public async Task Say([Summary("Guild to say in")] ulong guild_id,
                [Summary("Channel to say in")] ulong channel_id,
                [Remainder, Summary("Message to say")] string message)
            {

            }

            [Command("statistics")]
            [Summary("Shows bot's statistics")]
            [Alias("stats")]
            [AdaDebug]
            public async Task Statistics()
            {

            }

            [Command("environment")]
            [Summary("Shows information about bot's environment")]
            [Alias("env")]
            [AdaDebug]
            public async Task Environment()
            {
                var ps = PlatformServices.Default;
                var rf = ps.Application.RuntimeFramework;
                var an = Assembly.GetEntryAssembly().GetName();

                var embed = this.Utilities.BuildEmbed(this.Context, "ADA Environment information", "Debug information about ADA's environment.", AdaUtilities.EmbedColour.Debug);
                this.Utilities.CreateEmbedField(embed, "Platform", RuntimeInformation.OSDescription);
                this.Utilities.CreateEmbedField(embed, "CPU and OS Architecture", string.Concat("", System.Environment.ProcessorCount, " cores"));
                this.Utilities.CreateEmbedField(embed, ".NET Core version", rf.Version.ToString());
                this.Utilities.CreateEmbedField(embed, "ADA version", an.Version.ToString());

                await this.ReplyAsync("", false, embed);
            }

            [Command("evaluate")]
            [Summary("Evaluates C# code and prints the output")]
            [Alias("eval")]
            [AdaDebug]
            public async Task Evaluate([Remainder, Summary("Code to evaluate")] string code)
            {
                var cs1 = code.IndexOf("```") + 3;
                cs1 = code.IndexOf('\n', cs1) + 1;
                var cs2 = code.IndexOf("```", cs1);
                var cs = code.Substring(cs1, cs2 - cs1);

                var embed = this.Utilities.BuildEmbed(this.Context, "Evaluation in progress", "Your code is being evaluated...", AdaUtilities.EmbedColour.Debug);
                var nmsg = await this.ReplyAsync("", false, embed);

                try
                {
                    var globals = new AdaEvaluationGlobals
                    {
                        Message = this.Context.Message as SocketUserMessage,
                        Utilities = this.Utilities
                    };

                    var sopts = ScriptOptions.Default;
                    sopts = sopts.WithImports("System", "System.Linq", "Discord", "Discord.WebSocket");
                    sopts = sopts.WithReferences(FrameworkAssemblyLoader.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                    var script = CSharpScript.Create(cs, sopts, typeof(AdaEvaluationGlobals));
                    script.Compile();
                    var result = await script.RunAsync(globals);

                    if (result != null && result.ReturnValue != null)
                    {
                        var rss = result.ReturnValue.ToString();

                        if (rss.Length < 2038)
                            embed = this.Utilities.BuildEmbed(this.Context, "Evaluation successful", string.Concat("```cs\n", rss, "\n```"), AdaUtilities.EmbedColour.Debug);
                        else
                            embed = this.Utilities.BuildEmbed(this.Context, "Evaluation successful", "The output was too long to post in an embed.", AdaUtilities.EmbedColour.Debug);
                    }
                    else
                        embed = this.Utilities.BuildEmbed(this.Context, "Evaluation successful", "No result was returned.", AdaUtilities.EmbedColour.Debug);
                }
                catch (Exception ex)
                {
                    embed = this.Utilities.BuildEmbed(this.Context, "Evaluation failed", string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), AdaUtilities.EmbedColour.Debug);
                }

                await nmsg.ModifyAsync(x => x.Embed = embed.Build());
            }

            [Command("shell")]
            [Summary("Executes a shell command and prints the output")]
            [Alias("exec")]
            [AdaDebug]
            public async Task Shell([Summary("Command to execute")] string command,
                [Remainder, Summary("Arguments")] string arguments = "")
            {
                var output = new StringBuilder();

                var psi = new ProcessStartInfo(string.Concat(@"""", command, @""""), arguments)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var proc = new Process()
                {
                    StartInfo = psi
                };
                proc.OutputDataReceived += (o, e) => output.AppendLine(e.Data);
                proc.ErrorDataReceived += (o, e) => output.AppendLine(e.Data);

                var embed = this.Utilities.BuildEmbed(this.Context, "Executing", string.Concat("Executing `", command, " ", arguments, "`."), AdaUtilities.EmbedColour.Debug);
                var nmsg = await this.ReplyAsync("", false, embed);

                try
                {
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.WaitForExit(5000);
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                        throw new TimeoutException("Execution timed out");
                    }

                    var ostr = output.ToString();
                    if (ostr.Length < 2040)
                        embed = this.Utilities.BuildEmbed(this.Context, "Execution successful", string.Concat("```\n", output.ToString(), "\n```"), AdaUtilities.EmbedColour.Debug);
                    else
                        embed = this.Utilities.BuildEmbed(this.Context, "Execution successful", "The output was too long to post in an embed.", AdaUtilities.EmbedColour.Debug);
                }
                catch (Exception ex)
                {
                    embed = this.Utilities.BuildEmbed(this.Context, "Execution failed", string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message), AdaUtilities.EmbedColour.Debug);
                }

                await nmsg.ModifyAsync(x => x.Embed = embed.Build());
            }
        }
        #endregion
    }
}
