using Newtonsoft.Json;

namespace Emzi0767.Ada.Config
{
    public sealed class AdaPostgresConfiguration
    {
        [JsonProperty("hostname")]
        public string Hostname { get; private set; }

        [JsonProperty("port")]
        public int Port { get; private set; }

        [JsonProperty("username")]
        public string Username { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        [JsonProperty("database")]
        public string Database { get; private set; }
    }
}
