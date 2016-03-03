using Concordia.Commands;
using DiscordSharp.Events;

namespace Concordia.Entities
{
    class DiscordUserMessage
    {
        public Command BotCommand { get; set; }
        public string CommandText { get; set; }
        public string[] CommandParams { get; set; }
        public string Arguments { get; set; }
        public DiscordMessageEventArgs Message { get; set; }
    }
}
