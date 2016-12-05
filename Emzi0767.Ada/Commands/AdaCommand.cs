using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Emzi0767.Ada.Commands.Permissions;

#pragma warning disable
namespace Emzi0767.Ada.Commands
{
    /// <summary>
    /// Represents a command.
    /// </summary>
    public sealed class AdaCommand
    {
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the aliases of the command.
        /// </summary>
        public ReadOnlyCollection<string> Aliases { get; private set; }

        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the command's parameter definitions.
        /// </summary>
        public ReadOnlyCollection<AdaCommandParameter> Parameters { get; private set; }

        /// <summary>
        /// Gets the permission checker for the command.
        /// </summary>
        internal IAdaPermissionChecker Checker { get; private set; }

        /// <summary>
        /// Gets the method executed when the command is called.
        /// </summary>
        internal MethodInfo Method { get; private set; }

        /// <summary>
        /// Gets the function to execute when this command is executed.
        /// </summary>
        internal Func<AdaCommandContext, Task> Function { get; private set; }

        /// <summary>
        /// Gets the command's registering module.
        /// </summary>
        internal IAdaCommandModule Module { get; private set; }

        /// <summary>
        /// Gets or sets the required permissions.
        /// </summary>
        internal AdaPermission RequiredPermission { get; set; }

        /// <summary>
        /// Creates a new instance of a command.
        /// </summary>
        /// <param name="name">Name of the command.</param>
        /// <param name="aliases">Aliases of the command.</param>
        /// <param name="description">Command's description.</param>
        /// <param name="checker">Command's permissions checker.</param>
        /// <param name="method">Method executed when command is called.</param>
        /// <param name="handler">Command's registering handler.</param>
        /// <param name="permission">Command's required permission.</param>
        public AdaCommand(string name, string[] aliases, string description, IAdaPermissionChecker checker, MethodInfo method, IAdaCommandModule module, AdaPermission permission, IList<AdaCommandParameter> @params)
        {
            this.Name = name;
            this.Aliases = new ReadOnlyCollection<string>(aliases);
            this.Description = description;
            this.Checker = checker;
            this.Method = method;
            this.Module = module;
            this.RequiredPermission = permission;
            this.Parameters = new ReadOnlyCollection<AdaCommandParameter>(@params);

            var mtd = method;
            var mtp = Expression.Parameter(typeof(AdaCommandContext));
            var fn = Expression.Lambda<Func<AdaCommandContext, Task>>(Expression.Call(Expression.Constant(module), mtd, mtp), mtp).Compile();
            this.Function = fn;
        }

        internal async Task Execute(AdaCommandContext context)
        {
            var error = (string)null;
            var canrun = false;
            if (this.Checker == null)
                canrun = true;
            else
                canrun = this.Checker.CanRun(this, context.User, context.Message, context.Channel, context.Guild, out error);
            if (canrun)
                await this.Function(context);
            else
                throw new UnauthorizedAccessException(error);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AdaCommand))
                return false;
            return this == (obj as AdaCommand);
        }

        public static bool operator ==(AdaCommand cmd1, AdaCommand cmd2)
        {
            var ocmd1 = (object)cmd1;
            var ocmd2 = (object)cmd2;
            if (ocmd1 == null && ocmd2 != null)
                return false;
            if (ocmd1 != null && ocmd2 == null)
                return false;
            if (ocmd1 == null && ocmd2 == null)
                return true;

            return cmd1.Name == cmd2.Name && cmd1.Module.Name == cmd2.Module.Name;
        }

        public static bool operator !=(AdaCommand cmd1, AdaCommand cmd2)
        {
            return !(cmd1 == cmd2);
        }
    }
}
