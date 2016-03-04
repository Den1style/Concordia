using Concordia.Managers.Interfaces;
using System;

namespace Concordia.Entities
{
    class BotCommand
    {
        public string commandText;
        public IManager manager;
        public Action<object> managerAction;
        public DiscordUserMessage userMessage;

        public BotCommand(string command, IManager manager, Action<object> action)
        {
            this.commandText = command;
            this.manager = manager;
            this.managerAction = action;
        }
    }
}