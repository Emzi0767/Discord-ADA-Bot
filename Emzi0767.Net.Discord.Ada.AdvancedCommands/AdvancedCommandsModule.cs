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
using Discord.WebSocket;
using Emzi0767.Net.Discord.AdaBot;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Commands;
using Emzi0767.Net.Discord.AdaBot.Commands.Permissions;
using Markov;
using d = System.Drawing;
using s = Discord;

namespace Emzi0767.Net.Discord.Ada.AdvancedCommands
{
    public class AdvancedCommandsModule : IAdaCommandModule
    {
        public string Name { get { return "ADA Advanced Commands"; } }

        [AdaCommand("colorme", "Sets your own color. This command can be disabled by server administrators.", CheckerId = "ACPChecker", CheckPermissions = true)]
        public async Task SetColor(AdaCommandContext ctx)
        {
            var gld = ctx.Guild as SocketGuild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var clr = Convert.ToUInt32(ctx.RawArguments[0], 16);
            var url = gld.Roles.FirstOrDefault(xr => xr.Name == usr.Id.ToString()) as IRole;
            if (url == null)
            {
                url = await gld.CreateRoleAsync(usr.Id.ToString(), gld.EveryoneRole.Permissions, null, false);
                await usr.AddRolesAsync(url);
            }
            await url.ModifyAsync(x =>
            {
                x.Color = clr;
            });
            
            var embed = this.PrepareEmbed("Success", string.Format(usr.Mention, "'s color is now ", clr.ToString("X6")), EmbedType.Success);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("generateid", "Generates a random ID. This command can be disabled by server administrators.", Aliases = "genid;makeid;mkid", CheckerId = "ACPChecker", CheckPermissions = true)]
        public async Task CreateId(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var rng = new Random();
            var n3 = rng.Next();
            var n2 = BitConverter.GetBytes(n3);
            var n1 = BitConverter.ToString(n2).Replace("-", "").ToLower().Insert(4, "-");
            
            var embed = this.PrepareEmbed("Generated ID", string.Concat(usr.Mention, ", the ID you requested is ", n1), EmbedType.Info);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("generateuuid", "Generates a random UUID. This command can be disabled by server administrators.", Aliases = "genuuid;makeuuid;mkuuid;generateguid;genguid;makeguid;mkguid", CheckerId = "ACPChecker", CheckPermissions = true)]
        public async Task CreateUuid(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();
            
            var n1 = Guid.NewGuid().ToString();
            
            var embed = this.PrepareEmbed("Generated UUID", string.Concat(usr.Mention, ", the UUID you requested is ", n1), EmbedType.Info);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("asciitobase64", "Converts an ASCII string to a Base64 string. This command can be disabled by server administrators.", Aliases = "ascii2base64;ascii2b64;2b64;b64;base64;tobase64;2base64", CheckerId = "ACPChecker", CheckPermissions = true)]
        public async Task AsciiToBase64(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var utf8 = new UTF8Encoding(false);
            var dat = utf8.GetBytes(string.Join(" ", ctx.RawArguments));
            var b64 = Convert.ToBase64String(dat);

            var embed = this.PrepareEmbed("ASCII to Base64", string.Concat(usr.Mention, ": ", b64), EmbedType.Info);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("base64toascii", "Converts a Base64 string to an ASCII string. This command can be disabled by server administrators.", Aliases = "base642ascii;b642ascii;2ascii;ascii;toascii", CheckerId = "ACPChecker", CheckPermissions = true)]
        public async Task Base64ToAscii(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var utf8 = new UTF8Encoding(false);
            var dat = Convert.FromBase64String(string.Join(" ", ctx.RawArguments));
            var ascii = utf8.GetString(dat);

            var embed = this.PrepareEmbed("Base64 to ASCII", string.Concat(usr.Mention, ": ", ascii), EmbedType.Info);
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("color", "Creates a colored square, used for color previewing. This command can be disabled by server administrators.", Aliases = "clr;colour;colorsquare;coloursquare;clrsq", CheckerId = "ACPChecker", CheckPermissions = true)]
        public async Task ColorSquare(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            using (var ms = new MemoryStream())
            using (var bmp = new Bitmap(64, 64, PixelFormat.Format32bppPArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighSpeed;

                var cli = Convert.ToInt32(ctx.RawArguments[0], 16);
                var clr = d.Color.FromArgb(cli);
                var csb = new SolidBrush(clr);

                g.Clear(d.Color.Transparent);
                g.FillRectangle(csb, new Rectangle(Point.Empty, bmp.Size));
                g.Flush();

                bmp.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                await chn.SendFileAsync(ms, "color_square.png", string.Concat(usr.Mention, ", here's a ", cli.ToString("X8"), " square."));
            }
        }

        [AdaCommand("markov", "Creates a markov chain sentence out of messages from specified source. This command can be disabled by server administrators.", CheckerId = "ACPChecker", CheckPermissions = true)]
        public async Task Markov(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var chain = new MarkovChain<string>(1);
            var rnd = new Random();
            var mnt = (string)null;

            var xmu = msg.MentionedUserIds.Select(xid => gld.GetUserAsync(xid).GetAwaiter().GetResult());
            var xmr = msg.MentionedRoleIds.Select(xid => gld.GetRole(xid));
            var xmc = msg.MentionedChannelIds.Select(xid => gld.GetChannelAsync(xid).GetAwaiter().GetResult());

            if (xmu.Count() == 0 && xmr.Count() == 0 && xmc.Count() == 0)
                throw new ArgumentException("Missing mention.");
            else if (xmu.Count() > 0)
            {
                var mus = xmu.First();
                mnt = mus.Mention;
                var chs = xmc;
                var maxm = 100;
                var lstm = -1;
                var msgs = new List<IMessage>(maxm);
                var msgt = (IEnumerable<IMessage>)null;
                while (msgs.Count < maxm && lstm != 0)
                {
                    foreach (var xch in chs)
                    {
                        var xcn = xch as SocketTextChannel;
                        if (msgs.Count == 0)
                        {
                            msgt = await xcn.GetMessagesAsync(100).Flatten();
                            msgs.AddRange(msgt.Where(xmsg => xmsg.Author != null && xmsg.Author.Id == mus.Id));
                        }
                        if ((await xcn.GetMessagesAsync(msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : DateTimeOffset.MinValue).FirstOrDefault(), Direction.Before, Math.Min(100, maxm - msgs.Count)).Flatten()).Count() > 0)
                        {
                            lstm = Math.Max(msgt.Count(), lstm);
                            msgs.AddRange(msgt.Where(xmsg => xmsg.Author != null && xmsg.Author.Id == mus.Id));
                        }
                    }
                }
                foreach (var xmsg in msgs)
                    chain.Add(xmsg.Content.Split(' '), 1);
            }
            else if (xmr.Count() > 0)
            {
                var mrl = xmr.First();
                mnt = mrl.Mention;
                var chs = xmc;
                var maxm = 100;
                var lstm = -1;
                var msgs = new List<IMessage>(maxm);
                var msgt = (IEnumerable<IMessage>)null;
                while (msgs.Count < maxm && lstm != 0)
                {
                    foreach (var xch in chs)
                    {
                        var xcn = xch as SocketTextChannel;
                        if (msgs.Count == 0)
                        {
                            msgt = await xcn.GetMessagesAsync(100).Flatten();
                            msgs.AddRange(msgt.Where(xmsg => xmsg.Author as SocketGuildUser != null && (xmsg.Author as SocketGuildUser).RoleIds.Contains(mrl.Id)));
                        }
                        if ((await xcn.GetMessagesAsync(msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : DateTimeOffset.MinValue).FirstOrDefault(), Direction.Before, Math.Min(100, maxm - msgs.Count)).Flatten()).Count() > 0)
                        {
                            lstm = Math.Max(msgt.Count(), lstm);
                            msgs.AddRange(msgt.Where(xmsg => xmsg.Author as SocketGuildUser != null && (xmsg.Author as SocketGuildUser).RoleIds.Contains(mrl.Id)));
                        }
                    }
                }
                foreach (var xmsg in msgs)
                    chain.Add(xmsg.Content.Split(' '), 1);
            }
            else if (xmc.Count() > 0)
            {
                var mch = xmc.First() as SocketTextChannel;
                mnt = mch.Mention;
                var maxm = 500;
                var msgs = new IMessage[maxm];
                var msgi = 0;
                var msgt = (IEnumerable<IMessage>)null;
                msgt = await mch.GetMessagesAsync(100).Flatten();
                Array.Copy(msgt.ToArray(), 0, msgs, msgi, msgt.Count());
                while (msgi < maxm && (msgt = await mch.GetMessagesAsync(msgs.OrderByDescending(xm => xm != null ? xm.Timestamp : DateTimeOffset.MinValue).FirstOrDefault(), Direction.Before, Math.Min(100, maxm - msgi)).Flatten()).Count() > 0)
                {
                    Array.Copy(msgt.ToArray(), 0, msgs, msgi, msgt.Count());
                    msgi += msgt.Count();
                }
                foreach (var xmsg in msgs)
                    chain.Add(xmsg.Content.Split(' '), 1);
            }

            var sentence = string.Join(" ", chain.Chain(rnd));
            var embed = this.PrepareEmbed("Markov Chain", string.Concat("Markov chain of ", mnt, "."), EmbedType.Info);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Markov Chain";
                x.Value = sentence;
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("enableadvancedcommand", "Enables an Advanced Commands command. This command can only be used by server administrators.", Aliases = "enableac;enableadvcmd", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public async Task EnableAdvancedCommand(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var cmds = ctx.RawArguments;
            if (cmds.Count == 0)
                throw new ArgumentException("You need to list commands you want to enable.");
            AdvancedCommandsPlugin.Instance.SetEnabled(cmds.ToArray(), gld.Id, true);

            var embed = this.PrepareEmbed("Success", "Requested commands were enabled.", EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Enabled commands";
                x.Value = string.Join(", ", cmds);
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("disableadvancedcommand", "Disables an Advanced Commands command. This command can only be used by server administrators.", Aliases = "disableac;disableadvcmd", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public async Task DisableAdvancedCommand(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();

            var cmds = ctx.RawArguments;
            if (cmds.Count == 0)
                throw new ArgumentException("You need to list commands you want to disable.");
            AdvancedCommandsPlugin.Instance.SetEnabled(cmds.ToArray(), gld.Id, false);

            var embed = this.PrepareEmbed("Success", "Requested commands were disabled.", EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Disabled commands";
                x.Value = string.Join(", ", cmds);
            });
            await chn.SendMessageAsync("", false, embed);
        }

        private EmbedBuilder PrepareEmbed(string title, string desc, EmbedType type)
        {
            var embed = new EmbedBuilder();
            embed.Title = title;
            embed.Description = desc;
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.IconUrl = AdaBotCore.AdaClient.CurrentUser.AvatarUrl;
            embed.Author.Name = "ADA, a bot by Emzi0767";
            var ecolor = new s.Color(255, 255, 255);
            switch (type)
            {
                case EmbedType.Info:
                    ecolor = new s.Color(0, 127, 255);
                    break;

                case EmbedType.Success:
                    ecolor = new s.Color(127, 255, 0);
                    break;

                case EmbedType.Warning:
                    ecolor = new s.Color(255, 255, 0);
                    break;

                case EmbedType.Error:
                    ecolor = new s.Color(255, 127, 0);
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
