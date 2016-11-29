using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Commands.Permissions;
using Emzi0767.Net.Discord.AdaBot.Extensions;

namespace Emzi0767.Net.Discord.AdaBot.Commands
{
    internal class AdaCommandModule : IAdaCommandModule
    {
        //var gld = ctx.Guild;
        //var chn = ctx.Channel;
        //var msg = ctx.Message;
        //var usr = ctx.User;

        public string Name { get { return "ADA Core Commands"; } }

        [AdaCommand("mkrole", "Creates a new role with specified name. This command can only be used by server administrators.", Aliases = "makerole;createrole;mkgroup;makegroup;creategroup;gmk;gmake;gcreate", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task CreateGroup(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            var nam = ctx.RawArguments[0];
            await gld.CreateRoleAsync(nam, new GuildPermissions(0x0635CC01u), null, false);
            var embed = this.PrepareEmbed("Success", string.Format("Role \"{0}\" was created successfully.", nam));
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("rmrole", "Removes a role with specified name. This command can only be used by server administrators.", Aliases = "removerole;deleterole;delrole;rmgroup;removegroup;deletegroup;delgroup;gdel;gdelete;grm;gremove", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task DeleteGroup(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

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
                return;

            await grp.DeleteAsync();
            var embed = this.PrepareEmbed("Success", string.Format("Role \"{0}\" was deleted successfully.", grp.Name));
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("modrole", "Edits a role with specified name. This command can only be used by server administrators.", Aliases = "modifyrole;editrole;modgroup;modifygroup;editgroup;gmod;gmodify;gedit", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task ModifyGroup(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

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
                return;

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
            });

            var embed = this.PrepareEmbed("Success", string.Format("Role \"{0}\" was edited successfully.", grp.Name));
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("dumprole", "Dumps all properties of a role. This command can only be used by server administrators.", Aliases = "printrole;dumpgroup;printgroup;gdump", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task DumpGroup(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            var nam = ctx.RawArguments[0];
            var grp = gld.Roles.FirstOrDefault(xr => xr.Name == nam);
            var grl = grp as SocketRole;
            if (grl == null)
                return;

            await gld.DownloadUsersAsync();
            var gls = gld as SocketGuild;
            var sb = new StringBuilder();
            sb.AppendLine("**ADA**: Dumping all properties of a role");
            sb.AppendFormat("**Name**: {0}", grl.Name).AppendLine();
            sb.AppendFormat("**ID**: {0}", grl.Id).AppendLine();
            sb.AppendFormat("**Color**: {0:X6}", grl.Color.RawValue).AppendLine();
            sb.AppendFormat("**Is hoisted**: {0}", grl.IsHoisted ? "Yes" : "No").AppendLine();
            sb.AppendFormat("**Is everyone**: {0}", grl.IsEveryone ? "Yes" : "No").AppendLine();
            sb.AppendFormat("**Is mentionable**: {0}", grl.IsMentionable ? "Yes" : "No").AppendLine();
            if (grl.IsMentionable)
                sb.AppendFormat("**Mention**: {0}", grl.Mention).AppendLine();
            sb.AppendFormat("**Position**: {0}", grl.Position).AppendLine();
            sb.AppendFormat("**Total members**: {0:#,##0}", gls.Users.Where(xus => xus.RoleIds.Contains(grl.Id)).Count()).AppendLine();
            sb.AppendLine("**Permissions**:");
            sb.AppendFormat("**Raw value**: {0}", grl.Permissions.RawValue).AppendLine();
            if (grl.Permissions.Administrator)
                sb.AppendLine("+ Administrator");
            if (grl.Permissions.AttachFiles)
                sb.AppendLine("+ Can attach files");
            if (grl.Permissions.BanMembers)
                sb.AppendLine("+ Can ban members");
            if (grl.Permissions.ChangeNickname)
                sb.AppendLine("+ Can change nickname");
            if (grl.Permissions.Connect)
                sb.AppendLine("+ Can use voice chat");
            if (grl.Permissions.CreateInstantInvite)
                sb.AppendLine("+ Can create instant invites");
            if (grl.Permissions.DeafenMembers)
                sb.AppendLine("+ Can deafen members");
            if (grl.Permissions.EmbedLinks)
                sb.AppendLine("+ Can embed links");
            if (grl.Permissions.KickMembers)
                sb.AppendLine("+ Can kick members");
            if (grl.Permissions.ManageChannels)
                sb.AppendLine("+ Can manage channels");
            if (grl.Permissions.ManageMessages)
                sb.AppendLine("+ Can manage messages");
            if (grl.Permissions.ManageNicknames)
                sb.AppendLine("+ Can manage nicknames");
            if (grl.Permissions.ManageRoles)
                sb.AppendLine("+ Can manage roles");
            if (grl.Permissions.ManageGuild)
                sb.AppendLine("+ Can manage guild");
            if (grl.Permissions.MentionEveryone)
                sb.AppendLine("+ Can mention everyone group");
            if (grl.Permissions.MoveMembers)
                sb.AppendLine("+ Can move members between voice channels");
            if (grl.Permissions.MuteMembers)
                sb.AppendLine("+ Can mute members");
            if (grl.Permissions.ReadMessageHistory)
                sb.AppendLine("+ Can read message history");
            if (grl.Permissions.ReadMessages)
                sb.AppendLine("+ Can read messages");
            if (grl.Permissions.SendMessages)
                sb.AppendLine("+ Can send messages");
            if (grl.Permissions.SendTTSMessages)
                sb.AppendLine("+ Can send TTS messages");
            if (grl.Permissions.Speak)
                sb.AppendLine("+ Can speak");
            if (grl.Permissions.UseVAD)
                sb.AppendLine("+ Can use voice activation");

            await chn.SendMessageAsync(sb.ToString());
        }

        [AdaCommand("listroles", "Lists all roles on the server. This command can only be used by server administrators.", Aliases = "lsroles;lsgroups;listgroups;glist;gls", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task ListGroups(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            var nam = ctx.RawArguments[0];
            var grp = gld.Roles;
            if (grp == null)
                return;

            var sb = new StringBuilder();
            sb.AppendFormat("**ADA**: All roles ({0:#,##0}):", grp.Count()).AppendLine();
            foreach (var xgrp in grp)
                sb.AppendLine(xgrp.IsMentionable ? xgrp.Mention : xgrp.Name);

            await chn.SendMessageAsync(sb.ToString());
        }

        [AdaCommand("roleadd", "Adds a user to a role. This command can only be used by server administrators.", Aliases = "groupadd;ugadd", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task GroupAdd(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            var raw = ctx.RawArguments[0];
            var par = raw.Split(';')
                .Select(xrs => xrs.Split('='))
                .ToDictionary(xrs => xrs[0], xrs => xrs[1]);

            var usn = par.ContainsKey("user") ? par["user"] : null;
            var grn = par.ContainsKey("role") ? par["role"] : null;

            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var usg = gls.Users;
            var gru = gld.Roles.FirstOrDefault(xr => xr.Name == grn);
            if (usg.Count() != 1 || gru == null)
                return;

            await usg.FirstOrDefault().AddRolesAsync(gru);
            await chn.SendMessageAsync(string.Format("**ADA**: Added {0} to {1}", usg.FirstOrDefault().Username, gru.Name));
        }

        [AdaCommand("roleremove", "Removes user from a role. This command can only be used by server administrators.", Aliases = "groupremove;ugremove;ugrm", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public async Task GroupRemove(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            var raw = ctx.RawArguments[0];
            var par = raw.Split(';')
                .Select(xrs => xrs.Split('='))
                .ToDictionary(xrs => xrs[0], xrs => xrs[1]);

            var usn = par.ContainsKey("user") ? par["user"] : null;
            var grn = par.ContainsKey("role") ? par["role"] : null;

            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var usg = gls.Users.Where(xus => xus.Username == usn);
            var gru = gls.Roles.FirstOrDefault(xr => xr.Name == grn);
            if (usg.Count() != 1 || gru == null)
                return;

            await usg.FirstOrDefault().RemoveRolesAsync(gru);
            await chn.SendMessageAsync(string.Format("**ADA**: Removed {0} from {1}", usg.FirstOrDefault().Username, gru.Name));
        }

        [AdaCommand("kick", "Kicks a user by name. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        public async Task Kick(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var nam = ctx.RawArguments[0];

            await msg.DeleteAsync();

            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var uss = gls.Users.Where(xus => xus.Username == nam);
            if (uss.Count() > 1)
            {
                await chn.SendMessageAsync(string.Format("**ADA**: Query for user '{0}' returned more than one user, try to be more specific.", nam));
                return;
            }

            var usk = uss.First();
            await usk.KickAsync();
            await chn.SendMessageAsync(string.Format("**ADA**: Kicked user '{0}'", usk.Username));
        }

        [AdaCommand("kickid", "Kicks a user by id. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        public async Task KickId(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var nam = ctx.RawArguments[0];

            await msg.DeleteAsync();

            var uss = await gld.GetUserAsync(ulong.Parse(nam));
            await uss.KickAsync();
            await chn.SendMessageAsync(string.Format("**ADA**: Kicked user '{0}'", uss.Username));
        }

        [AdaCommand("ban", "Bans a user by name. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.BanMembers)]
        public async Task Ban(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var nam = ctx.RawArguments[0];

            await msg.DeleteAsync();

            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var uss = gls.Users.Where(xus => xus.Username == nam);
            if (uss.Count() > 1)
            {
                await chn.SendMessageAsync(string.Format("**ADA**: Query for user '{0}' returned more than one user, try to be more specific.", nam));
                return;
            }

            var usb = uss.First();
            await gld.AddBanAsync(usb);
            await chn.SendMessageAsync(string.Format("**ADA**: Banned user '{0}'", usb.Username));
        }

        [AdaCommand("banid", "Bans a user by id. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.BanMembers)]
        public async Task BanId(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var nam = ctx.RawArguments[0];

            await msg.DeleteAsync();

            var uss = await gld.GetUserAsync(ulong.Parse(nam));
            await gld.AddBanAsync(uss);
            await chn.SendMessageAsync(string.Format("**ADA**: Banned user '{0}'", uss.Username));
        }

        [AdaCommand("prune", "Prunes inactive users. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        public async Task Prune(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var nam = ctx.RawArguments[0];

            await msg.DeleteAsync();

            var usp = await gld.PruneUsersAsync();
            await chn.SendMessageAsync(string.Format("**ADA**: Pruned {0:#,##0} users", usp));
        }

        [AdaCommand("userinfo", "Displays information about users matching given name. This command can only be used by server administrators.", Aliases = "uinfo;userlist;ulist;userfind;ufind", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public async Task UserInfo(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var nam = ctx.RawArguments[0].ToLower();

            await msg.DeleteAsync();

            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var usf = gls.Users;
            var uss = new List<SocketGuildUser>();
            foreach (var xus in usf)
            {
                var xusn = xus.Username;
                var xusm = xus.Nickname;

                if ((!string.IsNullOrWhiteSpace(xusn) && xusn.ToLower().Contains(nam)) || (!string.IsNullOrWhiteSpace(xusm) && xusm.ToLower().Contains(nam)))
                    uss.Add(xus);
            }

            var msgs = new List<string>();
            var sb = new StringBuilder();
            var msb = new StringBuilder();
            sb.AppendFormat("**ADA**: found {0:#,##0} users matching this query ('{1}'):", uss.Count(), nam).AppendLine().AppendLine();
            foreach (var xus in uss)
            {
                msb = new StringBuilder();
                msb.AppendFormat("**User**: {0}#{1}", xus.Username, xus.Discriminator).AppendLine();
                msb.AppendFormat("**ID**: {0}", xus.Id).AppendLine();
                msb.AppendFormat("**Nickname**: {0}", xus.Nickname).AppendLine();
                msb.AppendFormat("**Roles**: {0}", string.Join(", ", xus.RoleIds.Select(xid => gls.GetRole(xid)))).AppendLine();
                if (xus.JoinedAt != null)
                    msb.AppendFormat("**Joined**: {0:yyyy-MM-dd HH:mm:ss} UTC", xus.JoinedAt.Value.UtcDateTime).AppendLine();
                msb.AppendFormat("**Avatar URL**: {0}", xus.AvatarUrl).AppendLine();
                msb.AppendLine("------");
                if (msb.Length + sb.Length >= 2000)
                {
                    msgs.Add(sb.ToString());
                    sb = new StringBuilder();
                    sb.Append(msb.ToString());
                }
                else
                {
                    sb.Append(msb.ToString());
                }
            }
            msgs.Add(sb.ToString());

            foreach (var xmsg in msgs)
            {
                await chn.SendMessageAsync(xmsg);
            }
        }

        [AdaCommand("guildinfo", "Displays information about current guild. This command can only be used by server administrators.", Aliases = "ginfo", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageGuild)]
        public async Task ServerInfo(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var gls = gld as SocketGuild;
            await gls.DownloadUsersAsync();
            var sb = new StringBuilder();
            sb.AppendLine("**ADA**: guild info").AppendLine();
            sb.AppendFormat("**Name**: '{0}'", gls.Name).AppendLine();
            sb.AppendFormat("**ID**: {0}", gls.Id).AppendLine();
            sb.AppendFormat("**Voice Region ID**: {0}", gls.VoiceRegionId).AppendLine();
            sb.AppendFormat("**Owner**: {0} ({1})", (await gls.GetOwnerAsync()).Mention, gls.OwnerId).AppendLine();
            sb.AppendFormat("**Channel count**: {0:#,##0}", gls.Channels.Count).AppendLine();
            sb.AppendFormat("**Role count**: {0:#,##0}", gls.Roles.Count).AppendLine();
            sb.AppendFormat("**Member count**: {0:#,##0}", gls.Users.Count).AppendLine();
            sb.AppendFormat("**Default channel**: {0}", (await gls.GetDefaultChannelAsync()).Mention).AppendLine();
            sb.AppendFormat("**Features**: {0}", string.Join(", ", gls.Features.Select(xs => string.Concat("'", xs, "'")))).AppendLine();
            sb.AppendFormat("**Icon URL**: {0}", gls.IconUrl).AppendLine();
            await chn.SendMessageAsync(sb.ToString());
        }

        [AdaCommand("purgechannel", "Purges a channel. Removes up to 100 messages. This command can only be used by server administrators.", Aliases = "purgech;chpurge;chanpurge;purgechan", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public async Task PurgeChannel(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var nam = ctx.RawArguments[0];

            await msg.DeleteAsync();

            var gls = gld as SocketGuild;
            var chp = gls.Channels.FirstOrDefault(xch => xch.Name == nam) as SocketTextChannel;
            var msgs = await chp.GetMessagesAsync(100).Flatten();
            await chp.DeleteMessagesAsync(msgs);

            await chn.SendMessageAsync(string.Format("**ADA**: Deleted {0:#,##0} messages", msgs.Count()));
        }

        [AdaCommand("adahelp", "Shows command list. Add command name to learn more.", CheckPermissions = false)]
        public async Task Help(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var embed = (EmbedBuilder)null;
            if (ctx.RawArguments.Count == 0)
            {
                embed = this.PrepareEmbed("ADA Help", string.Format("List of all ADA commands, with aliases, and descriptions. Run {0}adahelp command to learn more about a specific command.", AdaBotCore.CommandManager.Prefix));
                foreach (var cmdg in AdaBotCore.CommandManager.GetCommands().GroupBy(xcmd => xcmd.Module))
                {
                    var sb = new StringBuilder();
                    foreach (var cmd in cmdg.OrderBy(xcmd => xcmd.Name))
                    {
                        var str0 = (string)null;
                        if (cmd.Checker != null && !cmd.Checker.CanRun(cmd, usr, msg, chn, gld, out str0))
                            continue;

                        sb.AppendFormat("{1}**{0}**", cmd.Name, AdaBotCore.CommandManager.Prefix).AppendLine();
                        //sb.AppendFormat("Aliases: {0}", string.Join(", ", cmd.Aliases)).AppendLine();
                        //sb.AppendLine(cmd.Description);
                        //sb.AppendLine();
                    }
                    sb.AppendLine();

                    embed.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = string.Format("Commands registered by {0}", cmdg.Key.Name);
                        x.Value = sb.ToString();
                    });
                }
            }
            else
            {
                var cmdn = ctx.RawArguments[0];
                var cmd = AdaBotCore.CommandManager.GetCommand(cmdn);
                if (cmd == null)
                    throw new InvalidOperationException(string.Format("Command '{0}' does not exist", cmdn));

                embed = this.PrepareEmbed("ADA Help", string.Format("\"{0}{1}\" Command help", AdaBotCore.CommandManager.Prefix, cmd.Name));
                
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Description";
                    x.Value = cmd.Description;
                });

                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Aliases";
                    x.Value = string.Join(", ", cmd.Aliases.Select(xa => string.Concat(AdaBotCore.CommandManager.Prefix, xa)));
                });
            }

            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("aboutada", "Shows information about ADA.", CheckPermissions = false)]
        public async Task About(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            var a = Assembly.GetExecutingAssembly();
            var n = a.GetName();

            var gls = gld as SocketGuild;
            var sb = new StringBuilder();
            sb.AppendLine("Hi! I am ADA, or Advanced (although Automatic is also applicable) Discord Administrator. A bot created by Emzi0767 to simplify several administrative tasks for discord servers.");
            sb.AppendLine();
            sb.AppendLine("I first went live on 2016-11-17, and this is what I look like IRL: http://i.imgur.com/Nykuwgj.jpg");
            sb.AppendLine();
            sb.AppendLine("You can see the list of currently available commands by typing /adahelp. Note that some of these commands might not be available to you, depending on this server's policy.");
            if (gls != null)
            {
                sb.AppendLine();
                sb.AppendFormat("I currently run on {0:#,##0} servers. If you want to add me to your server, contact <@181875147148361728>.", gls.Discord.Guilds.Count()).AppendLine();
            }
            sb.AppendLine();
            sb.AppendFormat("Current ADA version is {0}. There are {1:#,##0} plugins loaded, and {2:#,##0} commands registered.", n.Version, AdaBotCore.PluginManager.PluginCount, AdaBotCore.CommandManager.CommandCount).AppendLine();
            sb.AppendLine();
            sb.AppendFormat("The brave individuals who wish to do so, can view and contribute to my source code at https://github.com/Emzi0767/Discord-ADA-Bot");

            await chn.SendMessageAsync(sb.ToString());
        }

        [AdaCommand("fulldump", "Performs a full environment dump. This command can only be used by Emzi0767.", CheckerId = "CoreDebugChecker", CheckPermissions = true)]
        public async Task FullDump(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            // ada assembly data
            var ada_a = Assembly.GetExecutingAssembly();
            var ada_n = ada_a.GetName();
            var ada_l = ada_a.Location;

            // ada process data
            var ada_p = Process.GetCurrentProcess();
            var ada_m = ada_p.Modules;

            // ada appdomain
            var ada_d = AppDomain.CurrentDomain;
            var ada_s = ada_d.GetAssemblies().OrderBy(xa => xa.FullName);

            // dump holders
            var ada_info = new List<string>();
            var ada_sb0 = new StringBuilder();
            var ada_sb1 = new StringBuilder();

            // create the dump
            ada_sb0.AppendLine("**ADA**: full debug dump").AppendLine();

            // dump process info
            ada_sb0.AppendLine("**ADA PROCESS**");
            ada_sb0.AppendFormat("PID: {0}", ada_p.Id).AppendLine();
            ada_sb0.AppendFormat("Name: '{0}'", ada_p.ProcessName).AppendLine();
            ada_sb0.AppendFormat("Is 64-bit: {0}", Environment.Is64BitProcess ? "Yes" : "No").AppendLine();
            ada_sb0.AppendFormat("Command line: {0} {1}", ada_p.StartInfo.FileName, ada_p.StartInfo.Arguments).AppendLine();
            ada_sb0.AppendFormat("Started: {0:yyyy-MM-dd HH:mm:ss} UTC", ada_p.StartTime.ToUniversalTime()).AppendLine();
            ada_sb0.AppendFormat("Thread count: {0:#,##0}", ada_p.Threads.Count).AppendLine();
            ada_sb0.AppendFormat("Total processor time: {0:c}", ada_p.TotalProcessorTime).AppendLine();
            ada_sb0.AppendFormat("User processor time: {0:c}", ada_p.UserProcessorTime).AppendLine();
            ada_sb0.AppendFormat("Privileged processor time: {0:c}", ada_p.PrivilegedProcessorTime).AppendLine();
            ada_sb0.AppendFormat("Handle count: {0:#,##0}", ada_p.HandleCount).AppendLine();
            ada_sb0.AppendFormat("Working set: {0}", ada_p.WorkingSet64.ToSizeString()).AppendLine();
            ada_sb0.AppendFormat("Virtual memory size: {0}", ada_p.VirtualMemorySize64.ToSizeString()).AppendLine();
            ada_sb0.AppendFormat("Paged memory size: {0}", ada_p.PagedMemorySize64.ToSizeString()).AppendLine();
            ada_info.Add(ada_sb0.ToString());
            ada_sb0 = new StringBuilder();

            // dump process module info
            ada_sb0.AppendLine("**ADA PROCESS MODULES**");
            foreach (ProcessModule ada_xm in ada_m)
            {
                ada_sb1 = new StringBuilder();
                ada_sb1.AppendFormat("Name: {0}", ada_xm.ModuleName).AppendLine();
                ada_sb1.AppendFormat("File name: {0}", ada_xm.FileName).AppendLine();
                ada_sb1.AppendFormat("File version: {0}", ada_xm.FileVersionInfo.FileVersion).AppendLine();
                ada_sb1.AppendFormat("Product version: {0}", ada_xm.FileVersionInfo.ProductVersion).AppendLine();
                ada_sb1.AppendFormat("Product name: {0}", ada_xm.FileVersionInfo.ProductName).AppendLine();
                ada_sb1.AppendFormat("Base address: {0}", ada_xm.BaseAddress.ToPointerString()).AppendLine();
                ada_sb1.AppendFormat("Entry point address: {0}", ada_xm.EntryPointAddress.ToPointerString()).AppendLine();
                ada_sb1.AppendFormat("Memory size: {0}", ada_xm.ModuleMemorySize.ToSizeString()).AppendLine();
                ada_sb1.AppendLine("---");

                if (ada_sb0.Length + ada_sb1.Length >= 2000)
                {
                    ada_info.Add(ada_sb0.ToString());
                    ada_sb0 = new StringBuilder();
                }
                ada_sb0.Append(ada_sb1.ToString());
            }
            ada_info.Add(ada_sb0.ToString());
            ada_sb0 = new StringBuilder();

            // dump assembly info
            ada_sb0.AppendLine("**ADA ASSEMBLY INFO**");
            ada_sb0.AppendFormat("Name: {0}", ada_n.FullName).AppendLine();
            ada_sb0.AppendFormat("Version: {0}", ada_n.Version).AppendLine();
            ada_sb0.AppendFormat("Location: {0}", ada_l).AppendLine();
            ada_sb0.AppendFormat("Code base: {0}", ada_a.CodeBase).AppendLine();
            ada_sb0.AppendFormat("Entry point: {0}.{1}", ada_a.EntryPoint.DeclaringType, ada_a.EntryPoint.Name).AppendLine();
            ada_info.Add(ada_sb0.ToString());
            ada_sb0 = new StringBuilder();

            // dump environment info
            ada_sb0.AppendLine("**ADA OS/.NET INFO**");
            ada_sb0.AppendFormat("OS platform: {0}", Environment.OSVersion.Platform.ToString()).AppendLine();
            ada_sb0.AppendFormat("OS version: {0} ({1}); Service Pack: {2}", Environment.OSVersion.Version, Environment.OSVersion.VersionString, Environment.OSVersion.ServicePack).AppendLine();
            ada_sb0.AppendFormat("OS is 64-bit: {0}", Environment.Is64BitOperatingSystem ? "Yes" : "No").AppendLine();
            ada_sb0.AppendFormat(".NET environment version: {0}", Environment.Version).AppendLine();
            ada_sb0.AppendFormat(".NET is Mono: {0}", Type.GetType("Mono.Runtime") != null ? "Yes" : "No");
            ada_info.Add(ada_sb0.ToString());
            ada_sb0 = new StringBuilder();

            // dump appdomain info
            ada_sb0.AppendLine("**ADA APPDOMAIN INFO**");
            ada_sb0.AppendFormat("Name: {0}", ada_d.FriendlyName).AppendLine();
            ada_sb0.AppendFormat("Base directory: {0}", ada_d.BaseDirectory).AppendLine();
            ada_info.Add(ada_sb0.ToString());
            ada_sb0 = new StringBuilder();

            // dump appdomain assembly info
            ada_sb0.AppendLine("**ADA APPDOMAIN ASSEMBLY INFO**");
            foreach (var ada_xa in ada_s)
            {
                ada_sb1 = new StringBuilder();
                ada_sb1.AppendFormat("Name: {0}", ada_xa.FullName).AppendLine();
                ada_sb1.AppendFormat("Version: {0}", ada_xa.GetName().Version).AppendLine();
                if (!ada_xa.IsDynamic)
                {
                    ada_sb1.AppendFormat("Location: {0}", ada_xa.Location).AppendLine();
                    ada_sb1.AppendFormat("Code base: {0}", ada_xa.CodeBase).AppendLine();
                }
                if (ada_xa.EntryPoint != null)
                    ada_sb1.AppendFormat("Entry point: {0}.{1}", ada_xa.EntryPoint.DeclaringType, ada_xa.EntryPoint.Name).AppendLine();
                ada_sb1.AppendLine("---");

                if (ada_sb0.Length + ada_sb1.Length >= 2000)
                {
                    ada_info.Add(ada_sb0.ToString());
                    ada_sb0 = new StringBuilder();
                }
                ada_sb0.Append(ada_sb1.ToString());
            }
            ada_info.Add(ada_sb0.ToString());
            ada_sb0 = null;
            ada_sb1 = null;

            foreach (var ada_data in ada_info)
                await chn.SendMessageAsync(ada_data);
        }

        private EmbedBuilder PrepareEmbed(string title, string desc)
        {
            var embed = new EmbedBuilder();
            embed.Title = title;
            embed.Description = desc;
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.IconUrl = AdaBotCore.AdaClient.DiscordClient.CurrentUser.AvatarUrl;
            embed.Author.Name = "ADA, a bot by Emzi0767";
            embed.Timestamp = DateTimeOffset.UtcNow;
            embed.Color = new Color(127, 255, 0);
            return embed;
        }
    }
}
