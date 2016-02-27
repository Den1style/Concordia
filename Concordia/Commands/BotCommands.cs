using System.Collections.Concurrent;

namespace Concordia.Commands
{
    public enum Command { Kick, Join, Leave, Say }

    class BotCommands
    {
        ConcurrentDictionary<string, Command> _commands = new ConcurrentDictionary<string, Command>();


        public BotCommands()
        {
            //Get your commands from somewhere and add them to _commands
            _commands.TryAdd("kick", Command.Kick);
            _commands.TryAdd("say", Command.Say);
        }

        public Command GetCommand(string commandText)
        {
            Command c;
            _commands.TryGetValue(commandText, out c);
            return c;
        }

        //public void AddCommand(Command command, string commandickd)
        //{
        //    _commands.TryAdd()
        //}
    }
}
