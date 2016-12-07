using System;

namespace Emzi0767.Ada.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class AdaArgumentParameterAttribute : Attribute
    {
        /// <summary>
        /// Gets the parameter's description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets whether the parameter is required.
        /// </summary>
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Defines a new command parameter.
        /// </summary>
        /// <param name="description">Parameter's description.</param>
        /// <param name="required">Whether or not the parameter is required.</param>
        public AdaArgumentParameterAttribute(string description, bool required)
        {
            this.Description = description;
            this.IsRequired = required;
        }
    }
}
