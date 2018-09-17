// This file is part of ADA project
//
// Copyright 2018 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Emzi0767.Ada.Attributes;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.PlatformAbstractions;

namespace Emzi0767.Ada.Modules
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    [NotBlocked, NotDisabled]
    public sealed class MiscCommandsModule : BaseCommandModule
    {
        public MiscCommandsModule()
        { }

        [Command("about"), Aliases("info"), Description("Displays information about the bot.")]
        public async Task AboutAsync(CommandContext ctx)
        {
            var ccv = typeof(AdaBot)
                .GetTypeInfo()
                .Assembly
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ??

                typeof(AdaBot)
                .GetTypeInfo()
                .Assembly
                .GetName()
                .Version
                .ToString(3);

            var dsv = ctx.Client.VersionString;
            var ncv = PlatformServices.Default
                .Application
                .RuntimeFramework
                .Version
                .ToString(2);

            try
            {
                var a = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(xa => xa.GetName().Name == "System.Private.CoreLib");
                var pth = Path.GetDirectoryName(a.Location);
                pth = Path.Combine(pth, ".version");
                using (var fs = File.OpenRead(pth))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                {
                    await sr.ReadLineAsync();
                    ncv = await sr.ReadLineAsync();
                }
            }
            catch { }

            var invuri = $"https://discordapp.com/oauth2/authorize?scope=bot&permissions=0&client_id={ctx.Client.CurrentApplication.Id}";

            var embed = new DiscordEmbedBuilder
            {
                Title = "About ADA",
                Url = "https://emzi0767.com/Discord/Ada",
                Description = $"ADA is a bot made by Emzi0767#1837 (<@!181875147148361728>). The source code is available on " +
                    $"{Formatter.MaskedUrl("Emzi's GitHub", new Uri("https://github.com/Emzi0767/Discord-ADA-Bot"), "ADA's source code on GitHub")}.\n\nThis shard is currently " +
                    $"servicing {ctx.Client.Guilds.Count:#,##0} guilds.\n\nClick {Formatter.MaskedUrl("this invite link", new Uri(invuri), "ADA invite link")} to invite me to your guild!",
                Color = new DiscordColor(0x007FFF)
            };

            embed.AddField("Bot Version", $"{DiscordEmoji.FromName(ctx.Client, ":robot:")} {Formatter.Bold(ccv)}", true)
                .AddField("DSharpPlus Version", $"{DiscordEmoji.FromName(ctx.Client, ":dsharpplus:")} {Formatter.Bold(dsv)}", true)
                .AddField(".NET Core Version", $"{DiscordEmoji.FromName(ctx.Client, ":dotnet:")} {Formatter.Bold(ncv)}", true);

            await ctx.RespondAsync("", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("uptime"), Description("Display bot's uptime.")]
        public async Task UptimeAsync(CommandContext ctx)
        {
            var upt = DateTime.Now - Process.GetCurrentProcess().StartTime;
            var ups = upt.Humanize(precision: 2, maxUnit: TimeUnit.Week);
            await ctx.RespondAsync($"\u200b{DiscordEmoji.FromName(ctx.Client, ":stopwatch:")} The bot has been running for {Formatter.Bold(ups)}.").ConfigureAwait(false);
        }

        [Command("ping"), Description("Displays this shard's WebSocket latency.")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"\u200b{DiscordEmoji.FromName(ctx.Client, ":ping_pong:")} WebSocket latency: {ctx.Client.Ping:#,##0}ms.").ConfigureAwait(false);
        }

        [Command("cleanup")]
        public async Task CleanupAsync(CommandContext ctx, [Description("Maximum number of messages to clean up.")] int max_count = 100)
        {
            var lid = 0ul;
            for (var i = 0; i < max_count; i += 100)
            {
                var msgs = await ctx.Channel.GetMessagesBeforeAsync(lid != 0 ? lid : ctx.Message.Id, Math.Min(max_count - i, 100)).ConfigureAwait(false);
                var msgsf = msgs.Where(xm => xm.Author.Id == ctx.Client.CurrentUser.Id).OrderBy(xm => xm.Id);

                var lmsg = msgsf.FirstOrDefault();
                if (lmsg == null)
                    break;

                lid = lmsg.Id;

                try
                {
                    await ctx.Channel.DeleteMessagesAsync(msgsf).ConfigureAwait(false);
                }
                catch (UnauthorizedException)
                {
                    foreach (var xmsg in msgsf)
                        await xmsg.DeleteAsync();
                }
            }

            var msg = await ctx.RespondAsync(DiscordEmoji.FromName(ctx.Client, ":msokhand:").ToString()).ConfigureAwait(false);
            await Task.Delay(2500).ContinueWith(t => msg.DeleteAsync());
        }
    }
}
