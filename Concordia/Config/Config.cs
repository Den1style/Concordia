using Newtonsoft.Json;

namespace Concordia.Config
{
    public class Config
    {
        [JsonProperty("ownerID")]
        public string OwnerID { get; internal set; }

        [JsonProperty("botEmail")]
        public string BotEmail { get; internal set; }

        [JsonProperty("botPass")]
        public string BotPass { get; internal set; }

        [JsonProperty("commandPrefix")]
        public char CommandPrefix { get; internal set; } = '!';
    }
}
