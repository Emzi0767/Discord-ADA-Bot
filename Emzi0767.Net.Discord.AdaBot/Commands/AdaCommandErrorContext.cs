using System;

namespace Emzi0767.Net.Discord.AdaBot.Commands
{
    public class AdaCommandErrorContext
    {
        /// <summary>
        /// Gets the context of this command's execution.
        /// </summary>
        public AdaCommandContext Context { get; internal set; }

        /// <summary>
        /// Gets the exception that occured during execution.
        /// </summary>
        public Exception Exception { get; internal set; }
    }
}
