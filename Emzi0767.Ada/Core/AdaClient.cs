using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Emzi0767.Ada.Config;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Ada.Core
{
    public sealed class AdaClient
    {
        public IUser CurrentUser { get { return this.DiscordClient.CurrentUser; } }
        public string Game { get; private set; }
        internal DiscordSocketClient DiscordClient { get; private set; }
        internal JObject ConfigJson { get; private set; }
        private Timer BanHammer { get; set; }
        private string Token { get; set; }

        internal AdaClient()
        {
            L.W("ADA DSC", "Initializing Discord");
            this.Game = "Banhammer 40,000";

            var spr = AdaBotCore.SocketManager.SocketProvider;
            var spv = spr != null ? spr.Provider : null;
            var dsc = new DiscordSocketConfig()
            {
                LogLevel = Debugger.IsAttached ? LogSeverity.Debug : LogSeverity.Info,
                AudioMode = AudioMode.Disabled,
                WebSocketProvider = spv
            };
            
            this.DiscordClient = new DiscordSocketClient(dsc);
            this.DiscordClient.Log += Client_Log;
            this.DiscordClient.Ready += Client_Ready;

            // modlog events
            this.DiscordClient.UserJoined += DiscordClient_UserJoined;
            this.DiscordClient.UserLeft += DiscordClient_UserLeft;
            this.DiscordClient.UserBanned += DiscordClient_UserBanned;
            this.DiscordClient.UserUnbanned += DiscordClient_UserUnbanned;

            var a = typeof(AdaClient).GetTypeInfo().Assembly;
            var n = a.GetName();
            var l = Path.GetDirectoryName(a.Location);

            L.W("ADA DSC", "Loading config");
            var sp = Path.Combine(l, "config.json");
            var sjson = File.ReadAllText(sp, AdaBotCore.UTF8);
            var sjo = JObject.Parse(sjson);
            this.ConfigJson = sjo;
            this.Token = (string)sjo["token"];
            L.W("ADA DSC", "Discord initialized");
        }

        /// <summary>
        /// Registers a message received handler.
        /// </summary>
        /// <param name="handler">Handler to register.</param>
        public void RegisterMessageHandler(Func<SocketMessage, Task> handler)
        {
            this.DiscordClient.MessageReceived += handler;
        }

        /// <summary>
        /// Sends an embed to a sepcified channel.
        /// </summary>
        /// <param name="embed">Embed to send.</param>
        /// <param name="channel">Channel to send the embed to.</param>
        public void SendEmbed(EmbedBuilder embed, ulong channel)
        {
            var ch = (SocketTextChannel)null;
            var tg = DateTime.Now;
            while (ch == null && (DateTime.Now - tg).TotalSeconds < 10)
                ch = this.DiscordClient.GetChannel(channel) as SocketTextChannel;
            if (ch == null)
                return;
            this.SendEmbed(embed, ch);
        }

        internal void Initialize()
        {
            L.W("ADA DSC", "Connecting");
            this.DiscordClient.LoginAsync(TokenType.Bot, this.Token).GetAwaiter().GetResult();
            this.DiscordClient.ConnectAsync(true).GetAwaiter().GetResult();
            L.W("ADA DSC", "Connected as '{0}'", this.DiscordClient.CurrentUser.Username);
        }

        internal void Deinitialize()
        {
            L.W("ADA DSC", "Saving configs");
            AdaBotCore.PluginManager.UpdateAllConfigs();
            L.W("ADA DSC", "Disconnecting");
            this.DiscordClient.DisconnectAsync().GetAwaiter().GetResult();
            L.W("ADA DSC", "Disconnected");
        }

        /// <summary>
        /// Sends a message to a specified channel.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send the message to.</param>
        public void SendMessage(string message, ulong channel)
        {
            var ch = (SocketTextChannel)null;
            var tg = DateTime.Now;
            while (ch == null && (DateTime.Now - tg).TotalSeconds < 10)
                ch = this.DiscordClient.GetChannel(channel) as SocketTextChannel;
            if (ch == null)
                return;
            this.SendMessage(message, ch);
        }

        /// <summary>
        /// Sends a message to a specified channel
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send the message to.</param>
        internal void SendMessage(string message, SocketTextChannel channel)
        {
            var msg = new List<string>();
            if (message.Length > 2000)
            {
                var cmsg = "";
                message.Split(' ');
                foreach (var str in msg)
                {
                    if (str.Length + cmsg.Length > 2000)
                    {
                        msg.Add(cmsg);
                        cmsg = str;
                    }
                    else
                    {
                        cmsg += " " + str;
                    }
                }
                msg.Add(cmsg);
            }
            else
            {
                msg.Add(message);
            }

            foreach (var ms in msg)
                channel.SendMessageAsync(ms).GetAwaiter().GetResult();
        }

        internal void SendEmbed(EmbedBuilder embed, SocketTextChannel channel)
        {
            channel.SendMessageAsync("", false, embed).GetAwaiter().GetResult();
        }

        internal void WriteConfig()
        {
            var a = AdaBotCore.PluginManager.MainAssembly;
            var n = a.GetName();
            var l = Path.GetDirectoryName(a.Location);
            var sp = Path.Combine(l, "config.json");
            File.WriteAllText(sp, this.ConfigJson.ToString(), AdaBotCore.UTF8);
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

        private async Task DiscordClient_UserJoined(SocketGuildUser arg)
        {
            var usr = arg;
            var gld = usr.Guild;
            var gid = gld.Id;

            var cfg = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            if (cfg == null || cfg.ModLogChannel == null)
                return;

            var chn = await gld.GetTextChannelAsync(cfg.ModLogChannel.Value);
            if (chn == null)
                return;

            await chn.SendMessageAsync("", false, this.PrepareEmbed("User joined", usr.Mention, EmbedType.Info));
        }

        private async Task DiscordClient_UserLeft(SocketGuildUser arg)
        {
            var usr = arg;
            var gld = usr.Guild;
            var gid = gld.Id;

            var cfg = AdaBotCore.ConfigManager.GetGuildConfig(gid);
            if (cfg == null || cfg.ModLogChannel == null)
                return;

            var chn = await gld.GetTextChannelAsync(cfg.ModLogChannel.Value);
            if (chn == null)
                return;

            await chn.SendMessageAsync("", false, this.PrepareEmbed("User left", usr.Mention, EmbedType.Info));
        }

        private async Task DiscordClient_UserBanned(SocketUser arg1, SocketGuild arg2)
        {
            // figure out non-conflicting bans
            await Task.Delay(1);
        }

        private async Task DiscordClient_UserUnbanned(SocketUser arg1, SocketGuild arg2)
        {
            // figure out non-conflicting bans
            await Task.Delay(1);
        }

        private void BanHammer_Tick(object _)
        {
            var gconfs = AdaBotCore.ConfigManager != null ? AdaBotCore.ConfigManager.GetGuildConfigs() : new KeyValuePair<ulong, AdaGuildConfig>[0];
            if (gconfs.Count() > 0)
            {
                var now = DateTime.UtcNow;
                foreach (var kvp in gconfs.ToList())
                {
                    var gld = AdaBotCore.AdaClient.DiscordClient.GetGuild(kvp.Key) as SocketGuild;
                    var mrl = kvp.Value.MuteRole != null ? gld.GetRole(kvp.Value.MuteRole.Value) : null;
                    if (gld == null)
                        continue;

                    var done = new List<AdaModAction>();
                    foreach (var ma in kvp.Value.ModActions)
                    {
                        if (ma.Until <= now)
                        {
                            if (ma.ActionType == AdaModActionType.Mute && mrl != null)
                            {
                                var usr = gld.GetUser(ma.UserId);
                                if (usr == null)
                                    continue;

                                usr.RemoveRolesAsync(mrl).GetAwaiter().GetResult();
                                done.Add(ma);
                            }
                            else if (ma.ActionType == AdaModActionType.HardBan)
                            {
                                var ban = gld.GetBansAsync().GetAwaiter().GetResult().FirstOrDefault(xban => xban.User.Id == ma.UserId);
                                if (ban == null)
                                    continue;

                                gld.RemoveBanAsync(ma.UserId).GetAwaiter().GetResult();
                                done.Add(ma);
                            }
                        }
                    }

                    foreach (var ma in done)
                        kvp.Value.ModActions.Remove(ma);

                    AdaBotCore.ConfigManager.SetGuildConfig(kvp.Key, kvp.Value);
                }
            }

            if (this.CurrentUser.Game == null || this.CurrentUser.Game.Value.Name != this.Game)
                this.DiscordClient.SetGame(this.Game).GetAwaiter().GetResult();
            L.W("ADA DSC BH", "Ticked banhammer");
        }

        #region Embeds
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
        #endregion
    }
}
