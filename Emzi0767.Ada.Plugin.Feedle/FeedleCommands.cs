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

        [AdaCommand("addrss", "Adds an RSS feed to a specified channel. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        [AdaCommandParameter(0, "channel", "Mention of a channel to add the feed to.", true)]
        [AdaCommandParameter(1, "url", "URL of the RSS feed.", true)]
        [AdaCommandParameter(2, "tag", "Tag of the feed to use as title prefix.", false)]
        public async Task AddRss(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            if (msg.MentionedChannelIds.Count == 0)
                throw new ArgumentException("You need to mention a channel you want to add a feed to.");
            var chf = msg.MentionedChannelIds.FirstOrDefault();
            var chx = await gld.GetChannelAsync(chf) as SocketTextChannel;
            var url = ctx.RawArguments[1];
            var tag = ctx.RawArguments.Count > 2 ? ctx.RawArguments[2] : null;

            FeedlePlugin.Instance.AddFeed(new Uri(url), chf, tag);
            var embed = this.PrepareEmbed("Success", "Feed was added successfully.", EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Details";
                x.Value = string.Concat("Feed pointing to <", url, ">", tag != null ? string.Concat(" and **", tag, "** tag") : "", " was added to ", chx.Mention, ".");
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("rmrss", "Removes an RSS feed from a specified channel. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        [AdaCommandParameter(0, "channel", "Mention of a channel to remove the feed from.", true)]
        [AdaCommandParameter(1, "url", "URL of the RSS feed.", true)]
        [AdaCommandParameter(2, "tag", "Tag used as the feed's title prefix.", false)]
        public async Task RemoveRss(AdaCommandContext ctx)
        {
            var gld = ctx.Guild;
            var chn = ctx.Channel;
            var msg = ctx.Message;

            await msg.DeleteAsync();

            if (msg.MentionedChannelIds.Count == 0)
                throw new ArgumentException("You need to mention a channel you want to remove a feed from.");
            var chf = msg.MentionedChannelIds.FirstOrDefault();
            var chx = await gld.GetChannelAsync(chf) as SocketTextChannel;
            var url = ctx.RawArguments[1];
            var tag = ctx.RawArguments.Count > 2 ? ctx.RawArguments[2] : null;

            FeedlePlugin.Instance.RemoveFeed(new Uri(url), chf, tag);
            var embed = this.PrepareEmbed("Success", "Feed was removed successfully.", EmbedType.Success);
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Details";
                x.Value = string.Concat("Feed pointing to <", url, ">", tag != null ? string.Concat(" and **", tag, "** tag") : "", " was removed from ", chx.Mention, ".");
            });
            await chn.SendMessageAsync("", false, embed);
        }

        [AdaCommand("listrss", "Lists RSS feeds active in the current guild. This command can only be used by guild administrators.", CheckerId = "CoreAdminChecker", CheckPermissions = true, RequiredPermission = AdaPermission.Administrator)]
        public async Task ListRss(AdaCommandContext ctx)
        {
            var gld = ctx.Guild as SocketGuild;
            var chn = ctx.Channel;
            var msg = ctx.Message;
            var usr = ctx.User;

            await msg.DeleteAsync();
            
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
