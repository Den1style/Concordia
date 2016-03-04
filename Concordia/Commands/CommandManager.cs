using Concordia.Entities;
using System.Collections.Concurrent;
using System;


namespace Concordia
{
    static class CommandManager
    {
        static ConcurrentDictionary<string, BotCommand> _commands = new ConcurrentDictionary<string, BotCommand>();

        public static BotCommand GetCommand(string command)
        {
            BotCommand c;
            if (_commands.TryGetValue(command, out c))
            {
                return c;
            }
            else
            {
                return null;
            }
        }

        public static void RegisterCommand(BotCommand command)
        {
            if (!_commands.TryAdd(command.commandText, command))
            {
                throw new Exception("Error trying to register command");
            }
        }
    }
}