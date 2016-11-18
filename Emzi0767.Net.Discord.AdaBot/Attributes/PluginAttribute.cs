using System;

namespace Emzi0767.Net.Discord.AdaBot.Attributes
{
    /// <summary>
    /// Declares this class an ADA Plugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PluginAttribute : Attribute
    {
        /// <summary>
        /// Gets the plugin's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the plugin's initializer method, contained within this class.
        /// </summary>
        public string InitializerMethod { get; set; }

        /// <summary>
        /// Declares this class an ADA Plugin.
        /// </summary>
        public PluginAttribute(string name)
        {
            this.Name = name;
        }
    }
}
