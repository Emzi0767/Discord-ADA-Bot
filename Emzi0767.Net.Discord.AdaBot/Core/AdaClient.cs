using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Emzi0767.Tools.MicroLogger;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.AdaBot.Core
{
    public sealed class AdaClient
    {
        public IUser CurrentUser { get { return this.DiscordClient.CurrentUser; } }
        internal DiscordSocketClient DiscordClient { get; private set; }
        internal JObject ConfigJson { get; private set; }
        private Timer BanHammer { get; set; }
        private string Token { get; set; }

        internal AdaClient()
        {
            L.W("ADA DSC", "Initializing Discord");
            var dsc = new DiscordSocketConfig()
            {
                LogLevel = Debugger.IsAttached ? LogSeverity.Debug : LogSeverity.Info,
                AudioMode = AudioMode.Disabled
            };
            
            this.DiscordClient = new DiscordSocketClient(dsc);
            this.DiscordClient.Log += Client_Log;
            this.DiscordClient.Ready += Client_Ready;

            var a = Assembly.GetExecutingAssembly();
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

        internal void Initialize()
        {
            L.W("ADA DSC", "Connecting");
            this.DiscordClient.LoginAsync(TokenType.Bot, this.Token).Wait();
            this.DiscordClient.ConnectAsync(true).Wait();
            L.W("ADA DSC", "Connected as '{0}'", this.DiscordClient.CurrentUser.Username);
        }

        internal void Deinitialize()
        {
            L.W("ADA DSC", "Disconnecting");
            this.DiscordClient.DisconnectAsync().Wait();
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
                channel.SendMessageAsync(ms).Wait();
        }

        public void SendEmbed(EmbedBuilder embed, SocketTextChannel channel)
        {
            channel.SendMessageAsync("", false, embed).Wait();
        }

        internal void WriteConfig()
        {
            var a = Assembly.GetExecutingAssembly();
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
            this.BanHammer = new Timer(new TimerCallback(BanHammer_Tick), null, 0, 3600000);
            return Task.CompletedTask;
        }

        private void BanHammer_Tick(object _)
        {
            this.DiscordClient.SetGame("Banhammer 40,000").Wait();
            L.W("ADA DSC BH", "Ticked banhammer");
        }
    }
}
