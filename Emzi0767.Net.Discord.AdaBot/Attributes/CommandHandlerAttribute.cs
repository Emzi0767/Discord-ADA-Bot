using System;

namespace Emzi0767.Net.Discord.AdaBot.Attributes
{
    /// <summary>
    /// Indicates that this class contains commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandHandlerAttribute : Attribute
    {
    }
}
