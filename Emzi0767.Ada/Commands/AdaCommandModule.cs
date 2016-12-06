using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Emzi0767.Ada.Attributes;
using Emzi0767.Ada.Commands.Permissions;
using Emzi0767.Ada.Extensions;
using Microsoft.Extensions.PlatformAbstractions;

namespace Emzi0767.Ada.Commands
{
    internal class AdaCommandModule : IAdaCommandModule
    {
        public string Name { get { return "ADA Core Commands"; } }

        [AdaCommand("mkrole", "Creates a new role. This command can only be used by guild administrators.", Aliases = "makerole;createrole;mkgroup;makegroup;creategroup;gmk;gmake;gcreate", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        [AdaCommandParameter(0, "name", "Name of the new role.", true)]
        public async Task CreateRole(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var nam = ctx.RawArguments[0];
            var grl = await gld.CreateRoleAsync(nam, new GuildPermissions(0x0635CC01u), null, false);
            
            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("Role create", string.Concat(usr.Mention, " has created role **", grl.Name, "**."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Format("Role **{0}** was created successfully.", grl.Name), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("rmrole", "Removes a role. This command can only be used by guild administrators.", Aliases = "removerole;deleterole;delrole;rmgroup;removegroup;deletegroup;delgroup;gdel;gdelete;grm;gremove", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        [AdaCommandParameter(0, "name", "Name or mention of the role to delete.", true)]
        public async Task DeleteRole(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var grp = (IRole)null;
            if (msg.MentionedRoleIds.Count > 0)
            {
                grp = gld.GetRole(msg.MentionedRoleIds.First());
            }
            else
            {
                var nam = ctx.RawArguments[0];
                grp = gld.Roles.FirstOrDefault(xr => xr.Name == nam);
            }
            if (grp == null)
                throw new ArgumentException("You must supply a role.");
            await grp.DeleteAsync();

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("Role remove", string.Concat(usr.Mention, " has removed role **", grp.Name, "**."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Format("Role **{0}** was deleted successfully.", grp.Name), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("modrole", "Edits a role. This command can only be used by guild administrators.", Aliases = "modifyrole;editrole;modgroup;modifygroup;editgroup;gmod;gmodify;gedit", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        [AdaCommandParameter(0, "name", "Name or mention of the role to modify.", true)]
        [AdaCommandParameter(1, "properties", "Properties to set. Format is property=value.", true, IsCatchAll = true)]
        public async Task ModifyRole(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var grp = (IRole)null;
            if (msg.MentionedRoleIds.Count > 0)
            {
                grp = gld.GetRole(msg.MentionedRoleIds.First());
            }
            else
            {
                var nam = ctx.RawArguments[0];
                grp = gld.Roles.FirstOrDefault(xr => xr.Name == nam);
            }
            if (grp == null)
                throw new ArgumentException("You must supply a role.");

            var raw = ctx.RawArguments[0];
            var par = ctx.RawArguments
                .Skip(1)
                .Select(xrs => xrs.Split('='))
                .ToDictionary(xrs => xrs[0], xrs => xrs[1]);

            var gpr = par.ContainsKey("permissions") ? ulong.Parse(par["permissions"]) : 0;
            var gcl = par.ContainsKey("color") ? Convert.ToUInt32(par["color"], 16) : 0;
            var ghs = par.ContainsKey("hoist") ? par["hoist"] == "true" : false;
            var gps = par.ContainsKey("position") ? int.Parse(par["position"]) : 0;
            var gmt = par.ContainsKey("mention") ? par["mention"] == "true" : false;

            // TODO: figure out editing mentionability
            await grp.ModifyAsync(x =>
            {
                if (par.ContainsKey("color"))
                    x.Color = gcl;
                if (par.ContainsKey("hoist"))
                    x.Hoist = ghs;
                if (par.ContainsKey("permissions"))
                    x.Permissions = gpr;
                if (par.ContainsKey("position"))
                    x.Position = gps;
                x.Permissions = par.ContainsKey("permissions") ? gpr : grp.Permissions.RawValue;
            });

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("Role modify", string.Concat(usr.Mention, " has modified role **", grp.Name, "**."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Format("Role **{0}** was edited successfully.", grp.Name), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("roleinfo", "Dumps all properties of a role. This command can only be used by guild administrators.", Aliases = "rinfo;dumprole;printrole;dumpgroup;printgroup;gdump", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        [AdaCommandParameter(0, "name", "Name or mention of the role to display.", true)]
        public async Task RoleInfo(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var grp = (IRole)null;
            if (msg.MentionedRoleIds.Count > 0)
            {
                grp = gld.GetRole(msg.MentionedRoleIds.First());
            }
            else
            {
                var nam = string.Join(" ", ctx.RawArguments);
                grp = gld.Roles.FirstOrDefault(xr => xr.Name == nam);
            }
            if (grp == null)
                throw new ArgumentException("You must supply a role.");

            var grl = grp as SocketRole;
            var gls = gld as SocketGuild;

            var embed = this.PrepareEmbed("Role Info", null, EmbedType.Info);

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Name";
                x.Value = grl.Name;
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "ID";
                x.Value = grl.Id.ToString();
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Color";
                x.Value = grl.Color.RawValue.ToString("X6");
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Hoisted?";
                x.Value = grl.IsHoisted ? "Yes" : "No";
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Mentionable?";
                x.Value = grl.IsMentionable ? "Yes" : "No";
            });

            var perms = new List<string>(23);
            if (grl.Permissions.Administrator)
                perms.Add("Administrator");
            if (grl.Permissions.AttachFiles)
                perms.Add("Can attach files");
            if (grl.Permissions.BanMembers)
                perms.Add("Can ban members");
            if (grl.Permissions.ChangeNickname)
                perms.Add("Can change nickname");
            if (grl.Permissions.Connect)
                perms.Add("Can use voice chat");
            if (grl.Permissions.CreateInstantInvite)
                perms.Add("Can create instant invites");
            if (grl.Permissions.DeafenMembers)
                perms.Add("Can deafen members");
            if (grl.Permissions.EmbedLinks)
                perms.Add("Can embed links");
            if (grl.Permissions.KickMembers)
                perms.Add("Can kick members");
            if (grl.Permissions.ManageChannels)
                perms.Add("Can manage channels");
            if (grl.Permissions.ManageMessages)
                perms.Add("Can manage messages");
            if (grl.Permissions.ManageNicknames)
                perms.Add("Can manage nicknames");
            if (grl.Permissions.ManageRoles)
                perms.Add("Can manage roles");
            if (grl.Permissions.ManageGuild)
                perms.Add("Can manage guild");
            if (grl.Permissions.MentionEveryone)
                perms.Add("Can mention everyone group");
            if (grl.Permissions.MoveMembers)
                perms.Add("Can move members between voice channels");
            if (grl.Permissions.MuteMembers)
                perms.Add("Can mute members");
            if (grl.Permissions.ReadMessageHistory)
                perms.Add("Can read message history");
            if (grl.Permissions.ReadMessages)
                perms.Add("Can read messages");
            if (grl.Permissions.SendMessages)
                perms.Add("Can send messages");
            if (grl.Permissions.SendTTSMessages)
                perms.Add("Can send TTS messages");
            if (grl.Permissions.Speak)
                perms.Add("Can speak");
            if (grl.Permissions.UseVAD)
                perms.Add("Can use voice activation");
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Permissions";
                x.Value = string.Join(", ", perms);
            });

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("listroles", "Lists all roles on the server. This command can only be used by guild administrators.", Aliases = "lsroles;lsgroups;listgroups;glist;gls", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task ListRoles(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            
            var grp = gld.Roles;
            if (grp == null)
                return;

            var embed = this.PrepareEmbed("Role List", string.Format("Listing of all {0:#,##0} role{1} in this Guild.", grp.Count, grp.Count > 1 ? "s" : ""), EmbedType.Info);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Role list";
                x.Value = string.Join(", ", grp.Select(xr => string.Concat("**", xr.Name, "**")));
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("roleadd", "Adds users to a role. This command can only be used by guild administrators.", Aliases = "groupadd;ugadd", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        [AdaCommandParameter(0, "role", "Name or mention of the role to add to.", true)]
        [AdaCommandParameter(1, "users", "Mentions of users to add to the role.", true, IsCatchAll = true)]
        public async Task RoleAdd(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;
            
            var grp = (IRole)null;
            if (msg.MentionedRoleIds.Count > 0)
            {
                grp = gld.GetRole(msg.MentionedRoleIds.First());
            }
            else
            {
                var nam = ctx.RawArguments[0];
                grp = gld.Roles.FirstOrDefault(xr => xr.Name == nam);
            }
            if (grp == null)
                throw new ArgumentException("You must supply a role.");
            
            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var usrs = msg.MentionedUserIds.Select(xid => gls.Users.FirstOrDefault(xusr => xusr.Id == xid));
            if (usrs.Count() == 0)
                throw new ArgumentException("You must mention users you want to add to a role.");

            foreach (var usm in usrs)
                await usm.AddRolesAsync(grp);

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("Role Member Add", string.Concat(usr.Mention, " has added ", string.Join(", ", usrs.Select(xusr => xusr.Mention)), " to role **", grp.Name, "**."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Concat("User", usrs.Count() > 1 ? "s were" : " was", " added to the role."), EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Details";
                x.Value = string.Concat("The following user", usrs.Count() > 1 ? "s were" : " was", " added to role **", grp.Name, "**: ", string.Join(", ", usrs.Select(xusr => xusr.Mention)));
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("roleremove", "Removes users from a role. This command can only be used by guild administrators.", Aliases = "groupremove;ugremove;ugrm", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        [AdaCommandParameter(0, "role", "Name or mention of the role to remove from.", true)]
        [AdaCommandParameter(1, "users", "Mentions of users to remove from the role.", true, IsCatchAll = true)]
        public async Task RoleRemove(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var grp = (IRole)null;
            if (msg.MentionedRoleIds.Count > 0)
            {
                grp = gld.GetRole(msg.MentionedRoleIds.First());
            }
            else
            {
                var nam = ctx.RawArguments[0];
                grp = gld.Roles.FirstOrDefault(xr => xr.Name == nam);
            }
            if (grp == null)
                throw new ArgumentException("You must supply a role.");

            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var usrs = msg.MentionedUserIds.Select(xid => gls.Users.FirstOrDefault(xusr => xusr.Id == xid));
            if (usrs.Count() == 0)
                throw new ArgumentException("You must mention users you want to remove from a role.");

            foreach (var usm in usrs)
                await usm.RemoveRolesAsync(grp);

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("Role Member Remove", string.Concat(usr.Mention, " has removed ", string.Join(", ", usrs.Select(xusr => xusr.Mention)), " from role **", grp.Name, "**."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Concat("User", usrs.Count() > 1 ? "s were" : " was", " removed from the role."), EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Details";
                x.Value = string.Concat("The following user", usrs.Count() > 1 ? "s were" : " was", " removed from role **", grp.Name, "**: ", string.Join(", ", usrs.Select(xusr => xusr.Mention)));
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("report", "Reports a user to guild moderators.", Aliases = "reportuser", CheckPermissions = false)]
        [AdaCommandParameter(0, "user", "Mention of a user to report.", true)]
        [AdaCommandParameter(1, "reason", "Reason for report.", true, IsCatchAll = true)]
        public async Task Report(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            if (msg.MentionedUserIds.Count == 0)
                throw new ArgumentException("You need to mention the user you want to report.");
            var rep = await gld.GetUserAsync(msg.MentionedUserIds.First());

            var rsn = string.Join(" ", ctx.RawArguments.Skip(1));
            if (string.IsNullOrWhiteSpace(rsn))
                throw new ArgumentException("You need to supply a report reason.");

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            if (cnf.ModLogChannel == null)
                throw new InvalidOperationException("This guild does not have moderator log configured.");

            var mod = await gld.GetTextChannelAsync(cnf.ModLogChannel.Value);

            var embed1 = this.PrepareEmbed("User report", string.Concat(usr.Mention, " reported ", rep.Mention, "."), EmbedType.Info);
            embed1.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Reason";
                x.Value = rsn;
            });

            var embed2 = this.PrepareEmbed("Success", string.Concat("User ", rep.Mention, " was reported."), EmbedType.Success);

            await mod.SendMessageAsync("", false, embed1);
            await chn.SendMessageAsync("", false, embed2);
        }

        [AdaCommand("kick", "Kicks users. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        [AdaCommandParameter(0, "users", "Mentions of users to kick.", true, IsCatchAll = true)]
        public async Task Kick(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var gls = gld as SocketGuild;
            var uss = msg.MentionedUserIds.Select(xid => gls.GetUser(xid));
            if (uss.Count() < 1)
                throw new ArgumentException("You must mention users you want to kick.");

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            uss = uss.Where(xus => !xus.GuildPermissions.Administrator);
            foreach (var usm in uss)
                await usm.KickAsync();

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("User kicks", string.Concat(usr.Mention, " has kicked ", string.Join(", ", uss.Select(xus => xus.Mention)), "."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed(EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "User Kicked";
                x.Value = string.Concat("The following user", uss.Count() > 1 ? "s were" : " was" , " kicked: ", string.Join(", ", uss.Select(xusr => xusr.Mention)));
            });

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("ban", "Bans users. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.BanMembers)]
        [AdaCommandParameter(0, "users", "Mentions of users to ban.", true, IsCatchAll = true)]
        public async Task Ban(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var gls = gld as SocketGuild;
            var uss = msg.MentionedUserIds.Select(xid => gls.GetUser(xid));
            if (uss.Count() < 1)
                throw new ArgumentException("You must mention users you want to ban.");
            
            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            uss = uss.Where(xus => !xus.GuildPermissions.Administrator);
            foreach (var usm in uss)
                await gls.AddBanAsync(usm);

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("User bans", string.Concat(usr.Mention, " has banned ", string.Join(", ", uss.Select(xus => xus.Mention)), "."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }
            
            var embed = this.PrepareEmbed(EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "User Bans";
                x.Value = string.Concat("The following user", uss.Count() > 1 ? "s were" : " was", " banned: ", string.Join(", ", uss.Select(xusr => xusr.Mention)));
            });

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("prune", "Prunes inactive users. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        public async Task Prune(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var usp = await gld.PruneUsersAsync();

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("User prune", string.Concat(usr.Mention, " has pruned ", usp.ToString("#,##0"), " users."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Concat(usp.ToString("#,##0"), " user", usp > 1 ? "s were" : " was" , " pruned."), EmbedType.Success);
        }

        [AdaCommand("userinfo", "Displays information about users matching given name. This command can only be used by guild administrators.", Aliases = "uinfo;userlist;ulist;userfind;ufind", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        [AdaCommandParameter(0, "user", "Mention of user to display.", true)]
        public async Task UserInfo(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var usrs = msg.MentionedUserIds;
            if (usrs.Count == 0)
                throw new ArgumentException("You need to mention a user whose information you want to see.");

            var usr = await gld.GetUserAsync(usrs.First()) as SocketGuildUser;
            if (usr == null)
                throw new ArgumentNullException("Specified user is invalid.");

            var embed = this.PrepareEmbed(EmbedType.Info);
            if (!string.IsNullOrWhiteSpace(usr.AvatarUrl))
                embed.ThumbnailUrl = usr.AvatarUrl;

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Username";
                x.Value = string.Concat("**", usr.Username, "**#", usr.DiscriminatorValue);
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "ID";
                x.Value = usr.Id.ToString();
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Nickname";
                x.Value = usr.Nickname ?? string.Concat("**", usr.Username, "**#", usr.DiscriminatorValue);
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Status";
                x.Value = usr.Status.ToString();
            });

            if (usr.Game != null)
                embed.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "Game";
                    x.Value = usr.Game.Value.Name;
                });
            
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Roles";
                x.Value = string.Join(", ", usr.RoleIds.Select(xid => string.Concat("**", gld.GetRole(xid).Name, "**")));
            });

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("guildinfo", "Displays information about current guild. This command can only be used by guild administrators.", Aliases = "ginfo", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageGuild)]
        public async Task GuildInfo(AdaCommandContext ctx)
        {
            var gld = ctx.Guild as SocketGuild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var embed = this.PrepareEmbed(EmbedType.Info);
            if (!string.IsNullOrWhiteSpace(gld.IconUrl))
                embed.ThumbnailUrl = gld.IconUrl;

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Name";
                x.Value = gld.Name;
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "ID";
                x.Value = gld.Id.ToString();
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Voice Region";
                x.Value = gld.VoiceRegionId;
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Owner";
                x.Value = gld.GetOwnerAsync().GetAwaiter().GetResult().Mention;
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Default Channel";
                x.Value = gld.GetDefaultChannelAsync().GetAwaiter().GetResult().Mention;
            });

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("purgechannel", "Purges a channel. Removes up to 100 messages. This command can only be used by guild administrators.", Aliases = "purgech;chpurge;chanpurge;purgechan", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        [AdaCommandParameter(0, "channel", "Mention of channel to purge.", true)]
        public async Task PurgeChannel(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            if (msg.MentionedChannelIds.Count == 0)
                throw new ArgumentException("You need to mention a channel you want to purge");
            var gls = gld as SocketGuild;
            var chp = gls.Channels.FirstOrDefault(xch => xch.Id == msg.MentionedChannelIds.First()) as SocketTextChannel;
            var msgs = await chp.GetMessagesAsync(100).Flatten();
            await chp.DeleteMessagesAsync(msgs);

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("Channel Purge", string.Concat(usr.Mention, " has purged ", msgs.Count().ToString("#,##0"), " messages from channel ", chp.Mention, "."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Format("Deleted {0:#,##0} message{2} from channel {1}.", msgs.Count(), chp.Mention, msgs.Count() > 1 ? "s" : ""), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("guildconfig", "Manages ADA configuration for this guild. This command can only be used by guild administrators.", Aliases = "guildconf;adaconfig;adaconf;modconfig;modconf", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        [AdaCommandParameter(0, "channel", "Mention of a channel to be used as mod log.", true)]
        public async Task ModConfig(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            if (ctx.RawArguments.Count == 0)
                throw new ArgumentException("You need to specify setting and value.");
            var setting = ctx.RawArguments[0];
            var val = string.Empty;
            var embed = (EmbedBuilder)null;

            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gld.Id);
            if (setting == "modlog")
            {
                var mod = (ITextChannel)null;
                if (msg.MentionedChannelIds.Count() > 0)
                {
                    mod = await gld.GetTextChannelAsync(msg.MentionedChannelIds.First());
                    var bot = AdaBotCore.AdaClient.CurrentUser;
                    var prm = mod.GetPermissionOverwrite(bot);
                    if (prm != null && prm.Value.SendMessages == PermValue.Deny)
                        throw new InvalidOperationException("ADA cannot write to specified channel.");
                }

                val = mod != null ? mod.Mention : "<null>";
                cnf.ModLogChannel = mod != null ? (ulong?)mod.Id : null;
                embed = this.PrepareEmbed("Success", string.Concat("Moderator log was ", mod != null ? string.Concat("set to ", mod.Mention) : "removed", "."), EmbedType.Success);
            }
            else if (setting == "prefix")
            {
                var pfix = (string)null;
                if (ctx.RawArguments.Count >= 2)
                    pfix = string.Join(" ", ctx.RawArguments.Skip(1));
                if (string.IsNullOrWhiteSpace(pfix))
                    pfix = null;

                val = pfix ?? "<default>";
                cnf.CommandPrefix = pfix;
                embed = this.PrepareEmbed("Success", string.Concat("Command prefix was set to ", pfix != null ? string.Concat("**", pfix, "**") : "default", "."), EmbedType.Success);
            }
            else if (setting == "deletecommands")
            {
                var delcmd = false;
                if (ctx.RawArguments.Count > 1 && ctx.RawArguments[1] == "enable")
                    delcmd = true;

                val = delcmd.ToString();
                cnf.DeleteCommands = delcmd;
                embed = this.PrepareEmbed("Success", string.Concat("Command message deletion is now **", delcmd ? "enabled" : "disabled", "**."), EmbedType.Success);
            }
            else
                throw new ArgumentException("Invalid setting specified.");
            AdaBotCore.ConfigManager.SetGuildConfig(gld.Id, cnf);

            if (cnf.ModLogChannel != null)
            {
                var mod = await gld.GetTextChannelAsync(cnf.ModLogChannel.Value);
                var embedmod = this.PrepareEmbed("Config updated", string.Concat(usr.Mention, " has has updated guild setting **", setting, "** with value **", val, "**."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("adanick", "Changes ADA nickname in this guild. This command can only be used by guild administrators.", Aliases = "adaname", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageNicknames)]
        [AdaCommandParameter(0, "nickname", "New nickname to use.", false, IsCatchAll = true)]
        public async Task AdaNick(AdaCommandContext ctx)
        {
            var gld = ctx.Guild as SocketGuild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var nck = string.Join(" ", ctx.RawArguments);
            if (string.IsNullOrWhiteSpace(nck))
                nck = "";
            else
                nck = string.Concat(AdaBotCore.AdaClient.CurrentUser.Username, " (", nck, ")");
            var ada = gld.GetUser(AdaBotCore.AdaClient.CurrentUser.Id);
            await ada.ModifyAsync(x =>
            {
                x.Nickname = nck;
            });

            var gid = gld.Id;
            var cnf = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            var mod = cnf != null && cnf.ModLogChannel != null ? await gld.GetTextChannelAsync(cnf.ModLogChannel.Value) : null;

            if (mod != null)
            {
                var embedmod = this.PrepareEmbed("ADA Nickname Change", string.Concat(usr.Mention, " has changed ADA nickname to **", nck == "" ? AdaBotCore.AdaClient.CurrentUser.Username : nck, "**."), EmbedType.Info);
                await mod.SendMessageAsync("", false, embedmod);
            }

            var embed = this.PrepareEmbed("Success", string.Concat("Nickname set to **", nck == "" ? AdaBotCore.AdaClient.CurrentUser.Username : nck, "**."), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("adahelp", "Shows command list. Add command name to learn more.", Aliases = "help", CheckPermissions = false)]
        [AdaCommandParameter(0, "command", "Command to display details of.", false)]
        public async Task Help(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            var embed = (EmbedBuilder)null;
            if (ctx.RawArguments.Count == 0)
            {
                embed = this.PrepareEmbed("ADA Help", string.Format("List of all ADA commands, with aliases, and descriptions. All commands use the **{0}** prefix. Run **{0}adahelp** command to learn more about a specific command.", AdaBotCore.CommandManager.GetPrefix(gld.Id)), EmbedType.Info);
                foreach (var cmdg in AdaBotCore.CommandManager.GetCommands().GroupBy(xcmd => xcmd.Module))
                {
                    var err = "";
                    var xcmds = cmdg.Where(xcmd => (xcmd.Checker != null && xcmd.Checker.CanRun(xcmd, usr, msg, chn, gld, out err)) || xcmd.Checker == null)
                        .OrderBy(xcmd => xcmd.Name)
                        .Select(xcmd => string.Concat("**", xcmd.Name, "**"));

                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = string.Format("Commands registered by {0}", cmdg.Key.Name);
                        x.Value = string.Join(", ", xcmds);
                    });
                }
            }
            else
            {
                var cmdn = ctx.RawArguments[0];
                var cmd = AdaBotCore.CommandManager.GetCommand(cmdn);
                if (cmd == null)
                    throw new InvalidOperationException(string.Format("Command **{0}** does not exist", cmdn));
                var err = (string)null;
                if (cmd.Checker != null && !cmd.Checker.CanRun(cmd, usr, msg, chn, gld, out err))
                    throw new ArgumentException("You can't run this command.");

                embed = this.PrepareEmbed("ADA Help", string.Format("**{0}** Command help", cmd.Name), EmbedType.Info);
                
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Description";
                    x.Value = cmd.Description;
                });

                if (cmd.Aliases != null && cmd.Aliases.Count > 0)
                {
                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Aliases";
                        x.Value = string.Join(", ", cmd.Aliases.Select(xa => string.Concat("**", xa, "**")));
                    });
                }

                if (cmd.Parameters.Count > 0)
                {
                    var sb1 = new StringBuilder();
                    var sb2 = new StringBuilder();
                    sb1.Append(AdaBotCore.CommandManager.GetPrefix(gld.Id)).Append(cmd.Name).Append(' ');
                    foreach (var param in cmd.Parameters.OrderBy(xp => xp.Order))
                    {
                        sb1.Append(param.IsRequired ? '<' : '[').Append(param.Name).Append(param.IsCatchAll ? "..." : "").Append(param.IsRequired ? '>' : ']').Append(' ');
                        sb2.Append("**").Append(param.Name).Append("**: ").AppendLine(param.Description);
                    }
                    sb1.AppendLine();
                    sb1.Append(sb2.ToString());

                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Usage";
                        x.Value = sb1.ToString();
                    });
                }
            }

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("aboutada", "Shows information about ADA.", CheckPermissions = false)]
        public async Task About(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var a = AdaBotCore.PluginManager.MainAssembly;
            var n = a.GetName();

            var gls = gld as SocketGuild;
            var embed = this.PrepareEmbed("About ADA", "Hi! I am ADA, or Advanced (although Automatic is also applicable) Discord Administrator. A bot created by Emzi0767 to simplify several administrative tasks for discord servers. I first went live on 2016-11-17.", EmbedType.Info);
            embed.ImageUrl = "http://i.imgur.com/Nykuwgj.jpg";

            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "What can you do?";
                x.Value = string.Format("You can see the list of commands available to you by invoking **{0}adahelp**. Some commands might not be available to you, depending on this server's policy.", AdaBotCore.CommandManager.GetPrefix(gld.Id));
            });

            if (gls != null)
            {
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Where else do you run?";
                    x.Value = string.Format("I currently run on {0:#,##0} servers. If you want to add me to your server, contact <@181875147148361728>.", gls.Discord.Guilds.Count());
                });
            }

            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "What is your present version and status?";
                x.Value = string.Format("Current ADA version is {0}. There are {1:#,##0} plugins loaded, {4:#,##0} plugin configurations managed, {2:#,##0} commands registered, and {3:#,##0} checkers registered.", n.Version, AdaBotCore.PluginManager.PluginCount, AdaBotCore.CommandManager.CommandCount, AdaBotCore.CommandManager.CheckerCount, AdaBotCore.ConfigManager.ConfigCount);
            });

            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Are you Open-Source?";
                x.Value = "Yes! I am fully Open-Source, my code is licensed under Apache License 2.0. You can view and contribute to it at <https://github.com/Emzi0767/Discord-ADA-Bot>.";
            });

            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "What do you look like in real life?";
                x.Value = "I am hosted on a Raspberry Pi 2, model B. The device is enclosed in a black case, and is connected to local network using a yellow Cat5 cord. Additionally, there is a silver monoblock USB thumb drive attached to it at all times, it serves as external storage for various purposes. Pi draws power from a nearby extension cord through a black USB cable plugged into its power connector. It's stacked on top of a dormant Raspberry Pi, model B, revision 2.0. If you have trouble imagining that, here's what it looks like:";
            });
            
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("fulldump", "Performs a full environment dump. This command can only be used by Emzi0767.", CheckerId = "CoreDebugChecker", CheckPermissions = true)]
        public async Task FullDump(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            // ada assembly data
            var ada_a = AdaBotCore.PluginManager.MainAssembly;
            var ada_n = ada_a.GetName();
            var ada_l = ada_a.Location;

            // ada process data
            var ada_p = Process.GetCurrentProcess();
            var ada_m = ada_p.Modules;

            // ada environment
            var ada_e = PlatformServices.Default;

            // dump holders
            var ada_sb0 = (StringBuilder)null;

            // create the dump
            var embed = this.PrepareEmbed("ADA Diagnostic Information", "Full dump of all diagnostic information about this ADA instance.", EmbedType.Warning);

            // dump process info
            ada_sb0 = new StringBuilder();
            ada_sb0.AppendFormat("**PID**: {0}", ada_p.Id).AppendLine();
            ada_sb0.AppendFormat("**Name**: '{0}'", ada_p.ProcessName).AppendLine();
            //ada_sb0.AppendFormat("**Is 64-bit**: {0}", Environment.Is64BitProcess ? "Yes" : "No").AppendLine();
            ada_sb0.AppendFormat("**Is 64-bit**: {0}", IntPtr.Size == 8 ? "Yes" : "No").AppendLine();
            //ada_sb0.AppendFormat("**Command line**: {0} {1}", ada_p.StartInfo.FileName, ada_p.StartInfo.Arguments).AppendLine();
            ada_sb0.AppendFormat("**Started**: {0:yyyy-MM-dd HH:mm:ss} UTC", ada_p.StartTime.ToUniversalTime()).AppendLine();
            ada_sb0.AppendFormat("**Thread count**: {0:#,##0}", ada_p.Threads.Count).AppendLine();
            ada_sb0.AppendFormat("**Total processor time**: {0:c}", ada_p.TotalProcessorTime).AppendLine();
            ada_sb0.AppendFormat("**User processor time**: {0:c}", ada_p.UserProcessorTime).AppendLine();
            ada_sb0.AppendFormat("**Privileged processor time**: {0:c}", ada_p.PrivilegedProcessorTime).AppendLine();
            //ada_sb0.AppendFormat("**Handle count**: {0:#,##0}", ada_p.HandleCount).AppendLine();
            ada_sb0.AppendFormat("**Working set**: {0}", ada_p.WorkingSet64.ToSizeString()).AppendLine();
            ada_sb0.AppendFormat("**Virtual memory size**: {0}", ada_p.VirtualMemorySize64.ToSizeString()).AppendLine();
            ada_sb0.AppendFormat("**Paged memory size**: {0}", ada_p.PagedMemorySize64.ToSizeString()).AppendLine();
            ada_sb0.AppendFormat("**Module count**: {0:#,##0}", ada_m.Count);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "ADA Process";
                x.Value = ada_sb0.ToString();
            });

            // dump process module info
            //ada_sb0 = new StringBuilder();
            //foreach (ProcessModule ada_xm in ada_m)
            //{
            //    ada_sb0.AppendFormat("**Name**: {0}", ada_xm.ModuleName).AppendLine();
            //    ada_sb0.AppendFormat("**File name**: {0}", ada_xm.FileName).AppendLine();
            //    ada_sb0.AppendFormat("**File version**: {0}", ada_xm.FileVersionInfo.FileVersion).AppendLine();
            //    ada_sb0.AppendFormat("**Product version**: {0}", ada_xm.FileVersionInfo.ProductVersion).AppendLine();
            //    ada_sb0.AppendFormat("**Product name**: {0}", ada_xm.FileVersionInfo.ProductName).AppendLine();
            //    ada_sb0.AppendFormat("**Base address**: {0}", ada_xm.BaseAddress.ToPointerString()).AppendLine();
            //    ada_sb0.AppendFormat("**Entry point address**: {0}", ada_xm.EntryPointAddress.ToPointerString()).AppendLine();
            //    ada_sb0.AppendFormat("**Memory size**: {0}", ada_xm.ModuleMemorySize.ToSizeString()).AppendLine();
            //    ada_sb0.AppendLine("---------");
            //}
            //embed.AddField(x =>
            //{
            //    x.IsInline = false;
            //    x.Name = "ADA Process Modules";
            //    x.Value = ada_sb0.ToString();
            //});

            // dump assembly info
            ada_sb0 = new StringBuilder();
            ada_sb0.AppendFormat("**Name**: {0}", ada_n.FullName).AppendLine();
            ada_sb0.AppendFormat("**Version**: {0}", ada_n.Version).AppendLine();
            ada_sb0.AppendFormat("**Location**: {0}", ada_l).AppendLine();
            ada_sb0.AppendFormat("**Code base**: {0}", ada_a.CodeBase).AppendLine();
            ada_sb0.AppendFormat("**Entry point**: {0}.{1}", ada_a.EntryPoint.DeclaringType, ada_a.EntryPoint.Name).AppendLine();
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "ADA Assembly";
                x.Value = ada_sb0.ToString();
            });

            // dump environment info
            ada_sb0 = new StringBuilder();
            //ada_sb0.AppendFormat("**OS platform**: {0}", Environment.OSVersion.Platform.ToString()).AppendLine();
            //ada_sb0.AppendFormat("**OS version**: {0} ({1}); Service Pack: {2}", Environment.OSVersion.Version, Environment.OSVersion.VersionString, Environment.OSVersion.ServicePack).AppendLine();
            //ada_sb0.AppendFormat("**OS is 64-bit**: {0}", Environment.Is64BitOperatingSystem ? "Yes" : "No").AppendLine();
            ada_sb0.AppendFormat("**.NET environment version**: {0}", ada_e.Application.RuntimeFramework.Version).AppendLine();
            ada_sb0.AppendFormat("**.NET is Mono**: {0}", Type.GetType("Mono.Runtime") != null ? "Yes" : "No").AppendLine();
            ada_sb0.AppendFormat("**Heap size**: {0}", GC.GetTotalMemory(false).ToSizeString());
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "OS and .NET";
                x.Value = ada_sb0.ToString();
            });

            // dump appdomain assembly info
            //foreach (var ada_xa in ada_s)
            //{
            //    ada_sb0 = new StringBuilder();
            //    ada_sb0.AppendFormat("Name: {0}", ada_xa.FullName).AppendLine();
            //    ada_sb0.AppendFormat("Version: {0}", ada_xa.GetName().Version).AppendLine();
            //    if (!ada_xa.IsDynamic)
            //    {
            //        ada_sb0.AppendFormat("Location: {0}", ada_xa.Location).AppendLine();
            //        ada_sb0.AppendFormat("Code base: {0}", ada_xa.CodeBase).AppendLine();
            //    }
            //    if (ada_xa.EntryPoint != null)
            //        ada_sb0.AppendFormat("Entry point: {0}.{1}", ada_xa.EntryPoint.DeclaringType, ada_xa.EntryPoint.Name).AppendLine();
            //    ada_sb0.AppendLine("---------");
            //}
            //embed.AddField(x =>
            //{
            //    x.IsInline = false;
            //    x.Name = "ADA AppDomain Assemblies";
            //    x.Value = ada_sb0.ToString();
            //});
            //ada_sb0 = null;
            
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("hang", "Hangs current thread. This command can only be used by Emzi0767.", CheckerId = "CoreDebugChecker", CheckPermissions = true)]
        [AdaCommandParameter(0, "timeout", "How long to hang the thread for.", false)]
        public async Task Hang(AdaCommandContext ctx)
        {
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var duration = 42510;
            if (ctx.RawArguments.Count > 0)
                if (!int.TryParse(ctx.RawArguments[0], out duration))
                    duration = 42510;

            await Task.Delay(duration);

            var embed = this.PrepareEmbed("Thread hang complete", string.Concat("Thread was hanged for ", duration.ToString("#,##0"), "ms."), EmbedType.Warning);
            await chn.SendMessageAsync("", false, embed);
        }

        private EmbedBuilder PrepareEmbed(EmbedType type)
        {
            var embed = new EmbedBuilder();
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
