using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Discord;
using Emzi0767.Tools.MicroLogger;
using Newtonsoft.Json.Linq;

namespace Emzi0767.Net.Discord.AdaBot.Core
{
    public sealed class AdaDiscord
    {
        internal DiscordClient Client { get; private set; }
        private Timer BanHammer { get; set; }
        private string Token { get; set; }

        internal AdaDiscord()
        {
            L.W("ADA DSC", "Initializing Discord");
            var dcb = new DiscordConfigBuilder();
            dcb.LogLevel = Debugger.IsAttached ? LogSeverity.Debug : LogSeverity.Info;
            var dc = dcb.Build();

            this.Client = new DiscordClient(dc);
            this.Client.Log.Message += Log_Message;
            this.Client.Ready += Client_Ready;

            var a = Assembly.GetExecutingAssembly();
            var n = a.GetName();
            var l = Path.GetDirectoryName(a.Location);

            L.W("ADA DSC", "Loading config");
            var sp = Path.Combine(l, "config.json");
            var sjson = File.ReadAllText(sp, AdaBotCore.UTF8);
            var sjo = JObject.Parse(sjson);
            this.Token = (string)sjo["token"];
            L.W("ADA DSC", "Discord initialized");
        }

        internal void Initialize()
        {
            L.W("ADA DSC", "Connecting");
            this.Client.Connect(this.Token, TokenType.Bot).Wait();
            L.W("ADA DSC", "Connected as '{0}'", this.Client.CurrentUser.Name);
        }

        internal void Deinitialize()
        {
            L.W("ADA DSC", "Disconnecting");
            this.Client.Disconnect();
            L.W("ADA DSC", "Disconnected");
        }

        /// <summary>
        /// Sends a message to a specified channel
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send the message to.</param>
        public void SendMessage(string message, ulong channel)
        {
            var ch = (Channel)null;
            while (ch == null)
                ch = this.Client.GetChannel(channel);
            this.SendMessage(message, ch);
        }

        /// <summary>
        /// Sends a message to a specified channel
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="channel">Channel to send the message to.</param>
        internal void SendMessage(string message, Channel channel)
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
                channel.SendMessage(ms).Wait();
        }

        private void Log_Message(object sender, LogMessageEventArgs e)
        {
            L.W("DISCORD", "{0}/{1}: {2}", e.Severity, e.Source, e.Message);
            if (e.Exception != null)
                L.X("DISCORD", e.Exception);
        }

        private void Client_Ready(object sender, EventArgs e)
        {
            this.BanHammer = new Timer(new TimerCallback(BanHammer_Tick), null, 0, 3600000);
        }

        private void BanHammer_Tick(object _)
        {
            this.Client.SetGame("Banhammer 40,000");
            L.W("ADA DSC BH", "Ticked banhammer");
        }
    }
}
