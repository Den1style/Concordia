using System.Collections.Concurrent;

namespace Concordia.Commands
{
    public enum Command
    {
        //Bug, ryans fixing?
        Null,

        //ADMIN COMMANDS
        Kick,
        WhoIs,
        Join,
        Leave,
        Say,
        Ban,
        Unban,

        //SEARCH COMMANDS
        UrbanDictionary,
        HashTag,

        //Music Commands
        Youtube,
        Echo
    }

    class BotCommands
    {
        ConcurrentDictionary<string, Command> _commands = new ConcurrentDictionary<string, Command>();


        public BotCommands()
        {
            //Admin Commands
            _commands.TryAdd("null", Command.Null);
            _commands.TryAdd("kick", Command.Kick);
            _commands.TryAdd("whois", Command.WhoIs);
            _commands.TryAdd("join", Command.Join);
            _commands.TryAdd("leave", Command.Leave);
            _commands.TryAdd("say", Command.Say);
            _commands.TryAdd("ban", Command.Ban);
            _commands.TryAdd("unban", Command.Unban);

            //Search Commands
            _commands.TryAdd("ud", Command.UrbanDictionary);
            _commands.TryAdd("#", Command.HashTag);

            //Music Commands
            _commands.TryAdd("youtube", Command.Youtube);
            _commands.TryAdd("echo", Command.Echo);
        }

        public Command GetCommand(string commandText)
        {
            Command c;
            _commands.TryGetValue(commandText, out c);
            return c;
        }

        public void AddCommand(Command command, string commandText)
        {
            _commands.TryAdd(commandText, command);
        }

        public void RemoveCommand(string commandText)
        {
            Command c;
            _commands.TryRemove(commandText, out c);
        }

    }
}
