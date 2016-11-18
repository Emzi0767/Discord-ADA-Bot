using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Core;

namespace Emzi0767.Net.Discord.Ada.Feedle
{
    [CommandHandler]
    public class FeedleCommands
    {
        [Command("addrss", "Adds an RSS feed to a specified channel. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public static async Task AddRss(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var args = ea.Args[0].Split(';')
                .Select(xs => xs.Split('='))
                .ToDictionary(xsa => xsa[0], xsa => xsa[1]);

            var uri = args["feed"];
            var chm = args["channel"];
            var tag = args.ContainsKey("tag") ? args["tag"] : null;
            var cha = srv.AllChannels.FirstOrDefault(xch => xch.Name == chm);

            FeedlePlugin.AddFeed(new Uri(uri), cha.Id, tag);
            await chn.SendMessage(string.Format("**ADA**: added RSS feed **<{0}>** to channel **{1}**{2}", uri, cha.Name, tag == null ? "" : string.Concat(" using tag **[", tag, "]**")));
        }

        [Command("rmrss", "Removes an RSS feed from a specified channel. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public static async Task RemoveRss(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var args = ea.Args[0].Split(';')
                .Select(xs => xs.Split('='))
                .ToDictionary(xsa => xsa[0], xsa => xsa[1]);

            var uri = args["feed"];
            var chm = args["channel"];
            var tag = args.ContainsKey("tag") ? args["tag"] : null;
            var cha = srv.AllChannels.FirstOrDefault(xch => xch.Name == chm);

            FeedlePlugin.RemoveFeed(new Uri(uri), cha.Id, tag);
            await chn.SendMessage(string.Format("**ADA**: removed RSS feed **<{0}>** from channel **{1}**{2}", uri, cha.Name, tag == null ? "" : string.Concat(" using tag **[", tag, "]**")));
        }

        [Command("listrss", "Lists RSS feeds active on the current server. This command can only be used by server administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public static async Task ListRss(CommandEventArgs ea)
        {
            var srv = ea.Server;
            var chn = ea.Channel;
            var msg = ea.Message;
            var usr = ea.User;

            await msg.Delete();

            var feeds = FeedlePlugin.GetFeeds(srv.AllChannels.Select(xch => xch.Id).ToArray());

            var msgs = new List<string>();
            var sb = new StringBuilder();
            var msb = new StringBuilder();
            sb.AppendLine("**ADA**: listing all feeds configured for this server:").AppendLine();
            foreach (var feed in feeds)
            {
                var xch = srv.GetChannel(feed.ChannelId);
                msb = new StringBuilder();
                msb.AppendFormat("**URL**: <{0}>", feed.FeedUri).AppendLine();
                msb.AppendFormat("**Tag**: {0}", feed.Tag).AppendLine();
                msb.AppendFormat("**Channel**: {0}", xch.Mention).AppendLine();
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
    }
}
