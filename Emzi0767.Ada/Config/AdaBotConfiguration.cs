using Newtonsoft.Json;

namespace Emzi0767.Ada.Config
{
    public sealed class AdaBotConfiguration
    {
        [JsonProperty("token")]
        internal string Token { get; private set; }

        [JsonProperty("shard_count")]
        public int ShardCount { get; private set; }

        [JsonProperty("game")]
        public string Game { get; private set; }

        [JsonProperty("image_path")]
        public string ImagePath { get; private set; }

        [JsonProperty("postgresql")]
        internal AdaPostgresConfiguration PostgreSQL { get; private set; }
    }
}
