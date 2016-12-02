using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Commands.Permissions;
using Emzi0767.Net.Discord.AdaBot.Attributes;
using Emzi0767.Tools.MicroLogger;

namespace Emzi0767.Net.Discord.AdaBot.Commands
{
    /// <summary>
    /// Handles all commands.
    /// </summary>
    public class AdaCommandHandler
    {
        private Dictionary<string, AdaCommand> RegisteredCommands { get; set; }
        private Dictionary<string, IPermissionChecker> RegisteredCheckers { get; set; }
        public int CommandCount { get { return this.GetCommands().Count(); } }
        public int CheckerCount { get { return this.RegisteredCheckers.Count; } }

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
            foreach (var cmd in this.RegisteredCommands.GroupBy(xkvp => xkvp.Value))
                yield return cmd.Key;
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
            this.RegisteredCheckers = new Dictionary<string, IPermissionChecker>();
            var @as = AppDomain.CurrentDomain.GetAssemblies();
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var ct = typeof(IPermissionChecker);
            var it = typeof(CheckerAttribute);
            foreach (var t in ts)
            {
                if (!ct.IsAssignableFrom(t) || !t.IsClass || t.IsAbstract)
                    continue;

                var xit = (CheckerAttribute)Attribute.GetCustomAttribute(t, it);
                if (xit == null)
                    continue;

                var ipc = (IPermissionChecker)Activator.CreateInstance(t);
                this.RegisteredCheckers.Add(xit.Id, ipc);
                L.W("ADA CMD", "Registered checker {0} for type {1}", xit.Id, t.ToString());
            }
            L.W("ADA CMD", "Registered {0:#,##0} checkers", this.RegisteredCheckers.Count);
        }

        private void RegisterCommands()
        {
            L.W("ADA CMD", "Registering commands");
            this.RegisteredCommands = new Dictionary<string, AdaCommand>();
            var @as = AppDomain.CurrentDomain.GetAssemblies();
            var ts = @as.SelectMany(xa => xa.DefinedTypes);
            var ht = typeof(CommandHandlerAttribute);
            var ct = typeof(CommandAttribute);
            foreach (var t in ts)
            {
                var xht = (CommandHandlerAttribute)Attribute.GetCustomAttribute(t, ht);
                if (xht == null)
                    continue;

                L.W("ADA CMD", "Type {0} is a handler", t.ToString());
                foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    var xct = (CommandAttribute)Attribute.GetCustomAttribute(m, ct);
                    if (xct == null)
                        continue;

                    L.W("ADA CMD", "Method {0} in type {1} is a command", m.Name, t.ToString());
                    var aliases = xct.Aliases != null ? xct.Aliases.Split(';') : new string[] { };
                    var cmd = new AdaCommand(xct.Name, aliases, xct.Description, xct.CheckPermissions && this.RegisteredCheckers.ContainsKey(xct.CheckerId) ? this.RegisteredCheckers[xct.CheckerId] : null, m, t, xct.RequiredPermission);
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
                        L.W("ADA CMD", "Registered command '{0}' for handler '{1}'", cmd.Name, xct.Name);
                    }
                    else
                        L.W("ADA CMD", "Command name '{0}' is already registered, skipping", cmd.Name);
                }
            }
            L.W("ADA CMD", "Registered {0:#,##0} commands", this.RegisteredCommands.GroupBy(xkvp => xkvp.Value).Count());
        }

        private void InitCommands()
        {
            L.W("ADA CMD", "Initializing all commands");
            var cscb = new CommandServiceConfigBuilder();
            cscb.PrefixChar = '/';
            cscb.HelpMode = HelpMode.Disabled;
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
        }
    }
}
