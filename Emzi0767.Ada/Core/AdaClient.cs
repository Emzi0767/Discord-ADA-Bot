using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Emzi0767.Ada.Commands;
using Emzi0767.Ada.Config;
using Emzi0767.Ada.Plugins;
using Emzi0767.Ada.Sql;

namespace Emzi0767.Ada.Core
{
    public sealed class AdaClient
    {
        public DiscordSocketClient DiscordClient { get; private set; }
        public CommandService DiscordCommands { get; private set; }
        public AdaUtilities Utilities { get; private set; }

        public AdaConfigurationManager ConfigurationManager { get; private set; }
        public AdaPluginManager PluginManager { get; private set; }
        public AdaSqlManager SqlManager { get; private set; }

        // periodic tasks
        private Timer BanHammer { get; set; }
        private int ShardId { get; set; }
        
        internal AdaClient(int shard, AdaConfigurationManager confman, AdaPluginManager plugman, AdaSqlManager sqlman)
        {
            this.DiscordClient = null;
            this.DiscordCommands = null;

            this.Utilities = new AdaUtilities(this);
            this.ConfigurationManager = confman;
            this.PluginManager = plugman;
            this.SqlManager = sqlman;

            this.BanHammer = null;
            this.ShardId = shard;
        }
        
        public void RegisterMessageHandler(Func<SocketMessage, Task> handler)
        {
            this.DiscordClient.MessageReceived += handler;
        }

        internal async Task InitializeAsync()
        {
            L.W("ADA CLIENT", "Initializing ADA for shard {0}", this.ShardId);

            L.W("ADA DSC", "Initializing socket client");
            var dconf = new DiscordSocketConfig
            {
                AudioMode = AudioMode.Disabled,
                TotalShards = this.ConfigurationManager.BotConfiguration.ShardCount,
                ShardId = this.ShardId,
                LogLevel = LogSeverity.Warning
            };

            this.DiscordClient = new DiscordSocketClient(dconf);

            L.W("ADA DSC", "Creating type readers");
            var tr_tsn = new AdaTimeSpanReader();

            L.W("ADA DSC", "Initializing command service");
            var cconf = new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async
            };

            this.DiscordCommands = new CommandService(cconf);
            this.DiscordCommands.AddTypeReader<TimeSpan?>(tr_tsn);

            L.W("ADA DSC", "Loading modules");
            var mtd = this.DiscordCommands.GetType().GetMethod("AddModuleAsync", BindingFlags.Public | BindingFlags.Instance);
            foreach (var module in this.PluginManager.Modules)
                await (Task)mtd.MakeGenericMethod(module).Invoke(this.DiscordCommands, null);

            L.W("ADA DSC", "Hooking socket events");
            this.DiscordClient.Log += Client_Log;
            this.DiscordClient.Ready += Client_Ready;
            this.DiscordClient.UserJoined += Client_UserJoined;
            this.DiscordClient.UserLeft += Client_UserLeft;
            this.DiscordClient.UserBanned += Client_UserBanned;
            this.DiscordClient.UserUnbanned += Client_UserUnbanned;
            this.DiscordClient.MessageReceived += Client_MessageReceived;
            this.DiscordClient.GuildAvailable += Client_GuildAvailable;
            this.DiscordClient.GuildUnavailable += Client_GuildUnavailable;

            L.W("ADA DSC", "Logging in and connecting");
            await this.DiscordClient.LoginAsync(TokenType.Bot, this.ConfigurationManager.BotConfiguration.Token);
            await this.DiscordClient.ConnectAsync();

            L.W("ADA DSC", "Connected and running");
        }

        internal async Task Deinitialize()
        {
            L.W("ADA DSC", "Disconnect requested");
            await this.DiscordClient.LogoutAsync();
            await this.DiscordClient.DisconnectAsync();
        }

        public string GetPrefix(SocketGuild gld)
        {
            var prefix = "/";
            if (this.DiscordClient.CurrentUser.Id != 207900508562653186u)
                prefix = "?";

            var gconf = this.ConfigurationManager.GetConfiguration(gld);
            if (gconf != null && gconf.CommandPrefix != null && !string.IsNullOrWhiteSpace(gconf.CommandPrefix))
                prefix = gconf.CommandPrefix;

            return prefix;
        }

        private Task Client_Log(LogMessage e)
        {
            L.W("DISCORD", "{0}/{1}: {2}", e.Severity, e.Source, e.Message);
            if (e.Exception != null)
                L.X("DISCORD", e.Exception);
            return Task.CompletedTask;
        }

        private Task Client_Ready()
        {
            this.BanHammer = new Timer(new TimerCallback(BanHammer_Tick), null, 0, 60000);
            return Task.CompletedTask;
        }

        private async Task Client_UserJoined(SocketGuildUser arg)
        {
            await this.Utilities.AnnounceUserAsync(arg, ModLogEntryType.UserJoin);
        }

        private async Task Client_UserLeft(SocketGuildUser arg)
        {
            await this.Utilities.AnnounceUserAsync(arg, ModLogEntryType.UserLeave);
        }

        private async Task Client_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            
        }

        private async Task Client_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {

        }

        private async Task Client_GuildAvailable(SocketGuild arg)
        {
            await this.ConfigurationManager.CacheGuildAsync(arg);
            L.W("ADA DSC", "Guild '{0}' ({1}) is now available", arg.Name, arg.Id);
        }

        private Task Client_GuildUnavailable(SocketGuild arg)
        {
            this.ConfigurationManager.UncacheGuild(arg);
            L.W("ADA DSC", "Guild '{0}' ({1}) is now unavailable", arg.Name, arg.Id);
            return Task.CompletedTask;
        }

        private async Task Client_MessageReceived(SocketMessage arg)
        {
            await Task.Yield();

            var msg = arg as SocketUserMessage;
            if (msg == null || msg.Author == null || msg.Author.IsBot)
                return;

            var chn = msg.Channel as SocketTextChannel;
            if (chn == null)
                return;

            var gld = chn.Guild;
            if (gld == null)
                return;

            var client = this.DiscordClient;

            var argpos = 0;
            var cprefix = this.GetPrefix(gld);
            if (!msg.HasStringPrefix(cprefix, ref argpos) && !msg.HasMentionPrefix(client.CurrentUser, ref argpos))
                return;

            var ctx = new AdaCommandContext(client, msg, this.Utilities, this.ConfigurationManager, this.SqlManager);
            var rst = await this.DiscordCommands.ExecuteAsync(ctx, argpos);

            if (!rst.IsSuccess && rst is ExecuteResult)
                await this.HandleCommandExceptionAsync(ctx, (ExecuteResult)rst);
            else if (!rst.IsSuccess)
                await this.HandleCommandErrorAsync(ctx, rst);
        }

        private async Task HandleCommandErrorAsync(AdaCommandContext ctx, IResult rst)
        {
            if (rst.Error != null && rst.Error == CommandError.UnknownCommand)
                return;

            var embed = this.Utilities.BuildEmbed(ctx, "Error executing command", string.Concat("An error occured when executing command: `", rst.ErrorReason, "`."), AdaUtilities.EmbedColour.Error);
            await ctx.Channel.SendMessageAsync("", false, embed);
        }

        private async Task HandleCommandExceptionAsync(AdaCommandContext ctx, ExecuteResult rst)
        { 
            if (rst.Exception != null)
            {
                L.X("DSC CMD", rst.Exception);
            
                var embed = this.Utilities.BuildEmbed(ctx, "Error executing command", string.Concat("An exception occured when executing command: `", rst.Exception.GetType(), ": ", rst.Exception.Message, "`."), AdaUtilities.EmbedColour.Error);
                await ctx.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var embed = this.Utilities.BuildEmbed(ctx, "Error executing command", string.Concat("An unknown error occured when executing command."), AdaUtilities.EmbedColour.Error);
                await ctx.Channel.SendMessageAsync("", false, embed);
            }
        }

        private void BanHammer_Tick(object _)
        {

        }
    }
}
