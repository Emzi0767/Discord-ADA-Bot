﻿namespace Emzi0767.Net.Discord.AdaBot.Commands
{
    /// <summary>
    /// Defines a command parameter.
    /// </summary>
    public class AdaCommandParameter
    {
        /// <summary>
        /// Gets the parameter's order of appearance.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the parameter's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the parameter's description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets whether the parameter is required.
        /// </summary>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Gets whether the parameter catches everything until the end of invocation.
        /// </summary>
        public bool IsCatchAll { get; private set; }

        /// <summary>
        /// Defines a new command parameter.
        /// </summary>
        /// <param name="order">Order of parameter's appearance.</param>
        /// <param name="name">Parameter's name.</param>
        /// <param name="description">Parameter's description.</param>
        /// <param name="required">Whether or not the parameter is required.</param>
        /// <param name="catchall">Whether or not the parameter is catchall.</param>
        internal AdaCommandParameter(int order, string name, string description, bool required, bool catchall)
        {
            this.Order = order;
            this.Name = name;
            this.Description = description;
            this.IsRequired = required;
            this.IsCatchAll = catchall;
        }
    }
}
