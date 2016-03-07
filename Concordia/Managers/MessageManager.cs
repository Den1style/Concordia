using Concordia.Entities;
using DiscordSharp.Events;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading;
using System;

namespace Concordia.Managers
{
    class MessageManager
    {
        ConcurrentQueue<DiscordMessageEventArgs> _messageQ;
        readonly static MessageManager _instance = new MessageManager();
        bool _killWorkerThreads = false;

        public static MessageManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public MessageManager()
        {
            _messageQ = new ConcurrentQueue<DiscordMessageEventArgs>();
            //build thread worker pool or task worker pool
            int workerCount = 5;//default
            int.TryParse(ConfigurationManager.AppSettings["MessageManager"], out workerCount);

            for (int i = 0; i < workerCount; i++)
            {
                Thread t = new Thread(MessageQWorker);
                t.Name = "UserBotMessageWorker" + i;
                t.IsBackground = true;
                t.Start();
            }

            RegisterManagers();
        }

        public void AddMessageToQue(DiscordMessageEventArgs command)
        {
            _messageQ.Enqueue(command);
        }

        private void RegisterManagers()
        {
            AdminManager.Instance.Init();
            SearchManager.Instance.Init();
            JokeManager.Instance.Init();
            MusicManager.Instance.Init();
        }

        private void MessageQWorker()
        {
            while (!_killWorkerThreads)
            {
                DiscordMessageEventArgs message;
                _messageQ.TryDequeue(out message);

                if (message != null)
                {
                    ParseMessage(message);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void ParseMessage(DiscordMessageEventArgs message)
        {
            //Clean check
            if (message.message.content.Length <= 1) { return; }
            if (!message.message.content.StartsWith(Concordia.config.CommandPrefix)) { return; }

            string[] stuff = message.message.content.Split(' ');
            string command = stuff[0].Substring(Concordia.config.CommandPrefix.Length);

            //get command
            var botCommand = CommandManager.GetCommand(command);

            if (botCommand != null)
            {
                DiscordUserMessage dMessage = new DiscordUserMessage();
                dMessage.CommandText = message.message.content;

                string[] commandParams = new string[stuff.Length - 1];

                for (int i = 1; i < stuff.Length; i++)
                {
                    commandParams[i - 1] = stuff[i];
                }

                dMessage.CommandParams = commandParams;
                dMessage.Arguments = string.Join(" ", commandParams);
                dMessage.Message = message;
                botCommand.userMessage = dMessage;
                RouteCommand(botCommand);
            }
            else//command is null
            {
                Concordia.client.SendMessageToChannel("Something went horribly wrong in the MessageManager.", message.message.Channel());
            }
        }


        private void RouteCommand(BotCommand message)
        {
            //route command
            message.manager.AddMessageToManager(message);
        }
    }
}
