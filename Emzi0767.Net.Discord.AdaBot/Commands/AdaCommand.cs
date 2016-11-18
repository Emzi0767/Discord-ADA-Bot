using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Discord.Commands;
using Discord.Commands.Permissions;
using Emzi0767.Net.Discord.AdaBot.Core;

namespace Emzi0767.Net.Discord.AdaBot.Commands
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
        /// Gets the permission checker for the command.
        /// </summary>
        internal IPermissionChecker Checker { get; private set; }

        /// <summary>
        /// Gets the method executed when the command is called.
        /// </summary>
        internal MethodInfo Method { get; private set; }

        /// <summary>
        /// Gets the command's registering handler.
        /// </summary>
        internal Type Handler { get; private set; }

        /// <summary>
        /// Gets or sets the Discord command object.
        /// </summary>
        internal Command Command { get; set; }

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
        public AdaCommand(string name, string[] aliases, string description, IPermissionChecker checker, MethodInfo method, Type handler, AdaPermission permission)
        {
            this.Name = name;
            this.Aliases = new ReadOnlyCollection<string>(aliases);
            this.Description = description;
            this.Checker = checker;
            this.Method = method;
            this.Handler = handler;
            this.RequiredPermission = permission;
        }
    }
}
