using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Emzi0767.Ada.Sql;
using Npgsql;
using NpgsqlTypes;

namespace Emzi0767.Ada.Config
{
    public class ModLogEntry
    {
        private AdaSqlManager SqlManager { get; set; }

        public long CaseId { get; private set; }
        public ulong GuildId { get; private set; }
        public ulong TargetUser { get; private set; }
        public ModLogEntryType ActionType { get; set; }
        public ulong? Moderator { get; set; }
        public string Reason { get; set; }
        public DateTimeOffset ActionTimestamp { get; private set; }
        public DateTimeOffset? Until { get; private set; }
        public ulong AttachedMessage { get; private set; }

        internal ModLogEntry(AdaSqlManager sql)
        {
            this.SqlManager = sql;
        }

        public async Task CommitAsync()
        {
            var ps = new NpgsqlParameter[8];
            ps[0] = new NpgsqlParameter("guild_id", NpgsqlDbType.Bigint);
            ps[0].Value = (long)this.GuildId;
            ps[1] = new NpgsqlParameter("target_user", NpgsqlDbType.Bigint);
            ps[1].Value = (long)this.TargetUser;
            ps[2] = new NpgsqlParameter("action_type", NpgsqlDbType.Smallint);
            ps[2].Value = (short)this.ActionType;
            ps[3] = new NpgsqlParameter("moderator", NpgsqlDbType.Bigint);
            ps[3].Value = (long?)this.Moderator == null ? (object)DBNull.Value : (long?)this.Moderator;
            ps[4] = new NpgsqlParameter("reason", NpgsqlDbType.Varchar);
            ps[4].Value = string.IsNullOrWhiteSpace(this.Reason) ? (object)DBNull.Value : this.Reason;
            ps[5] = new NpgsqlParameter("action_timestamp", NpgsqlDbType.TimestampTZ);
            ps[5].Value = this.ActionTimestamp;
            ps[6] = new NpgsqlParameter("until", NpgsqlDbType.TimestampTZ);
            ps[6].Value = this.Until == null ? (object)DBNull.Value : this.Until;
            ps[7] = new NpgsqlParameter("attached_message_id", NpgsqlDbType.Bigint);
            ps[7].Value = (long)this.AttachedMessage;
            
            var rst = await this.SqlManager.QueryAsync("INSERT INTO ada_moderator_actions(guild_id, target_user, action_type, moderator, reason, action_timestamp, until, attached_message_id) VALUES(:guild_id, :target_user, :action_type, :moderator, :reason, :action_timestamp, :until, :attached_message_id) ON CONFLICT(guild_id, target_user, action_type, action_timestamp) DO UPDATE SET action_type=EXCLUDED.action_type, moderator=EXCLUDED.moderator, reason=EXCLUDED.reason RETURNING id;", ps);
            var rs = rst.First();

            this.CaseId = (long)rs["id"];
        }

        internal async Task PullAsync(SocketGuild gld, long case_id)
        {
            var ps = new NpgsqlParameter[2];
            ps[0] = new NpgsqlParameter("id", NpgsqlDbType.Bigint);
            ps[0].Value = case_id;
            ps[1] = new NpgsqlParameter("guild_id", NpgsqlDbType.Bigint);
            ps[1].Value = (long)gld.Id;

            var rst = await this.SqlManager.QueryAsync("SELECT id, guild_id, target_user, action_type, moderator, reason, action_timestamp, until, attached_message_id FROM ada_moderator_actions WHERE id=:id AND guild_id=:guild_id;", ps);
            var rs = rst.FirstOrDefault();
            if (rs == null)
                throw new ArgumentException("Could not find specified case");

            var mod = rs["moderator"] == DBNull.Value ? null : (ulong?)(long?)rs["moderator"];
            var rsn = rs["reason"] == DBNull.Value ? null : (string)rs["reason"];
            var unt = rs["until"] == DBNull.Value ? null : new DateTime?(((DateTime)rs["until"]).ToUniversalTime());

            this.CaseId = (long)rs["id"];
            this.GuildId = (ulong)(long)rs["guild_id"];
            this.TargetUser = (ulong)(long)rs["target_user"];
            this.ActionType = (ModLogEntryType)(short)rs["action_type"];
            this.Moderator = mod;
            this.Reason = rsn;
            this.ActionTimestamp = new DateTimeOffset(((DateTime)rs["action_timestamp"]).ToUniversalTime());
            this.Until = unt == null ? null : new DateTimeOffset?(new DateTimeOffset((DateTime)unt));
            this.AttachedMessage = (ulong)(long)rs["attached_message_id"];
        }

        internal async Task CreateAsync(SocketGuildUser usr, ModLogEntryType type, SocketGuildUser mod, string reason, TimeSpan? duration, IMessage msg)
        {
            await this.CreateAsync(usr.Guild, usr, type, mod, reason, duration, msg);
        }

        internal async Task CreateAsync(SocketGuild gld, IUser usr, ModLogEntryType type, SocketGuildUser mod, string reason, TimeSpan? duration, IMessage msg)
        {
            this.GuildId = gld.Id;
            this.TargetUser = usr.Id;
            this.ActionType = type;
            this.Moderator = mod != null ? (ulong?)mod.Id : null;
            this.Reason = reason;
            this.ActionTimestamp = DateTimeOffset.UtcNow;
            this.Until = duration != null ? (DateTimeOffset?)(this.ActionTimestamp + (TimeSpan)duration) : null;
            this.AttachedMessage = msg.Id;

            await this.CommitAsync();
        }
    }
}
