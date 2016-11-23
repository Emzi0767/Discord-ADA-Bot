using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Extensions;

namespace Emzi0767.Net.Discord.AdaBot.Core
{
    [CommandHandler]
    public class AdaCommands
    {
        [Command("mkgroup", "Creates a new role with specified name. This command can only be used by server administrators.", Aliases = "makegroup;creategroup;mkrole;makerole;createrole;gmk;gmake;gcreate", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public static async Task CreateGroup(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;

            await msg.Delete();

            var nam = ea.Args[0];
            await srv.CreateRole(nam, new ServerPermissions(0x0635CC01u), null, false, false);
            await chn.SendMessage(string.Format("**ADA**: Created new role named '{0}'.", nam));
        }

        [Command("rmgroup", "Removes a role with specified name. This command can only be used by server administrators.", Aliases = "removegroup;deletegroup;delgroup;rmrole;removerole;deleterole;delrole;gdel;gdelete;grm;gremove", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public static async Task DeleteGroup(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;

            await msg.Delete();

            var nam = ea.Args[0];
            var grp = srv.Roles.FirstOrDefault(xr => xr.Name == nam);

            if (grp == null)
                return;

            await grp.Delete();
            await chn.SendMessage(string.Format("**ADA**: Deleted role named '{0}'.", nam));
        }

        [Command("modgroup", "Edits a role with specified name. This command can only be used by server administrators.", Aliases = "modifygroup;editgroup;modrole;modifyrole;editrole;gmod;gmodify;gedit", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public static async Task ModifyGroup(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;

            await msg.Delete();

            var raw = ea.Args[0];
            var par = raw.Split(';')
                .Select(xrs => xrs.Split('='))
                .ToDictionary(xrs => xrs[0], xrs => xrs[1]);

            var gpr = par.ContainsKey("permissions") ? new ServerPermissions(uint.Parse(par["permissions"])) : (ServerPermissions?)null;
            var gcl = par.ContainsKey("color") ? new Color(Convert.ToUInt32(par["color"], 16)) : null;
            var ghs = par.ContainsKey("hoist") ? par["hoist"] == "true" : (bool?)null;
            var gps = par.ContainsKey("position") ? int.Parse(par["position"]) : (int?)null;
            var gmt = par.ContainsKey("mention") ? par["mention"] == "true" : (bool?)null;

            var gnm = par.ContainsKey("name") ? par["name"] : null;
            if (gnm == null)
                return;

            var grp = srv.Roles.FirstOrDefault(xr => xr.Name == gnm);
            if (grp == null)
                return;

            await grp.Edit(gnm, gpr, gcl, ghs, gps, gmt);
            await chn.SendMessage(string.Format("**ADA**: Edited role named '{0}'.", gnm));
        }

        [Command("dumpgroup", "Dumps all properties of a role. This command can only be used by server administrators.", Aliases = "printgroup;dumprole;printrole;gdump", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public static async Task DumpGroup(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;

            await msg.Delete();

            var nam = ea.Args[0];
            var grp = srv.Roles.FirstOrDefault(xr => xr.Name == nam);

            if (grp == null)
                return;
            
            var sb = new StringBuilder();
            sb.AppendLine("**ADA**: Dumping all properties of a role");
            sb.AppendFormat("**Name**: {0}", grp.Name).AppendLine();
            sb.AppendFormat("**ID**: {0}", grp.Id).AppendLine();
            sb.AppendFormat("**Color**: {0:X6}", grp.Color.RawValue).AppendLine();
            sb.AppendFormat("**Is everyone**: {0}", grp.IsEveryone ? "Yes" : "No").AppendLine();
            sb.AppendFormat("**Is hoisted**: {0}", grp.IsHoisted ? "Yes" : "No").AppendLine();
            sb.AppendFormat("**Is mentionable**: {0}", grp.IsMentionable ? "Yes" : "No").AppendLine();
            if (grp.IsMentionable)
                sb.AppendFormat("**Mention**: {0}", grp.Mention).AppendLine();
            sb.AppendFormat("**Position**: {0}", grp.Position).AppendLine();
            sb.AppendFormat("**Total members**: {0:#,##0}", grp.Members.Count()).AppendLine();
            sb.AppendLine("**Permissions**:");
            sb.AppendFormat("**Raw value**: {0}", grp.Permissions.RawValue).AppendLine();
            if (grp.Permissions.Administrator)
                sb.AppendLine("+ Administrator");
            if (grp.Permissions.AttachFiles)
                sb.AppendLine("+ Can attach files");
            if (grp.Permissions.BanMembers)
                sb.AppendLine("+ Can ban members");
            if (grp.Permissions.ChangeNickname)
                sb.AppendLine("+ Can change nickname");
            if (grp.Permissions.Connect)
                sb.AppendLine("+ Can use voice chat");
            if (grp.Permissions.CreateInstantInvite)
                sb.AppendLine("+ Can create instant invites");
            if (grp.Permissions.DeafenMembers)
                sb.AppendLine("+ Can deafen members");
            if (grp.Permissions.EmbedLinks)
                sb.AppendLine("+ Can embed links");
            if (grp.Permissions.KickMembers)
                sb.AppendLine("+ Can kick members");
            if (grp.Permissions.ManageChannels)
                sb.AppendLine("+ Can manage channels");
            if (grp.Permissions.ManageMessages)
                sb.AppendLine("+ Can manage messages");
            if (grp.Permissions.ManageNicknames)
                sb.AppendLine("+ Can manage nicknames");
            if (grp.Permissions.ManageRoles)
                sb.AppendLine("+ Can manage roles");
            if (grp.Permissions.ManageServer)
                sb.AppendLine("+ Can manage server");
            if (grp.Permissions.MentionEveryone)
                sb.AppendLine("+ Can mention everyone group");
            if (grp.Permissions.MoveMembers)
                sb.AppendLine("+ Can move members between voice channels");
            if (grp.Permissions.MuteMembers)
                sb.AppendLine("+ Can mute members");
            if (grp.Permissions.ReadMessageHistory)
                sb.AppendLine("+ Can read message history");
            if (grp.Permissions.ReadMessages)
                sb.AppendLine("+ Can read messages");
            if (grp.Permissions.SendMessages)
                sb.AppendLine("+ Can send messages");
            if (grp.Permissions.SendTTSMessages)
                sb.AppendLine("+ Can send TTS messages");
            if (grp.Permissions.Speak)
                sb.AppendLine("+ Can speak");
            if (grp.Permissions.UseVoiceActivation)
                sb.AppendLine("+ Can use voice activation");

            await chn.SendMessage(sb.ToString());
        }

        [Command("listgroups", "Lists all roles on the server. This command can only be used by server administrators.", Aliases = "lsgroups;listroles;lsroles;glist;gls", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public static async Task ListGroups(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;

            await msg.Delete();

            var nam = ea.Args[0];
            var grp = srv.Roles;

            if (grp == null)
                return;

            var sb = new StringBuilder();
            sb.AppendFormat("**ADA**: All groups ({0:#,##0}):", grp.Count()).AppendLine();
            foreach (var xgrp in grp)
            {
                sb.AppendLine(xgrp.IsMentionable ? xgrp.Mention : xgrp.Name);
            }

            await chn.SendMessage(sb.ToString());
        }

        [Command("groupadd", "Adds a user to a role. This command can only be used by server administrators.", Aliases = "roleadd;ugadd", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public static async Task GroupAdd(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;

            await msg.Delete();

            var raw = ea.Args[0];
            var par = raw.Split(';')
                .Select(xrs => xrs.Split('='))
                .ToDictionary(xrs => xrs[0], xrs => xrs[1]);

            var usn = par.ContainsKey("user") ? par["user"] : null;
            var grn = par.ContainsKey("role") ? par["role"] : null;

            var usg = srv.FindUsers(usn);
            var gru = srv.Roles.FirstOrDefault(xr => xr.Name == grn);
            if (usg.Count() != 1 || gru == null)
                return;

            await usg.FirstOrDefault().AddRoles(gru);
            await chn.SendMessage(string.Format("**ADA**: Added {0} to {1}", usg.FirstOrDefault().Name, gru.Name));
        }

        [Command("groupremove", "Removes user from a role. This command can only be used by server administrators.", Aliases = "roleremove;ugremove;ugrm", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageRoles)]
        public static async Task GroupRemove(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;

            await msg.Delete();

            var raw = ea.Args[0];
            var par = raw.Split(';')
                .Select(xrs => xrs.Split('='))
                .ToDictionary(xrs => xrs[0], xrs => xrs[1]);

            var usn = par.ContainsKey("user") ? par["user"] : null;
            var grn = par.ContainsKey("role") ? par["role"] : null;

            var usg = srv.FindUsers(usn);
            var gru = srv.Roles.FirstOrDefault(xr => xr.Name == grn);
            if (usg.Count() != 1 || gru == null)
                return;

            await usg.FirstOrDefault().RemoveRoles(gru);
            await chn.SendMessage(string.Format("**ADA**: Removed {0} from {1}", usg.FirstOrDefault().Name, gru.Name));
        }

        [Command("kick", "Kicks a user by name. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        public static async Task Kick(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;
            var nam = ea.Args[0];

            await msg.Delete();

            var uss = srv.FindUsers(nam);
            if (uss.Count() > 1)
            {
                await chn.SendMessage(string.Format("**ADA**: Query for user '{0}' returned more than one user, try to be more specific.", nam));
                return;
            }

            var usk = uss.First();
            await usk.Kick();
            await chn.SendMessage(string.Format("**ADA**: Kicked user '{0}'", usk.Name));
        }

        [Command("kickid", "Kicks a user by id. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        public static async Task KickId(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;
            var nam = ea.Args[0];

            await msg.Delete();

            var uss = srv.GetUser(ulong.Parse(nam));
            await uss.Kick();
            await chn.SendMessage(string.Format("**ADA**: Kicked user '{0}'", uss.Name));
        }

        [Command("ban", "Bans a user by name. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.BanMembers)]
        public static async Task Ban(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;
            var nam = ea.Args[0];

            await msg.Delete();

            var uss = srv.FindUsers(nam);
            if (uss.Count() > 1)
            {
                await chn.SendMessage(string.Format("**ADA**: Query for user '{0}' returned more than one user, try to be more specific.", nam));
                return;
            }

            var usb = uss.First();
            await srv.Ban(usb);
            await chn.SendMessage(string.Format("**ADA**: Banned user '{0}'", usb.Name));
        }

        [Command("banid", "Bans a user by id. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.BanMembers)]
        public static async Task BanId(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;
            var nam = ea.Args[0];

            await msg.Delete();

            var uss = srv.GetUser(ulong.Parse(nam));
            await srv.Ban(uss);
            await chn.SendMessage(string.Format("**ADA**: Banned user '{0}'", uss.Name));
        }

        [Command("prune", "Prunes inactive users. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.KickMembers)]
        public static async Task Prune(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;
            var nam = ea.Args[0];

            await msg.Delete();

            var usp = await srv.PruneUsers();
            await chn.SendMessage(string.Format("**ADA**: Pruned {0:#,##0} users", usp));
        }

        [Command("userinfo", "Displays information about users matching given name. This command can only be used by server administrators.", Aliases = "uinfo;userlist;ulist;userfind;ufind", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public static async Task UserInfo(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;
            var nam = ea.Args[0].ToLower();

            await msg.Delete();

            var usf = srv.Users;
            var uss = new List<User>();
            foreach (var xus in usf)
            {
                var xusn = xus.Name;
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
                msb.AppendFormat("**User**: {0}#{1:0000}", xus.Name, xus.Discriminator).AppendLine();
                msb.AppendFormat("**ID**: {0}", xus.Id).AppendLine();
                msb.AppendFormat("**Nickname**: {0}", xus.Nickname).AppendLine();
                msb.AppendFormat("**Roles**: {0}", string.Join(", ", xus.Roles)).AppendLine();
                msb.AppendFormat("**Joined**: {0:yyyy-MM-dd HH:mm:ss} UTC", xus.JoinedAt.ToUniversalTime()).AppendLine();
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
                await chn.SendMessage(xmsg);
            }
        }

        [Command("serverinfo", "Displays information about current server. This command can only be used by server administrators.", Aliases = "sinfo;guildinfo;ginfo", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageGuild)]
        public static async Task ServerInfo(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var sb = new StringBuilder();
            sb.AppendLine("**ADA**: guild info").AppendLine();
            sb.AppendFormat("**Name**: '{0}'", srv.Name).AppendLine();
            sb.AppendFormat("**ID**: {0}", srv.Id).AppendLine();
            sb.AppendFormat("**Region**: {0}", srv.Region.Name).AppendLine();
            sb.AppendFormat("**Owner**: {0} ({1})", srv.Owner.Mention, srv.Owner.Id).AppendLine();
            sb.AppendFormat("**Channel count**: {0:#,##0}", srv.ChannelCount).AppendLine();
            sb.AppendFormat("**Role count**: {0:#,##0}", srv.RoleCount).AppendLine();
            sb.AppendFormat("**Member count**: {0:#,##0}", srv.UserCount).AppendLine();
            sb.AppendFormat("**Default channel**: {0}", srv.DefaultChannel.Mention).AppendLine();
            sb.AppendFormat("**Features**: {0}", string.Join(", ", srv.Features.Select(xs => string.Concat("'", xs, "'")))).AppendLine();
            sb.AppendFormat("**Icon URL**: {0}", srv.IconUrl).AppendLine();
            await chn.SendMessage(sb.ToString());
        }

        [Command("purgechannel", "Purges a channel. Removes up to 100 messages. This command can only be used by server administrators.", Aliases = "purgech;chpurge;chanpurge;purgechan", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.ManageMessages)]
        public static async Task PurgeChannel(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var prm = usr.ServerPermissions;
            var nam = ea.Args[0];

            await msg.Delete();

            var chp = srv.AllChannels.FirstOrDefault(xch => xch.Name == nam);
            var msgs = await chp.DownloadMessages(100);
            foreach (var xmsg in msgs)
                await xmsg.Delete();

            await chn.SendMessage(string.Format("**ADA**: Deleted {0:#,##0} messages", msgs.Length));
        }

        [Command("adahelp", "Shows command list.", CheckPermissions = false)]
        public static async Task Help(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;
            var nam = ea.Args[0];

            await msg.Delete();

            var a = Assembly.GetExecutingAssembly();
            var n = a.GetName();

            var msgs = new List<string>();
            var sb = new StringBuilder();
            var msb = new StringBuilder();
            sb.AppendLine("**ADA**: Help");
            sb.AppendFormat("ADA Version: {0}", n.Version).AppendLine();
            sb.AppendFormat("Created by Emzi0767 (<@181875147148361728>)").AppendLine();
            sb.AppendLine();
            foreach (var cmdg in AdaBotCore.Handler.GetCommands().GroupBy(xcmd => xcmd.Handler))
            {
                var h = string.Format("Commands registered by **{0}**:", cmdg.Key.ToString());
                if (sb.Length + h.Length + Environment.NewLine.Length >= 2000)
                {
                    msgs.Add(sb.ToString());
                    sb = new StringBuilder();
                }
                sb.AppendLine(h).AppendLine();
                foreach (var cmd in cmdg.OrderBy(xcmd => xcmd.Name))
                {
                    var str0 = (string)null;
                    if (cmd.Checker != null && !cmd.Checker.CanRun(cmd.Command, usr, chn, out str0))
                        continue;

                    msb = new StringBuilder();
                    msb.AppendFormat("/**{0}**", cmd.Name).AppendLine();
                    msb.AppendFormat("Aliases: {0}", string.Join(", ", cmd.Aliases)).AppendLine();
                    msb.AppendLine(cmd.Description);
                    msb.AppendLine();
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
                if (sb.Length + 6 + Environment.NewLine.Length >= 2000)
                {
                    msgs.Add(sb.ToString());
                    sb = new StringBuilder();
                }
                sb.AppendLine("------").AppendLine();
            }
            msgs.Add(sb.ToString());

            foreach (var xmsg in msgs)
            {
                await chn.SendMessage(xmsg);
            }
        }

        [Command("aboutada", "Shows information about ADA.", CheckPermissions = false)]
        public static async Task About(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var a = Assembly.GetExecutingAssembly();
            var n = a.GetName();

            var sb = new StringBuilder();
            sb.AppendLine("Hi! I am ADA, or Advanced (although Automatic is also applicable) Discord Administrator. A bot created by Emzi0767 to simplify several administrative tasks for discord servers.");
            sb.AppendLine();
            sb.AppendLine("I first went live on 2016-11-17, and this is what I look like IRL: http://i.imgur.com/Nykuwgj.jpg");
            sb.AppendLine();
            sb.AppendLine("You can see the list of currently available commands by typing /adahelp. Note that some of these commands might not be available to you, depending on this server's policy.");
            sb.AppendLine();
            sb.AppendFormat("I currently run on {0:#,##0} servers. If you want to add me to your server, contact <@181875147148361728>.", srv.Client.Servers.Count()).AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Current ADA version is {0}. There are {1:#,##0} plugins loaded, and {2:#,##0} commands registered.", n.Version, AdaBotCore.PluginManager.PluginCount, AdaBotCore.Handler.CommandCount).AppendLine();
            sb.AppendLine();
            sb.AppendFormat("The brave individuals who wish to do so, can view and contribute to my source code at https://github.com/Emzi0767/Discord-ADA-Bot");

            await chn.SendMessage(sb.ToString());
        }

        [Command("fulldump", "Performs a full environment dump. This command can only be used by Emzi0767.", CheckerId = "CoreDebugChecker", CheckPermissions = true)]
        public static async Task FullDump(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

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
                //ada_sb1.AppendFormat("File name: {0}", ada_xm.FileName).AppendLine();
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
                await chn.SendMessage(ada_data);
        }
    }
}
