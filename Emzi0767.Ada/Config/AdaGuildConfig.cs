using System.Collections.Generic;

namespace Emzi0767.Ada.Config
{
    public class AdaGuildConfig
    {
        public ulong? ModLogChannel { get; internal set; }
        public bool? DeleteCommands { get; internal set; }
        public string CommandPrefix { get; internal set; }
        public ulong? MuteRole { get; internal set; }
        internal List<AdaModAction> ModActions { get; set; }

        public AdaGuildConfig()
        {
            this.ModActions = new List<AdaModAction>();
        }
    }
}
