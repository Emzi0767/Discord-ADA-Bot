using System;

namespace Emzi0767.Ada.Config
{
    public class AdaModAction
    {
        public ulong UserId { get; internal set; }
        public ulong Issuer { get; internal set; }
        public DateTime Until { get; internal set; }
        public DateTime Issued { get; internal set; }
        public AdaModActionType ActionType { get; internal set; }
        public string Reason { get; internal set; }

        public AdaModAction()
        {
            this.Issued = DateTime.UtcNow;
        }
    }
}
