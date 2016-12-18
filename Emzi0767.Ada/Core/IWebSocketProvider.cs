using Discord.Net.WebSockets;

namespace Emzi0767.Ada.Core
{
    public interface IWebSocketProvider
    {
        WebSocketProvider Provider { get; }
    }
}
