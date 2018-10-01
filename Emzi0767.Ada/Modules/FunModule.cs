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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Emzi0767.Ada.Attributes;
using Emzi0767.Ada.Services;
using Markov;

namespace Emzi0767.Ada.Modules
{
    [Group("fun")]
    [Description("Various fun commands, such as random number generators, markov chain generators, etc.")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [NotBlacklisted, NotDisabled]
    public sealed class FunModule : BaseCommandModule
    {
        private static Regex DiceRegex { get; } = new Regex(@"^(?<count>\d+)?d(?<sides>\d+)$", RegexOptions.Compiled | RegexOptions.ECMAScript);

        private CSPRNG RNG { get; }

        public FunModule(CSPRNG rng)
        {
            this.RNG = rng;
        }

        [Command("random"), Description("Generates a random number between specified bounds (lower inclusive)."), NotBlacklisted, NotDisabled]
        public async Task RandomAsync(CommandContext ctx, 
            [Description("Minimum value to generate (inclusive). 0 by default.")] int min = 0, 
            [Description("Maximum value to generate (exclusive). 32,768 by default.")] int max = 32768, 
            [Description("Number of numbers to generate. 1 by default.")] int count = 1)
        {
            if (max <= min)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} Maximum value needs to be greater than minimum value.").ConfigureAwait(false);
                return;
            }

            if (count <= 0)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to specify a value greater than 0 for number count.").ConfigureAwait(false);
                return;
            }

            if (count >= 100)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You can generate a 100 numbers at once.").ConfigureAwait(false);
                return;
            }

            if (count == 1)
            {
                var num = this.RNG.Next(min, max);
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} Your random number: {num}").ConfigureAwait(false);
            }
            else
            {
                var nums = string.Join(", ", Enumerable.Range(0, count).Select(x => this.RNG.Next(min, max)));
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} Your random numbers: {nums}").ConfigureAwait(false);
            }
        }

        [Command("dice"), Description("Roll dice!"), NotBlacklisted, NotDisabled]
        public async Task DiceAsync(CommandContext ctx, 
            [Description("Number of sides in a die. Minimum of 2, default of 6.")] int sides = 6, 
            [Description("Number of dice to roll. Minimum and default of 1.")] int count = 1)
        {
            if (sides < 2)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} A die needs to have a minimum of 2 sides.").ConfigureAwait(false);
                return;
            }

            if (count <= 0)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to roll at least one die.").ConfigureAwait(false);
                return;
            }

            if (count >= 100)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You can roll a 100 dice at once.").ConfigureAwait(false);
                return;
            }

            if (count == 1)
            {
                int die = this.RNG.Next(1, sides + 1);
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {die}").ConfigureAwait(false);
            }
            else
            {
                var dice = string.Join(" ", Enumerable.Range(1, count).Select(x => this.RNG.Next(1, sides + 1)));
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {dice}").ConfigureAwait(false);
            }
        }

        [Command("dice")]
        public async Task DiceAsync(CommandContext ctx,
            [Description("Dies to roll, in xdy format (e.g. 2d20 or d6). Default of 1d6.")] string dice = "1d6")
        {
            if (string.IsNullOrWhiteSpace(dice))
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to specify dice to roll.").ConfigureAwait(false);
                return;
            }

            var m = DiceRegex.Match(dice);
            if (!m.Success)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to specify valid dice.").ConfigureAwait(false);
                return;
            }

            if (!m.Groups["count"].Success)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} Invalid dice count.").ConfigureAwait(false);
                return;
            }

            if (!int.TryParse(m.Groups["count"].Value, out var count))
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} Invalid dice count.").ConfigureAwait(false);
                return;
            }

            if (!int.TryParse(m.Groups["sides"].Value, out var sides))
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} Invalid side count.").ConfigureAwait(false);
                return;
            }

            await this.DiceAsync(ctx, sides, count).ConfigureAwait(false);
        }

        [Command("choice"), Aliases("pick", "choose"), Description("Pick a random element from a list of options."), NotBlacklisted, NotDisabled]
        public async Task ChoiceAsync(CommandContext ctx,
            [Description("Options to choose from.")] params string[] options)
        {
            if (options?.Any() != true || options.Length < 2)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to specify at least 2 options.").ConfigureAwait(false);
                return;
            }

            var opt = this.RNG.PickOne(options).Replace("@everyone", "@\u200beveryone").Replace("@here", "@\u200bhere");
            await ctx.RespondAsync($"\u200b{opt}").ConfigureAwait(false);
        }

        [Command("choicex"), Aliases("pickx", "choosex"), Description("Pick random elements from a list of options, performing the choice a specified number of times."), NotBlacklisted, NotDisabled]
        public async Task ChoicexAsync(CommandContext ctx,
            [Description("Number of times to perform random choice. Minimum of 2, maximum of 10.")] int count,
            [Description("Options to choose from.")] params string[] options)
        {
            if (options?.Any() != true || options.Length < 2)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to specify at least 2 options.").ConfigureAwait(false);
                return;
            }

            if (count < 2 || count > 10)
            {
                await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} Choice count needs to be at least 2 and at most 10.").ConfigureAwait(false);
                return;
            }

            var choices = Enumerable.Range(1, count)
                .Select(x => (index: x, choice: this.RNG.PickOne(options).Replace("@everyone", "@\u200beveryone").Replace("@here", "@\u200bhere")))
                .ToList();
            var sb = new StringBuilder();
            foreach (var (index, choice) in choices)
                sb.AppendLine($"Choice {index}: {choice}");

            var top = choices.Select(x => x.choice)
                .GroupBy(x => x)
                .GroupBy(xg => xg.Count())
                .OrderByDescending(xg => xg.Key)
                .First();
            var topc = top.First().First();

            if (top.Count() > 1)
            {
                // tie-breaker
                var tie = top.Select(xg => xg.Key);
                topc = tie.ElementAt(this.RNG.Next(0, tie.Count()));
                sb.AppendLine($"Tie-breaker: {topc}");
            }

            sb.AppendLine().Append($"Result: {topc}");
            var result = sb.ToString().Trim().Replace("\r\n", "\n");
            await ctx.RespondAsync(result).ConfigureAwait(false);
        }

        [Group("markov")]
        [Description("Generates Markov chains from input data.")]
        [ModuleLifespan(ModuleLifespan.Transient)]
        [NotBlacklisted, NotDisabled]
        public sealed class MarkovModule : BaseCommandModule
        {
            private CSPRNG RNG { get; }

            public MarkovModule(CSPRNG rng)
            {
                this.RNG = rng;
            }

            [Command("channel")]
            [Aliases("chn")]
            [Description("Generates messages from up to 200 messages in specified channel.")]
            [NotBlacklisted, NotDisabled]
            public async Task ChannelAsync(CommandContext ctx, 
                [Description("Channel to generate message from. Will use current if not specified.")] DiscordChannel channel = null)
            {
                await ctx.TriggerTypingAsync().ConfigureAwait(false);

                channel = channel ?? ctx.Channel;
                if (channel.Type != ChannelType.Text)
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to specify a text channel.").ConfigureAwait(false);
                    return;
                }

                var msgs = await channel.GetMessagesAsync(200).ConfigureAwait(false);
                if (msgs.Count < 100)
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} Not enough messages available in {channel.Mention}.").ConfigureAwait(false);
                    return;
                }

                var markov = new MarkovChain<string>(1);
                foreach (var msg in msgs)
                    markov.Add(msg.Content.SplitSentence(), 1);

                var rng = new EnhancedRandom(this.RNG);
                var sentence = string.Join(" ", markov.Chain(rng));
                if (string.IsNullOrWhiteSpace(sentence))
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":mscry:")} Somehow I failed to generate a sentence.").ConfigureAwait(false);
                    return;
                }

                if (sentence.Length <= 1500)
                    await ctx.RespondAsync($"\u200b{sentence}").ConfigureAwait(false);
                else
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msfrown:")} Generated sentence was too long.").ConfigureAwait(false);
            }

            [Command("role")]
            [Description("Generates messages from up to 50 messages sent by members with specified role in specified channel.")]
            [NotBlacklisted, NotDisabled]
            public async Task RoleAsync(CommandContext ctx,
                [Description("Role to generate messages from.")] DiscordRole role,
                [Description("Channel to generate message from. Will use current if not specified.")] DiscordChannel channel = null)
            {
                await ctx.TriggerTypingAsync().ConfigureAwait(false);

                channel = channel ?? ctx.Channel;
                if (channel.Type != ChannelType.Text)
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} You need to specify a text channel.").ConfigureAwait(false);
                    return;
                }

                var msgsRaw = await channel.GetMessagesAsync(500).ConfigureAwait(false);
                var msgs = new List<DiscordMessage>();
                foreach (var msg in msgsRaw)
                    if (msg.Author is DiscordMember mbr && mbr.Roles.Contains(role))
                        msgs.Add(msg);

                if (msgs.Count < 50)
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msunamused:")} Not enough messages available in {channel.Mention}.").ConfigureAwait(false);
                    return;
                }

                var markov = new MarkovChain<string>(1);
                foreach (var msg in msgs)
                    markov.Add(msg.Content.SplitSentence(), 1);

                var rng = new EnhancedRandom(this.RNG);
                var sentence = string.Join(" ", markov.Chain(rng));
                if (string.IsNullOrWhiteSpace(sentence))
                {
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":mscry:")} Somehow I failed to generate a sentence.").ConfigureAwait(false);
                    return;
                }

                if (sentence.Length <= 1500)
                    await ctx.RespondAsync($"\u200b{sentence}").ConfigureAwait(false);
                else
                    await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":msfrown:")} Generated sentence was too long.").ConfigureAwait(false);
            }
        }
    }
}
