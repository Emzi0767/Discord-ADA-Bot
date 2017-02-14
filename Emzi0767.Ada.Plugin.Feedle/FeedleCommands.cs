using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Emzi0767.Ada.Attributes;
using Emzi0767.Ada.Commands;
using Emzi0767.Ada.Commands.Permissions;

namespace Emzi0767.Ada.Plugin.Feedle
{
    public class FeedleCommands : IAdaCommandModule
    {
        public string Name { get { return "ADA RSS Commands"; } }

        [Command("addrss", "Adds an RSS feed to a specified channel.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public async Task AddRss(AdaCommandContext ctx,
            [AdaArgumentParameter("Mention of the channel to add the feed to.", true)] ITextChannel channel,
            [AdaArgumentParameter("URL of the RSS feed.", true)] string url,
            [AdaArgumentParameter("Tag of the feed to use as title prefix.", false)] string tag)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var chf = channel as SocketTextChannel;
            if (chf == null)
                throw new ArgumentException("Invalid channel specified.");

            FeedlePlugin.Instance.AddFeed(new Uri(url), chf.Id, tag);
            var embed = this.PrepareEmbed("Success", "Feed was added successfully.", EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Details";
                x.Value = string.Concat("Feed pointing to <", url, ">", tag != null ? string.Concat(" and **", tag, "** tag") : "", " was added to ", chf.Mention, ".");
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [Command("rmrss", "Removes an RSS feed from a specified channel.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public async Task RemoveRss(AdaCommandContext ctx,
            [AdaArgumentParameter("Mention of the channel to remove the feed from.", true)] ITextChannel channel,
            [AdaArgumentParameter("URL of the RSS feed.", true)] string url,
            [AdaArgumentParameter("Tag of the feed to use as title prefix.", false)] string tag)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            var chf = channel as SocketTextChannel;
            if (chf == null)
                throw new ArgumentException("Invalid channel specified.");

            FeedlePlugin.Instance.RemoveFeed(new Uri(url), chf.Id, tag);
            var embed = this.PrepareEmbed("Success", "Feed was removed successfully.", EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Details";
                x.Value = string.Concat("Feed pointing to <", url, ">", tag != null ? string.Concat(" and **", tag, "** tag") : "", " was removed from ", chf.Mention, ".");
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [Command("listrss", "Lists RSS feeds active in the current guild.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public async Task ListRss(AdaCommandContext ctx)
        {
            var gld = ctx.Guild as SocketGuild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;
            
            var feeds = FeedlePlugin.Instance.GetFeeds(gld.Channels.Select(xch => xch.Id).ToArray());
            
            var sb = new StringBuilder();
            foreach (var feed in feeds)
            {
                var xch = gld.GetChannel(feed.ChannelId) as SocketTextChannel;
                sb.AppendFormat("**URL**: <{0}>", feed.FeedUri).AppendLine();
                sb.AppendFormat("**Tag**: {0}", feed.Tag).AppendLine();
                sb.AppendFormat("**Channel**: {0}", xch.Mention).AppendLine();
                sb.AppendLine("---------");
            }

            var embed = this.PrepareEmbed("RSS Feeds", "Listing of all RSS feeds on this server.", EmbedType.Info);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "RSS Feeds";
                x.Value = sb.Length > 0 ? sb.ToString() : "No feeds are configured.";
            });
            await chn.SendMessageAsync("", false, embed);
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
            embed.ThumbnailUrl = AdaBotProgram.AdaClient.CurrentUser.AvatarUrl;
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
