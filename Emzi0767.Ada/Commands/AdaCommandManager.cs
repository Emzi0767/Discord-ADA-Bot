using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Emzi0767.Ada.Attributes;
using Emzi0767.Ada.Commands.Permissions;

namespace Emzi0767.Ada.Commands
{
    /// <summary>
    /// Handles all commands.
    /// </summary>
    public class AdaCommandManager
    {
        internal AdaParameterParser ParameterParser { get; private set; }
        private Dictionary<string, AdaCommand> RegisteredCommands { get; set; }
        private Dictionary<string, IAdaPermissionChecker> RegisteredCheckers { get; set; }
        public int CommandCount { get { return this.GetCommands().Count(); } }
        public int CheckerCount { get { return this.RegisteredCheckers.Count; } }

        /// <summary>
        /// Initializes the command handler.
        /// </summary>
        internal void Initialize()
        {
            L.W("ADA CMD", "Initializing commands");
            this.ParameterParser = new AdaParameterParser();
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
            foreach (var cmd in this.RegisteredCommands.GroupBy(xkvp => xkvp.Value))
                yield return cmd.Key;
        }

        public string GetPrefix(ulong guildid)
        {
            var prefix = "/";
            if (AdaBotCore.AdaClient.CurrentUser.Id != 207900508562653186u)
                prefix = "?";
            var gconf = AdaBotCore.ConfigManager.GetGuildConfig(guildid);
            if (gconf != null && gconf.CommandPrefix != null)
                prefix = gconf.CommandPrefix;

            return prefix;
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
            var @as = AdaBotCore.PluginManager.PluginAssemblies;
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var ct = typeof(IAdaPermissionChecker);
            foreach (var t in ts)
            {
                if (!ct.IsAssignableFrom(t.AsType()) || !t.IsClass || t.IsAbstract)
                    continue;

                var ipc = (IAdaPermissionChecker)Activator.CreateInstance(t.AsType());
                this.RegisteredCheckers.Add(ipc.Id, ipc);
                L.W("ADA CMD", "Registered checker '{0}' for type {1}", ipc.Id, t.ToString());
            }
            L.W("ADA CMD", "Registered {0:#,##0} checkers", this.RegisteredCheckers.Count);
        }

        private void RegisterCommands()
        {
            L.W("ADA CMD", "Registering commands");
            this.RegisteredCommands = new Dictionary<string, AdaCommand>();
            var @as = AdaBotCore.PluginManager.PluginAssemblies;
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var ht = typeof(IAdaCommandModule);
            var ct = typeof(AdaCommandAttribute);
            var pt = typeof(AdaMethodParameterAttribute);
            foreach (var t in ts)
            {
                if (!ht.IsAssignableFrom(t.AsType()) || !t.IsClass || t.IsAbstract)
                    continue;
                
                var ch = (IAdaCommandModule)Activator.CreateInstance(t.AsType());
                L.W("ADA CMD", "Found module handler '{0}' in type {1}", ch.Name, t.ToString());
                foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    var xct = m.GetCustomAttribute<AdaCommandAttribute>();
                    if (xct == null)
                        continue;

                    var prs = m.GetParameters();
                    var xps = m.GetCustomAttributes<AdaMethodParameterAttribute>().ToArray();
                    if (prs.Length > 1 && xps.Length > 0)
                    {
                        L.W("ADA CMD", "Command '{0}' has invalid parameter specification, skipping", xct.Name);
                        continue;
                    }

                    var ats = new List<AdaCommandParameter>();
                    if (xps.Length > 0)
                    {
                        foreach (var xp in xps)
                            ats.Add(new AdaCommandParameter(xp.Order, xp.Name, xp.Description, xp.IsRequired, xp.IsCatchAll, false));
                    }
                    else if (prs.Length > 1)
                    {
                        var prn = 0;
                        foreach (var prm in prs.Skip(1))
                        {
                            var pmi = prm.GetCustomAttribute<AdaArgumentParameterAttribute>();
                            var isp = prm.GetCustomAttribute<ParamArrayAttribute>();

                            var all = false; // catchall
                            if (isp != null)
                                all = true;

                            if (pmi == null)
                                pmi = new AdaArgumentParameterAttribute("UNSPECIFIED.", true);

                            ats.Add(new AdaCommandParameter(prn++, prm.Name, pmi.Description, pmi.IsRequired, all, true) { ParameterType = prm.ParameterType });
                        }
                    }
                    
                    var prms = m.GetParameters();
                    var args = new ParameterExpression[prms.Length];
                    var i = 0;
                    foreach (var prm in prms)
                        args[i++] = Expression.Parameter(prm.ParameterType, prm.Name);
                    var func = Expression.Lambda(Expression.Call(Expression.Constant(ch), m, args), args).Compile();

                    var aliases = xct.Aliases != null ? xct.Aliases.Split(';') : new string[] { };
                    var cmd = new AdaCommand(xct.Name, aliases, xct.Description, xct.CheckPermissions && this.RegisteredCheckers.ContainsKey(xct.CheckerId) ? this.RegisteredCheckers[xct.CheckerId] : null, func, ch, xct.RequiredPermission, ats);
                    var names = new string[1 + aliases.Length];
                    names[0] = cmd.Name;
                    if (aliases.Length > 0)
                        Array.Copy(aliases, 0, names, 1, aliases.Length);
                    if (!this.RegisteredCommands.ContainsKey(cmd.Name))
                    {
                        foreach (var name in names)
                        {
                            if (!this.RegisteredCommands.ContainsKey(name))
                                this.RegisteredCommands.Add(name, cmd);
                            else
                                L.W("ADA CMD", "Alias '{0}' for command '{1}' already taken, skipping", name, cmd.Name);
                        }
                        L.W("ADA CMD", "Registered command '{0}' for handler '{1}'", cmd.Name, ch.Name);
                    }
                    else
                        L.W("ADA CMD", "Command name '{0}' is already registered, skipping", cmd.Name);
                }
                L.W("ADA CMD", "Registered command module '{0}' for type {1}", ch.Name, t.ToString());
            }
            L.W("ADA CMD", "Registered {0:#,##0} commands", this.RegisteredCommands.GroupBy(xkvp => xkvp.Value).Count());
        }

        private void InitCommands()
        {
            L.W("ADA CMD", "Registering command handler");
            AdaBotCore.AdaClient.DiscordClient.MessageReceived += HandleCommand;
            L.W("ADA CMD", "Done");
        }

        private async Task HandleCommand(SocketMessage arg)
        {
            await Task.Delay(1);

            var msg = arg as SocketUserMessage;
            if (msg == null)
                return;

            var chn = msg.Channel as SocketTextChannel;
            if (chn == null)
                return;

            var gld = chn.Guild;
            if (gld == null)
                return;
            
            var client = AdaBotCore.AdaClient.DiscordClient;
            var argpos = 0;
            var gconf = AdaBotCore.ConfigManager.GetGuildConfig(gld.Id);
            var cprefix = "/";
            if (client.CurrentUser.Id != 207900508562653186u)
                cprefix = "?";
            if (gconf != null && gconf.CommandPrefix != null)
                cprefix = gconf.CommandPrefix;
            if (msg.HasStringPrefix(cprefix, ref argpos) || msg.HasMentionPrefix(client.CurrentUser, ref argpos))
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
                var t = Task.Run(async () =>
                {
                    try
                    {
                        if (gconf.DeleteCommands != null && gconf.DeleteCommands.Value)
                            await msg.DeleteAsync();
                        await cmd.Execute(ctx);
                        this.CommandExecuted(ctx);
                    }
                    catch (Exception ex)
                    {
                        this.CommandError(new AdaCommandErrorContext { Context = ctx, Exception = ex });
                    }
                });
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
            embed.Color = new Color(255, 127, 0);
            embed.ThumbnailUrl = AdaBotCore.AdaClient.CurrentUser.AvatarUrl;

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

            ctx.Channel.SendMessageAsync("", false, embed).GetAwaiter().GetResult();
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
    }
}
