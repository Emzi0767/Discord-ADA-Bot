using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Core;

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
            await url.Edit(null, null, new Color(clr), null, null, null);

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
