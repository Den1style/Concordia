using Concordia.Entities;
using Concordia.Managers.Interfaces;
using DiscordSharp.Objects;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading;


namespace Concordia.Managers
{
    abstract class Manager : IManager
    {
        ConcurrentQueue<BotCommand> _messageQ;
        bool _killWorkerThreads = false;

        public abstract void Init();

        public void AddMessageToManager(BotCommand command)
        {
            _messageQ.Enqueue(command);
        }

        internal void StartWorkers(string managerName)
        {
            _messageQ = new ConcurrentQueue<BotCommand>();

            int workerCount = 5;//default
            int.TryParse(ConfigurationManager.AppSettings[managerName], out workerCount);

            for (int i = 0; i < workerCount; i++)
            {
                Thread t = new Thread(MessageQWorker);
                t.Name = managerName + i;
                t.IsBackground = true;
                t.Start();
            }
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

    }
}