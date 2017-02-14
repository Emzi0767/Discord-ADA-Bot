using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Emzi0767.Ada.Sql;
using Npgsql;
using NpgsqlTypes;

namespace Emzi0767.Ada.Config
{
    public class AdaGuildConfiguration
    {
        internal static AdaSqlManager SqlManager { get; set; }
        private static readonly string[] RESTRICTED_NAMES;

        private const string MODLOG = "modlog";
        private const string MUTE_ROLE = "mute_role";
        private const string COMMAND_PREFIX = "command_prefix";
        private const string DISABLED_COMMANDS = "disabled_commands";

        #region Properties
        public SocketGuild Guild { get; private set; }
        public ulong GuildId { get { return this.Guild.Id; } }

        public SocketTextChannel ModerationLog
        {
            get { return this.Guild.GetTextChannel(this._modlog); }
            set { this.ModerationLogId = value.Id; }
        }
        public ulong ModerationLogId
        {
            get { return this._modlog; }

            set
            {
                if (this.Guild.GetTextChannel(value) != null)
                {
                    this._modlog = value;
                    this.RawValues[MODLOG] = value.ToString();
                    this.CommitProxy(MODLOG);
                    return;
                }

                throw new InvalidOperationException("Attempted to assign non-existent channel");
            }
        }
        private ulong _modlog;

        public SocketRole MuteRole
        {
            get { return this.Guild.GetRole(this._muterole); }
            set { this.MuteRoleId = value.Id; }
        }
        public ulong MuteRoleId
        {
            get { return this._muterole; }

            set
            {
                if (this.Guild.GetRole(value) != null)
                {
                    this._muterole = value;
                    this.RawValues[MUTE_ROLE] = value.ToString();
                    this.CommitProxy(MUTE_ROLE);
                    return;
                }

                throw new InvalidOperationException("Attempted to assign non-existent role");
            }
        }
        private ulong _muterole;

        public string CommandPrefix
        {
            get { return this._prefix; }

            set
            {
                this._prefix = value;
                this.RawValues[COMMAND_PREFIX] = value;
                this.CommitProxy(COMMAND_PREFIX);
            }
        }
        private string _prefix;

        private List<string> _disabled;

        private Dictionary<string, string> RawValues { get; set; }
        #endregion

        #region Constructors
        public AdaGuildConfiguration(SocketGuild guild)
        {
            this.Guild = guild;
            this.RawValues = new Dictionary<string, string>();

            this._modlog = 0u;
            this._muterole = 0u;
            this._prefix = null;
            this._disabled = new List<string>();
        }

        static AdaGuildConfiguration()
        {
            RESTRICTED_NAMES = new string[] { MODLOG, MUTE_ROLE, COMMAND_PREFIX, DISABLED_COMMANDS };
        }
        #endregion

        public async Task SetSettingAsync(string setting, string value)
        {
            if (RESTRICTED_NAMES.Contains(setting))
                throw new InvalidOperationException("Attempted to set a restricted setting");

            this.RawValues[setting] = value;
            await this.CommitAsync(setting);
        }

        public string GetSetting(string setting)
        {
            if (RESTRICTED_NAMES.Contains(setting))
                throw new InvalidOperationException("Attempted to set a restricted setting");

            if (this.RawValues.ContainsKey(setting))
                return this.RawValues[setting];

            throw new KeyNotFoundException("Specified setting is not set");
        }

        public async Task SetCommandStateAsync(CommandInfo cmd, bool state)
        {
            var qname = this.GetQualifiedName(cmd);

            if (state)
                this._disabled.Remove(qname);
            else
                this._disabled.Add(qname);

            this.RawValues[DISABLED_COMMANDS] = string.Join(";", this._disabled);
            await this.CommitAsync(DISABLED_COMMANDS);
        }

        public bool GetCommandState(CommandInfo cmd)
        {
            var qname = this.GetQualifiedName(cmd);

            return !this._disabled.Contains(qname);
        }

        internal async Task InitializeAsync()
        {
            var ps = new NpgsqlParameter[1];
            ps[0] = new NpgsqlParameter("guild_id", NpgsqlDbType.Bigint);
            ps[0].Value = ((long)this.GuildId).ToString();
            
            var st = await SqlManager.QueryAsync("SELECT setting_name, setting_value FROM ada_guild_settings WHERE guild_id=:guild_id;", ps);

            foreach (var xs in st)
                this.RawValues[xs["setting_name"].ToString()] = xs["setting_value"].ToString();

            if (this.RawValues.ContainsKey(COMMAND_PREFIX))
                this._prefix = this.RawValues[COMMAND_PREFIX];

            if (this.RawValues.ContainsKey(MODLOG))
                this._modlog = ulong.Parse(this.RawValues[MODLOG]);

            if (this.RawValues.ContainsKey(MUTE_ROLE))
                this._muterole = ulong.Parse(this.RawValues[MUTE_ROLE]);

            if (this.RawValues.ContainsKey(DISABLED_COMMANDS))
                this._disabled.AddRange(this.RawValues[DISABLED_COMMANDS].Split(';'));
        }

        private async Task CommitAsync()
        {
            var ps = new NpgsqlParameter[3];
            ps[0] = new NpgsqlParameter("guild_id", NpgsqlDbType.Bigint);
            ps[1] = new NpgsqlParameter("setting_name", NpgsqlDbType.Varchar);
            ps[2] = new NpgsqlParameter("setting_value", NpgsqlDbType.Varchar);

            var psvs = new Dictionary<string, object>[this.RawValues.Count];
            var i = 0;

            foreach (var kvp in this.RawValues)
            {
                var psv = new Dictionary<string, object>();
                psv.Add("guild_id", (long)this.GuildId);
                psv.Add("setting_name", kvp.Key);
                psv.Add("setting_value", kvp.Value);
                psvs[i++] = psv;
            }

            await SqlManager.QueryNonReaderAsync("INSERT INTO ada_guild_settings(guild_id, setting_name, setting_value) VALUES(:guild_id, :setting_name, :setting_value) ON CONFLICT(guild_id, setting_name) DO UPDATE SET setting_value=EXCLUDED.setting_value;",
                ps, psvs);
        }

        private async Task CommitAsync(string setting)
        {
            var ps = new NpgsqlParameter[3];
            ps[0] = new NpgsqlParameter("guild_id", NpgsqlDbType.Bigint);
            ps[1] = new NpgsqlParameter("setting_name", NpgsqlDbType.Varchar);
            ps[2] = new NpgsqlParameter("setting_value", NpgsqlDbType.Varchar);

            var psv = new Dictionary<string, object>();
            psv.Add("guild_id", (long)this.GuildId);
            psv.Add("setting_name", setting);
            psv.Add("setting_value", this.RawValues[setting]);

            await SqlManager.QueryNonReaderAsync("INSERT INTO ada_guild_settings(guild_id, setting_name, setting_value) VALUES(:guild_id, :setting_name, :setting_value) ON CONFLICT(guild_id, setting_name) DO UPDATE SET setting_value=EXCLUDED.setting_value;",
                ps, new[] { psv });
        }

        private void CommitProxy(string setting)
        {
            this.CommitAsync(setting).GetAwaiter().GetResult();
        }

        private string GetQualifiedName(CommandInfo cmd)
        {
            var qname = cmd.Name;

            var mod = cmd.Module;
            if (mod != null && mod.IsSubmodule && mod.Aliases.Count > 0 && !string.IsNullOrWhiteSpace(mod.Aliases.First()))
                qname = string.Concat(mod.Aliases.First(), " ", qname);

            return qname;
        }
    }
}
