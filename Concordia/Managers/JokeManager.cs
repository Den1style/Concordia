
using Concordia.Entities;
using Concordia.Managers.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Net;
using System.Threading;

namespace Concordia.Managers
{
    class JokeManager : IManager
    {
        ConcurrentQueue<BotCommand> _messageQ;
        readonly static JokeManager _instance = new JokeManager();
        bool _killWorkerThreads = false;

        public static JokeManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Init()
        {
            _messageQ = new ConcurrentQueue<BotCommand>();

            int workerCount = 3;//default
            //int.TryParse(ConfigurationManager.AppSettings["JokeManagerWorkers"], out workerCount);

            RegisterCommands();

            for (int i = 0; i < workerCount; i++)
            {
                Thread t = new Thread(MessageQWorker);
                t.Name = "JokeManagerWorker" + i;
                t.IsBackground = true;
                t.Start();
            }
        }

        public void AddMessageToManager(BotCommand command)
        {
            _messageQ.Enqueue(command);
        }

        private void RegisterCommands()
        {
            CommandManager.RegisterCommand(new BotCommand("chuck", this, (object x) => { Chuck(x); }));
            CommandManager.RegisterCommand(new BotCommand("mama", this, (object x) => { Mama(x); }));
        }

        private void MessageQWorker()
        {
            while (!_killWorkerThreads)
            {
                BotCommand message;
                _messageQ.TryDequeue(out message);

                if (message != null)
                {
                    ExecuteCommand(message);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void ExecuteCommand(BotCommand command)
        {
            command.managerAction.Invoke(command);
        }

        private async void Chuck(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;

            Helper.WriteCommand(bcMessage.userMessage.CommandText);

            var headers = new WebHeaderCollection();
            var res = await SearchHelper.GetResponseAsync($"http://api.icndb.com/jokes/random");
            try
            {
                var items = JObject.Parse(res);
                bcMessage.userMessage.Message.Channel.SendMessage(items["value"]["joke"].ToString().Replace("&quot;", "\""));
            }
            catch (Exception ex)
            {
                Helper.WriteCommand(ex.ToString());
                bcMessage.userMessage.Message.Channel.SendMessage("💢 Failed finding a chuck :(");
            }
        }

        private async void Mama(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;

            Helper.WriteCommand(bcMessage.userMessage.CommandText);

            var headers = new WebHeaderCollection();
            var res = await SearchHelper.GetResponseAsync($"http://api.yomomma.info/");
            try
            {
                var items = JObject.Parse(res.Replace("&quot;", "\""));
                string joke = items["joke"].ToString();
                joke = (bcMessage.userMessage.CommandParams.Length > 0) ? joke.Replace("Yo", bcMessage.userMessage.CommandParams[0] + "'s") : joke;
                bcMessage.userMessage.Message.Channel.SendMessage(joke);
            }
            catch (Exception ex)
            {
                Helper.WriteCommand(ex.ToString());
                bcMessage.userMessage.Message.Channel.SendMessage("💢 Failed finding your mom :(");
            }
        }
    }
}
