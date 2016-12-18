using Discord.Net.WebSockets;
using Emzi0767.Ada.Core;

// Code below based on https://github.com/RogueException/Discord.Net/blob/dev/src/Discord.Net.Providers.WS4Net/WS4NetProvider.cs

namespace Emzi0767.Ada.Providers.WS4Net
{
    public class WS4NetProvider : IWebSocketProvider
    {
        public WebSocketProvider Provider { get { return this.GetInstance; } }
        private IWebSocketClient Client { get; set; }

        private IWebSocketClient GetInstance()
        {
            if (this.Client == null)
                this.Client = new WS4NetClient();

            return this.Client;
        }
    }
}
