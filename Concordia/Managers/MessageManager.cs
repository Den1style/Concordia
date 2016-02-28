
using Concordia.Commands;
using Concordia.Entities;
using DiscordSharp.Events;
using System.Collections.Concurrent;
using System.Threading;

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
            //so we might want to read some settings to figure out how big to make the worker pool.
            Thread t = new Thread(MessageQWorker);
            t.Name = "UserBotMessageWorker1";
            t.IsBackground = true;
            t.Start();

            Thread t2 = new Thread(MessageQWorker);
            t2.Name = "UserBotMessageWorker2";
            t2.IsBackground = true;
            t2.Start();

            Thread t3 = new Thread(MessageQWorker);
            t3.Name = "UserBotMessageWorker3";
            t3.IsBackground = true;
            t3.Start();

            Thread t4 = new Thread(MessageQWorker);
            t4.Name = "UserBotMessageWorker4";
            t4.IsBackground = true;
            t4.Start();

            Thread t5 = new Thread(MessageQWorker);
            t5.Name = "UserBotMessageWorker5";
            t5.IsBackground = true;
            t5.Start();
        }

        public void AddMessageToQue(DiscordMessageEventArgs message)
        {
            _messageQ.Enqueue(message);
        }

        private void MessageQWorker()
        {
            while (!_killWorkerThreads)
            {
                DiscordMessageEventArgs message;
                _messageQ.TryDequeue(out message);

                if(message != null)
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
            if(message.message.content.Length <= 1) { return; }
            if (!message.message.content.StartsWith(Concordia.config.CommandPrefix)) { return; }

            BotCommands bc = new BotCommands();

            string[] stuff = message.message.content.Split(' ');
            string command = stuff[0].Substring(Concordia.config.CommandPrefix.Length);

           

            var botCommand = bc.GetCommand(command);
            if(botCommand != null)
            {
                DiscordUserMessage dMessage = new DiscordUserMessage();

                dMessage.CommandText = message.message.content;

                string[] commandParams = new string[stuff.Length - 1];

                for (int i = 1; i < stuff.Length; i++)
                {
                    commandParams[i - 1] = stuff[i];
                }

                dMessage.CommandParams = commandParams;

                dMessage.BotCommand = botCommand;
                dMessage.Message = message;
                RouteCommand(dMessage);
            }
            else//command is null
            {
                Concordia.client.SendMessageToChannel("You fucked up.", message.message.Channel());
            }
        }


        private void RouteCommand(DiscordUserMessage message)
        {
            switch (message.BotCommand)
            {
                //Admin Commands
                case Command.Kick:                   
                case Command.Say:                   
                case Command.WhoIs:
                    AdminManager.Instance.ProcessDiscordCommandMessage(message);
                    break;

                //Search Commands
                case Command.UrbanDictionary:                    
                case Command.HashTag:
                    SearchManager.Instance.ProcessDiscordCommandMessage(message);
                    break;
            }
        }
    }
}
