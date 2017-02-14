using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Emzi0767.Ada.Commands;
using Emzi0767.Ada.Commands.Permissions;
using Emzi0767.Ada.Config;

namespace Emzi0767.Ada.Core
{
    public sealed class AdaUtilities
    {
        public AdaClient AdaClient { get; private set; }

        private Dictionary<AdaPermission, string> PermissionStrings { get; set; }

        public AdaUtilities(AdaClient client)
        {
            this.AdaClient = client;

            this.PermissionStrings = new Dictionary<AdaPermission, string>();
            var t = typeof(AdaPermission);
            var ti = t.GetTypeInfo();
            var vs = Enum.GetValues(t).Cast<AdaPermission>();

            foreach (var xv in vs)
            {
                var xsv = xv.ToString();
                var xmv = ti.GetMember(xsv).First();
                var xav = xmv.GetCustomAttribute<PermissionStringAttribute>();

                this.PermissionStrings.Add(xv, xav.String);
            }
        }

        public EmbedBuilder BuildEmbed(AdaCommandContext ctx, string title, string description, EmbedColour colour)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Title and description cannot be empty.");

            var clr = this.EnumToColour(colour);

            var embed = new EmbedBuilder();
            embed.Title = title;
            embed.Description = description;
            embed.Color = new Color(clr);
            embed.ThumbnailUrl = ctx.Client.CurrentUser.AvatarUrl;

            return embed;
        }

        public EmbedBuilder BuildEmbed(AdaClient client, string title, string description, EmbedColour colour)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Title and description cannot be empty.");

            var clr = this.EnumToColour(colour);

            var embed = new EmbedBuilder();
            embed.Title = title;
            embed.Description = description;
            embed.Color = new Color(clr);
            embed.ThumbnailUrl = client.DiscordClient.CurrentUser.AvatarUrl;

            return embed;
        }

        public EmbedBuilder CreateEmbedField(EmbedBuilder embed, string name, string value)
        {
            return this.CreateEmbedField(embed, name, value, true);
        }

        public EmbedBuilder CreateEmbedField(EmbedBuilder embed, string name, string value, bool inline)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name and value cannot be empty.");

            embed.AddField(x =>
            {
                x.Name = name;
                x.Value = value;
                x.IsInline = inline;
            });

            return embed;
        }

        public string GetPermissionString(AdaPermission perm)
        {
            if (perm == AdaPermission.None)
                return this.PermissionStrings[perm];

            var strs = this.PermissionStrings
                .Where(xkvp => xkvp.Key != AdaPermission.None && (perm & xkvp.Key) == xkvp.Key)
                .Select(xkvp => xkvp.Value);

            return string.Join(", ", strs);
        }

        public bool CheckModLogConfiguration(SocketGuild guild)
        {
            return this.GetModLog(guild) != null;
        }

        public async Task ReportUserAsync(SocketGuildUser reportee, SocketGuildUser reporter, string reason)
        {
            var embed = this.BuildEmbed(this.AdaClient, "User report", string.Concat(reporter.Mention, " has reported ", reportee.Mention), EmbedColour.Info);
            this.CreateEmbedField(embed, "Reason", reason, false);

            var ml = this.GetModLog(reportee.Guild);
            await ml.SendMessageAsync("", false, embed);
        }

        public async Task AnnounceUserAsync(SocketGuildUser member, ModLogEntryType type)
        {
            var embed = this.BuildEmbed(this.AdaClient, this.GetActionTitle(type), this.GetActionDescription(type, member), EmbedColour.Info);
            embed.WithCurrentTimestamp();

            var ml = this.GetModLog(member.Guild);
            await ml.SendMessageAsync("", false, embed);
        }

        public async Task<ModLogEntry> CreateModLogAsync(SocketGuildUser member, ModLogEntryType type, TimeSpan? duration, SocketGuildUser mod)
        {
            return await this.CreateModLogAsync(member.Guild, member, type, duration, mod);
        }

        public async Task<ModLogEntry> CreateModLogAsync(SocketGuild gld, IUser member, ModLogEntryType type, TimeSpan? duration, SocketGuildUser mod)
        {
            var embed = this.BuildEmbed(this.AdaClient, this.GetActionTitle(type), this.GetActionDescription(type, member), EmbedColour.Info);
            this.CreateEmbedField(embed, "Case ID", "Unknown");
            this.CreateEmbedField(embed, "Timestamp", "Unknown");
            this.CreateEmbedField(embed, "Responsible moderator", "Unknown");
            this.CreateEmbedField(embed, "Reason", "Unknown");
            this.CreateEmbedField(embed, "Until", "Unknown");

            var ml = this.GetModLog(gld);
            var msg = await ml.SendMessageAsync("", false, embed);

            var mle = new ModLogEntry(this.AdaClient.SqlManager);
            await mle.CreateAsync(gld, member, type, mod, null, duration, msg);

            embed = this.BuildEmbed(this.AdaClient, this.GetActionTitle(type), this.GetActionDescription(type, member), EmbedColour.Info);
            this.CreateEmbedField(embed, "Case ID", mle.CaseId.ToString());
            this.CreateEmbedField(embed, "Timestamp", mle.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss zzz"));
            this.CreateEmbedField(embed, "Responsible moderator", mod != null ? mod.Mention : "Unknown");
            this.CreateEmbedField(embed, "Reason", string.Concat("Unknown, responsible moderator, please do `", this.AdaClient.GetPrefix(gld), "members reason ", mle.CaseId, " <reason...>`."));
            this.CreateEmbedField(embed, "Until", mle.Until != null ? mle.Until.Value.ToString("yyyy-MM-dd HH:mm:ss zzz") : this.GetActionNoUntilString(type));
            await msg.ModifyAsync(x => x.Embed = embed.Build(), null);

            return mle;
        }

        public async Task ModifyModLogAsync(ModLogEntry mle, ModLogEntryType type, SocketGuildUser mod, string reason)
        {
            mle.ActionType = type;
            mle.Moderator = mod.Id;
            mle.Reason = reason;
            await mle.CommitAsync();

            var ml = this.GetModLog(mod.Guild);
            var msg = await ml.GetMessageAsync(mle.AttachedMessage) as IUserMessage;

            var embed_old = msg.Embeds.First();
            var embed = this.BuildEmbed(this.AdaClient, embed_old.Title, embed_old.Description, EmbedColour.Info);
            this.CreateEmbedField(embed, "Case ID", mle.CaseId.ToString());
            this.CreateEmbedField(embed, "Timestamp", mle.ActionTimestamp.ToString("yyyy-MM-dd HH:mm:ss zzz"));
            this.CreateEmbedField(embed, "Responsible moderator", mod.Mention);
            this.CreateEmbedField(embed, "Reason", reason);
            this.CreateEmbedField(embed, "Until", mle.Until != null ? mle.Until.Value.ToString("yyyy-MM-dd HH:mm:ss zzz") : this.GetActionNoUntilString(type));
            await msg.ModifyAsync(x => x.Embed = embed.Build());
        }

        public async Task<ModLogEntry> GetModLog(SocketGuild gld, long case_id)
        {
            var mle = new ModLogEntry(this.AdaClient.SqlManager);
            await mle.PullAsync(gld, case_id);

            return mle;
        }

        public string GetQualifiedName(CommandInfo cmd, SocketGuild gld)
        {
            var qname = cmd.Name;

            var mod = cmd.Module;
            if (mod != null && mod.IsSubmodule && mod.Aliases.Count > 0 && !string.IsNullOrWhiteSpace(mod.Aliases.First()))
                qname = string.Concat(mod.Aliases.First(), " ", qname);

            return string.Concat(this.AdaClient.GetPrefix(gld), qname);
        }

        private uint EnumToColour(EmbedColour colour)
        {
            switch (colour)
            {
                default:
                case EmbedColour.None:
                    return 0x00000000u;

                case EmbedColour.Info:
                    return 0x00007FFFu;

                case EmbedColour.Success:
                    return 0x007FFF00u;

                case EmbedColour.Warning:
                    return 0x00FF7F00u;

                case EmbedColour.Error:
                    return 0x00FF0000u;

                case EmbedColour.Debug:
                    return 0x007F00FFu;
            }
        }

        private string GetActionTitle(ModLogEntryType type)
        {
            switch (type)
            {
                case ModLogEntryType.UserBan:
                    return "User banned";

                case ModLogEntryType.UserJoin:
                    return "User joined";

                case ModLogEntryType.UserKick:
                    return "User kicked";

                case ModLogEntryType.UserLeave:
                    return "User left or kicked";

                case ModLogEntryType.UserMute:
                    return "User muted";

                case ModLogEntryType.UserSoftban:
                    return "User softbanned";

                case ModLogEntryType.UserUnban:
                    return "User unbanned";

                case ModLogEntryType.UserUnmute:
                    return "User unmuted";

                default:
                case ModLogEntryType.Unknown:
                    return "##ERROR##";
            }
        }

        private string GetActionDescription(ModLogEntryType type, IUser usr)
        {
            switch (type)
            {
                case ModLogEntryType.UserBan:
                    return string.Concat(usr.Mention, " was banned.");

                case ModLogEntryType.UserJoin:
                    return string.Concat(usr.Mention, " has joined.");

                case ModLogEntryType.UserKick:
                    return string.Concat(usr.Mention, " was kicked.");

                case ModLogEntryType.UserLeave:
                    return string.Concat(usr.Mention, " left the guild or was kicked.");

                case ModLogEntryType.UserMute:
                    return string.Concat(usr.Mention, " was muted.");

                case ModLogEntryType.UserSoftban:
                    return string.Concat(usr.Mention, " was softbanned.");

                case ModLogEntryType.UserUnban:
                    return string.Concat(usr.Mention, " was unbanned.");

                case ModLogEntryType.UserUnmute:
                    return string.Concat(usr.Mention, " was unmuted.");

                default:
                case ModLogEntryType.Unknown:
                    return "**__*A CATASTROPHIC FAILURE OCCURED*__**.";
            }
        }

        private string GetActionNoUntilString(ModLogEntryType type)
        {
            switch (type)
            {
                case ModLogEntryType.UserBan:
                case ModLogEntryType.UserMute:
                    return "Indefinitely";

                case ModLogEntryType.UserKick:
                case ModLogEntryType.UserSoftban:
                case ModLogEntryType.UserUnban:
                case ModLogEntryType.UserUnmute:
                    return "Not applicable";

                case ModLogEntryType.UserJoin:
                case ModLogEntryType.UserLeave:
                    return "";

                case ModLogEntryType.Unknown:
                default:
                    return "**__*ERROR*__**";
            }
        }

        private SocketTextChannel GetModLog(SocketGuild guild)
        {
            var gconf = this.AdaClient.ConfigurationManager.GetConfiguration(guild);
            if (gconf == null)
                return null;

            return gconf.ModerationLog;
        }

        public enum EmbedColour : int
        {
            None = 0,
            Info = 1,
            Success = 2,
            Warning = 3,
            Error = 4,
            Debug = 5
        }
    }
}
