using System;

namespace Emzi0767.Net.Discord.AdaBot.Attributes
{
    /// <summary>
    /// Marks this checker with an ID.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CheckerAttribute : Attribute
    {
        /// <summary>
        /// Gets the ID of this checker.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Marks this checker with an ID.
        /// </summary>
        /// <param name="id">ID of the checker.</param>
        public CheckerAttribute(string id)
        {
            this.Id = id;
        }
    }
}
