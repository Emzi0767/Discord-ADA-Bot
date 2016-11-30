using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Builders;
using Discord.WebSocket;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Net.Discord.AdaBot.Commands.Permissions;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.AdaBot.Commands
{
    /// <summary>
    /// Handles all commands.
    /// </summary>
    public class AdaCommandManager
    {
        private Dictionary<string, AdaCommand> RegisteredCommands { get; set; }
        private Dictionary<string, IAdaPermissionChecker> RegisteredCheckers { get; set; }
        public int CommandCount { get { return this.RegisteredCommands.Count; } }
        public int CheckerCount { get { return this.RegisteredCheckers.Count; } }
        public string Prefix { get; private set; }

        /// <summary>
        /// Initializes the command handler.
        /// </summary>
        internal void Initialize()
        {
            L.W("ADA CMD", "Initializing commands");
            this.RegisterCheckers();
            this.RegisterCommands();
            this.InitCommands();
            L.W("ADA CMD", "Initialized");
        }

        /// <summary>
        /// Gets all registered commands.
        /// </summary>
        /// <returns>All registered commands.</returns>
        public IEnumerable<AdaCommand> GetCommands()
        {
            foreach (var cmd in this.RegisteredCommands)
                yield return cmd.Value;
        }

        internal AdaCommand GetCommand(string name)
        {
            if (this.RegisteredCommands.ContainsKey(name))
                return this.RegisteredCommands[name];
            return null;
        }

        private void RegisterCheckers()
        {
            L.W("ADA CMD", "Registering permission checkers");
            this.RegisteredCheckers = new Dictionary<string, IAdaPermissionChecker>();
            var @as = AppDomain.CurrentDomain.GetAssemblies();
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var ct = typeof(IAdaPermissionChecker);
            foreach (var t in ts)
            {
                if (!ct.IsAssignableFrom(t) || !t.IsClass || t.IsAbstract)
                    continue;

                var ipc = (IAdaPermissionChecker)Activator.CreateInstance(t);
                this.RegisteredCheckers.Add(ipc.Id, ipc);
                L.W("ADA CMD", "Registered checker '{0}' for type {1}", ipc.Id, t.ToString());
            }
            L.W("ADA CMD", "Registered {0:#,##0} checkers", this.RegisteredCheckers.Count);
        }

        private void RegisterCommands()
        {
            L.W("ADA CMD", "Registering commands");
            this.RegisteredCommands = new Dictionary<string, AdaCommand>();
            var @as = AppDomain.CurrentDomain.GetAssemblies();
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var ht = typeof(IAdaCommandModule);
            var ct = typeof(AdaCommandAttribute);
            foreach (var t in ts)
            {
                if (!ht.IsAssignableFrom(t) || !t.IsClass || t.IsAbstract)
                    continue;
                
                var ch = (IAdaCommandModule)Activator.CreateInstance(t);
                L.W("ADA CMD", "Found module handler '{0}' in type {1}", ch.Name, t.ToString());
                foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    var xct = (AdaCommandAttribute)Attribute.GetCustomAttribute(m, ct);
                    if (xct == null)
                        continue;

                    var cmd = new AdaCommand(xct.Name, xct.Aliases != null ? xct.Aliases.Split(';') : new string[] { }, xct.Description, xct.CheckPermissions && this.RegisteredCheckers.ContainsKey(xct.CheckerId) ? this.RegisteredCheckers[xct.CheckerId] : null, m, ch, xct.RequiredPermission);
                    this.RegisteredCommands.Add(cmd.Name, cmd);
                    L.W("ADA CMD", "Registered command '{0}' for handler '{1}'", cmd.Name, ch.Name);
                }
                L.W("ADA CMD", "Registered command module '{0}' for type {1}", ch.Name, t.ToString());
            }
            L.W("ADA CMD", "Registered {0:#,##0} commands", this.RegisteredCommands.Count);
        }

        private void InitCommands()
        {


            L.W("ADA CMD", "Registering command handler");
            AdaBotCore.AdaClient.DiscordClient.MessageReceived += HandleCommand;
            L.W("ADA CMD", "Done");
        }

        private async Task HandleCommand(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null)
                return;

            var client = AdaBotCore.AdaClient.DiscordClient;
            var argpos = 0;
            var cprefix = '/';
            if (client.CurrentUser.Id != 207900508562653186u)
                cprefix = '?';
            this.Prefix = cprefix.ToString();

            if (msg.HasCharPrefix(cprefix, ref argpos) || msg.HasMentionPrefix(client.CurrentUser, ref argpos))
            {
                var cmdn = msg.Content.Substring(argpos);
                var argi = cmdn.IndexOf(' ');
                if (argi == -1)
                    argi = cmdn.Length;
                var args = cmdn.Substring(argi).Trim();
                cmdn = cmdn.Substring(0, argi);
                var cmd = this.GetCommand(cmdn);
                if (cmd == null)
                    return;

                var ctx = new AdaCommandContext();
                ctx.Message = msg;
                ctx.Command = cmd;
                ctx.RawArguments = this.ParseArgumentList(args);
                try
                {
                    await cmd.Execute(ctx);
                    this.CommandExecuted(ctx);
                }
                catch (Exception ex)
                {
                    this.CommandError(new AdaCommandErrorContext { Context = ctx, Exception = ex });
                }
            }
        }

        private void CommandError(AdaCommandErrorContext ctxe)
        {
            var ctx = ctxe.Context;
            L.W("DSC CMD", "User '{0}#{1}' failed to execute command '{2}' in guild '{3}' ({4}); reason: {5} ({6})", ctx.User.Username, ctx.User.Discriminator, ctx.Command != null ? ctx.Command.Name : "<unknown>", ctx.Guild.Name, ctx.Guild.IconId, ctxe.Exception != null ? ctxe.Exception.GetType().ToString() : "<unknown exception type>", ctxe.Exception != null ? ctxe.Exception.Message : "N/A");
            if (ctxe.Exception != null)
                L.X("DSC CMD", ctxe.Exception);
            
            var embed = new EmbedBuilder();
            embed.Title = "Error executing command";
            embed.Description = string.Format("User {0} failed to execute command **{1}**.", ctx.User.Mention, ctx.Command != null ? ctx.Command.Name : "<unknown>");
            embed.Author = new EmbedAuthorBuilder();
            embed.Author.IconUrl = AdaBotCore.AdaClient.DiscordClient.CurrentUser.AvatarUrl;
            embed.Author.Name = "ADA, a bot by Emzi0767";
            embed.Color = new Color(255, 127, 0);

            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Reason";
                x.Value = ctxe.Exception != null ? ctxe.Exception.Message : "<unknown>";
            });

            if (ctxe.Exception != null)
            {
                embed.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Exception details";
                    x.Value = string.Format("**{0}**: {1}", ctxe.Exception.GetType().ToString(), ctxe.Exception.Message);
                });
            }

            ctx.Channel.SendMessageAsync("", false, embed).Wait();
        }

        private void CommandExecuted(AdaCommandContext ctx)
        {
            L.W("DSC CMD", "User '{0}#{1}' executed command '{2}' on server '{3}' ({4})", ctx.User.Username, ctx.User.Discriminator, ctx.Command.Name, ctx.Guild.Name, ctx.Guild.Id);
        }

        private IReadOnlyList<string> ParseArgumentList(string argstring)
        {
            if (string.IsNullOrWhiteSpace(argstring))
                return new List<string>().AsReadOnly();

            var arglist = new List<string>();
            var argsraw = argstring.Split(' ');
            var sb = new StringBuilder();
            var building_arg = false;
            foreach (var argraw in argsraw)
            {
                if (!building_arg && !argraw.StartsWith("\""))
                    arglist.Add(argraw);
                else if (!building_arg && argraw.StartsWith("\"") && argraw.EndsWith("\""))
                    arglist.Add(argraw.Substring(1, argraw.Length - 2));
                else if (!building_arg && argraw.StartsWith("\"") && !argraw.EndsWith("\""))
                {
                    building_arg = true;
                    sb.Append(argraw.Substring(1)).Append(' ');
                }
                else if (building_arg && !argraw.EndsWith("\""))
                    sb.Append(argraw).Append(' ');
                else if (building_arg && argraw.EndsWith("\"") && !argraw.EndsWith("\\\""))
                {
                    sb.Append(argraw.Substring(0, argraw.Length - 1));
                    arglist.Add(sb.ToString());
                    building_arg = false;
                    sb = new StringBuilder();
                }
                else if (building_arg && argraw.EndsWith("\\\""))
                    sb.Append(argraw.Remove(argraw.Length - 2, 1)).Append(' ');
            }

            return arglist.AsReadOnly();
        }

        // Below is an obsolete code block
        // A block upon which the gods looked
        // and smiled
        //
        // And then they looked at the new code block
        // and saw that it was an abomination
        // 
        // May the gods forgive me
        // For I have sinned
        // I made my own command service implementation
        /*private void InitCommands()
        {
            L.W("ADA CMD", "Initializing all commands");
            var cscb = new CommandServiceConfigBuilder();
            cscb.PrefixChar = '/';
            cscb.HelpMode = HelpMode.Public;
            cscb.ErrorHandler = new EventHandler<CommandErrorEventArgs>(Cmds_Error);
            AdaBotCore.AdaClient.DiscordClient.UsingCommands(cscb.Build());
            var cmds = AdaBotCore.AdaClient.DiscordClient.GetService<CommandService>();
            cmds.CommandExecuted += Cmds_CommandExecuted;
            cmds.CommandErrored += Cmds_CommandErrored;
            foreach (var cmd in this.GetCommands())
            {
                var xcmd = cmds.CreateCommand(cmd.Name);
                xcmd.Description(cmd.Description);
                if (cmd.Checker != null)
                    xcmd.AddCheck(cmd.Checker);
                if (cmd.Aliases.Count > 0)
                    xcmd.Alias(cmd.Aliases.ToArray());
                xcmd.Parameter("param", ParameterType.Unparsed);

                var mtd = cmd.Method;
                var mtp = Expression.Parameter(typeof(CommandEventArgs));
                var fn = Expression.Lambda<Func<CommandEventArgs, Task>>(Expression.Call(null, mtd, mtp), mtp).Compile();
                xcmd.Do(fn);

                cmd.Command = cmds.AllCommands.FirstOrDefault(xcm => xcm.Text == cmd.Name);
            }
            L.W("ADA CMD", "Done");
        }

        private void Cmds_CommandErrored(object sender, CommandErrorEventArgs e)
        {
            L.W("DSC CMD", "User '{0}' failed to execute command '{1}' on server '{2}' ({3}); reason: {4} ({5})", e.User.Name, e.Command != null ? e.Command.Text : "<unknown>", e.Server.Name, e.Server.Id, e.Exception != null ? e.Exception.GetType().ToString() : e.ErrorType.ToString(), e.Exception != null ? e.Exception.Message : "N/A");
            if (e.Exception != null)
                L.X("DSC CMD", e.Exception);
            e.Channel.SendMessage(string.Format("**ADA**: {0} failed to execute '{1}', reason: *{2}*", e.User.Mention, e.Command != null ? e.Command.Text : "<unknown>", e.Exception != null ? e.Exception.Message : "N/A")).Wait();
        }

        private void Cmds_CommandExecuted(object sender, CommandEventArgs e)
        {
            L.W("DSC CMD", "User '{0}' executed command '{1}' on server '{2}' ({3})", e.User.Name, e.Command.Text, e.Server.Name, e.Server.Id);
        }

        private void Cmds_Error(object sender, CommandEventArgs e)
        {
            L.W("DSC CMD", "User '{0}' failed to execute command '{1}' on server '{2}' ({3})", e.User.Name, e.Command.Text, e.Server.Name, e.Server.Id);
        }*/
    }
}
