using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Core;
using Markov;
using d = System.Drawing;
using s = Discord;

namespace Emzi0767.Net.Discord.Ada.AdvancedCommands
{
    [CommandHandler]
    public class AdvancedCommandsHandler
    {
        [Command("colorme", "Sets your own color. This command can be disabled by server administrators.", CheckerId = "ACPChecker", CheckPermissions = true)]
        public static async Task SetColor(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var clr = Convert.ToUInt32(ea.Args[0], 16);
            var url = usr.Roles.FirstOrDefault(xr => xr.Name == usr.Id.ToString());
            if (url == null)
            {
                url = await srv.CreateRole(usr.Id.ToString(), srv.EveryoneRole.Permissions, null, false, false);
                await usr.AddRoles(url);
            }
            await url.Edit(null, null, new s.Color(clr), null, null, null);

            await chn.SendMessage(string.Format("**ADA**: {0}'s color is now {1:X6}.", usr.Mention, clr));
        }

        [Command("generateid", "Generates a random ID. This command can be disabled by server administrators.", Aliases = "genid;makeid;mkid", CheckerId = "ACPChecker", CheckPermissions = true)]
        public static async Task CreateId(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var rng = new Random();
            var n3 = rng.Next();
            var n2 = BitConverter.GetBytes(n3);
            var n1 = BitConverter.ToString(n2).Replace("-", "").ToLower().Insert(4, "-");

            await chn.SendMessage(string.Format("{0}: the ID you requested is {1}", usr.Mention, n1));
        }

        [Command("generateuuid", "Generates a random UUID. This command can be disabled by server administrators.", Aliases = "genuuid;makeuuid;mkuuid;generateguid;genguid;makeguid;mkguid", CheckerId = "ACPChecker", CheckPermissions = true)]
        public static async Task CreateUuid(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();
            
            var n1 = Guid.NewGuid().ToString();

            await chn.SendMessage(string.Format("{0}: the UUID you requested is {1}", usr.Mention, n1));
        }

        [Command("asciitobase64", "Converts an ASCII string to a Base64 string. This command can be disabled by server administrators.", Aliases = "ascii2base64;ascii2b64;2b64;b64;base64;tobase64;2base64", CheckerId = "ACPChecker", CheckPermissions = true)]
        public static async Task AsciiToBase64(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var utf8 = new UTF8Encoding(false);
            var dat = utf8.GetBytes(ea.Args[0]);
            var b64 = Convert.ToBase64String(dat);

            await chn.SendMessage(string.Format("{0}: Base64 representation of your message is {1}", usr.Mention, b64));
        }

        [Command("base64toascii", "Converts a Base64 string to an ASCII string. This command can be disabled by server administrators.", Aliases = "base642ascii;b642ascii;2ascii;ascii;toascii", CheckerId = "ACPChecker", CheckPermissions = true)]
        public static async Task Base64ToAscii(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var utf8 = new UTF8Encoding(false);
            var dat = Convert.FromBase64String(ea.Args[0]);
            var ascii = utf8.GetString(dat);

            await chn.SendMessage(string.Format("{0}: ASCII representation of your message is {1}", usr.Mention, ascii));
        }

        [Command("color", "Creates a colored square, used for color previewing. This command can be disabled by server administrators.", Aliases = "clr;colour;colorsquare;coloursquare;clrsq", CheckerId = "ACPChecker", CheckPermissions = true)]
        public static async Task ColorSquare(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            using (var ms = new MemoryStream())
            using (var bmp = new Bitmap(64, 64, PixelFormat.Format32bppPArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighSpeed;

                var cli = Convert.ToInt32(ea.Args[0], 16);
                var clr = d.Color.FromArgb(cli);
                var csb = new SolidBrush(clr);

                g.Clear(d.Color.Transparent);
                g.FillRectangle(csb, new Rectangle(Point.Empty, bmp.Size));
                g.Flush();

                bmp.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                await chn.SendFile("color_square.png", ms);
            }
        }

        [Command("markov", "Creates a markov chain sentence out of messages from specified source. This command can be disabled by server administrators.", CheckerId = "ACPChecker", CheckPermissions = true)]
        public static async Task Markov(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var chain = new MarkovChain<string>(1);
            var rnd = new Random();
            //var mnt = msg.MentionedUsers
            //    .Cast<IMentionable>()
            //    .Concat(msg.MentionedRoles.Cast<IMentionable>())
            //    .Concat(msg.MentionedChannels.Cast<IMentionable>())
            //    .FirstOrDefault();

            //var mch = mnt as Channel;
            //var mrl = mnt as Role;
            //var mus = mnt as User;
            var mnt = (string)null;

            if (msg.MentionedUsers.Count() == 0 && msg.MentionedRoles.Count() == 0 && msg.MentionedChannels.Count() == 0)
                throw new ArgumentException("Missing mention.");
            else if (msg.MentionedUsers.Count() > 0)
            {
                var mus = msg.MentionedUsers.First();
                mnt = mus.Mention;
                var chs = msg.MentionedChannels;
                var maxm = 100;
                var lstm = -1;
                //var msgs = new Message[maxm];
                var msgs = new List<Message>(maxm);
                var msgt = (Message[])null;
                while (msgs.Count < maxm && lstm != 0)
                {
                    foreach (var xch in chs)
                    {
                        if ((msgt = await xch.DownloadMessages(Math.Min(100, maxm - msgs.Count), msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : new DateTime(2000, 1, 1, 0, 0, 0)).FirstOrDefault() == null ? null : (ulong?)msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : new DateTime(2000, 1, 1, 0, 0, 0)).FirstOrDefault().Id)).Length > 0)
                        {
                            lstm = Math.Max(msgt.Length, lstm);
                            msgs.AddRange(msgt.Where(xmsg => xmsg.User != null && xmsg.User.Id == mus.Id));
                        }
                    }
                }
                foreach (var xmsg in msgs)
                    chain.Add(xmsg.Text.Split(' '), 1);
            }
            else if (msg.MentionedRoles.Count() > 0)
            {
                var mrl = msg.MentionedRoles.First();
                mnt = mrl.Mention;
                var chs = msg.MentionedChannels;
                var maxm = 100;
                var lstm = -1;
                //var msgs = new Message[maxm];
                var msgs = new List<Message>(maxm);
                var msgt = (Message[])null;
                while (msgs.Count < maxm && lstm != 0)
                {
                    foreach (var xch in chs)
                    {
                        if ((msgt = await xch.DownloadMessages(Math.Min(100, maxm - msgs.Count), msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : new DateTime(2000, 1, 1, 0, 0, 0)).FirstOrDefault() == null ? null : (ulong?)msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : new DateTime(2000, 1, 1, 0, 0, 0)).FirstOrDefault().Id)).Length > 0)
                        {
                            lstm = Math.Max(msgt.Length, lstm);
                            msgs.AddRange(msgt.Where(xmsg => xmsg.User != null && xmsg.User.HasRole(mrl)));
                        }
                    }
                }
                foreach (var xmsg in msgs)
                    chain.Add(xmsg.Text.Split(' '), 1);
            }
            else if (msg.MentionedChannels.Count() > 0)
            {
                var mch = msg.MentionedChannels.First();
                mnt = mch.Mention;
                //var msgs = await mch.DownloadMessages(500);
                var maxm = 500;
                var msgs = new Message[maxm];
                var msgi = 0;
                var msgt = (Message[])null;
                while (msgi < maxm && (msgt = await mch.DownloadMessages(Math.Min(100, maxm - msgi), msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : new DateTime(2000, 1, 1, 0, 0, 0)).FirstOrDefault() == null ? null : (ulong?)msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : new DateTime(2000, 1, 1, 0, 0, 0)).FirstOrDefault().Id)).Length > 0)
                {
                    Array.Copy(msgt, 0, msgs, msgi, msgt.Length);
                    msgi += msgt.Length;
                }
                foreach (var xmsg in msgs)
                    chain.Add(xmsg.Text.Split(' '), 1);
            }

            var sentence = string.Join(" ", chain.Chain(rnd));
            await chn.SendMessage(string.Concat("**ADA**: markov chain of ", mnt, ": ", sentence));
        }

        [Command("enableadvancedcommand", "Enables an Advanced Commands command. This command can only be used by server administrators.", Aliases = "enableac;enableadvcmd", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public static async Task EnableAdvancedCommand(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var cmds = ea.Args[0].Split(',');
            foreach (var cmd in cmds)
            {
                AdvancedCommandsPlugin.SetEnabledState(cmd, srv.Id, true);
            }

            await chn.SendMessage(string.Format("{0}: the following commands are now enabled: {1}", usr.Mention, string.Join(", ", cmds)));
        }

        [Command("disableadvancedcommand", "Disables an Advanced Commands command. This command can only be used by server administrators.", Aliases = "disableac;disableadvcmd", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public static async Task DisableAdvancedCommand(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var cmds = ea.Args[0].Split(',');
            foreach (var cmd in cmds)
            {
                AdvancedCommandsPlugin.SetEnabledState(cmd, srv.Id, false);
            }

            await chn.SendMessage(string.Format("{0}: the following commands are now disabled: {1}", usr.Mention, string.Join(", ", cmds)));
        }
    }
}
