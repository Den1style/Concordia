
using Concordia.Entities;
using Concordia.Managers.Interfaces;
using DiscordSharp.Objects;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading;

namespace Concordia.Managers
{
    class AdminManager : IManager
    {
        ConcurrentQueue<BotCommand> _messageQ;
        readonly static AdminManager _instance = new AdminManager();
        bool _killWorkerThreads = false;

        public static AdminManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Init()
        {
            _messageQ = new ConcurrentQueue<BotCommand>();
            //build thread worker pool or task worker pool
            //so we might want to read some settings to figure out how big to make the worker pool.
            int workerCount = 5;//default
            int.TryParse(ConfigurationManager.AppSettings["AdminManagerWorkers"], out workerCount);

            RegisterCommands();

            for (int i = 0; i < workerCount; i++)
            {
                Thread t = new Thread(MessageQWorker);
                t.Name = "AdminManagerWorker" + i;
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
            CommandManager.RegisterCommand(new BotCommand("kick", this, (object x) => { KickUser(x); }));
            CommandManager.RegisterCommand(new BotCommand("say", this, (object x) => { Say(x); }));
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

        private void KickUser(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;

            Helper.WriteCommand(bcMessage.userMessage.CommandText);
            DiscordMember toKick = bcMessage.userMessage.Message.Channel.parent.members.Find(x => x.Username == bcMessage.userMessage.CommandParams[0]);
            if (toKick == null)
            {
                Helper.WriteWarning($"Unable to find user {bcMessage.userMessage.CommandParams[0]}");
            }
            try
            {
                Concordia.client.KickMember(toKick);
            }
            catch (Exception ex)
            {
                Helper.WriteError(ex.Message);
            }
        }

        private void Say(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;

            Helper.WriteCommand(bcMessage.userMessage.CommandText);
            string echoMe = "";
            foreach (string s in bcMessage.userMessage.CommandParams)
            {
                echoMe += s + " ";
            }
            try
            {
                Concordia.client.SendMessageToChannel(echoMe, bcMessage.userMessage.Message.Channel);
            }
            catch (Exception ex)
            {
                Helper.WriteError(ex.Message);
            }
        }

        private void WhoIs(DiscordUserMessage message)
        {
            Helper.WriteCommand(message.CommandText);
            DiscordMember memberToCheck = message.Message.Channel.parent.members.Find(x => x.Username == message.CommandParams[0]);
            if (memberToCheck != null)
            {
                string msg = $"User info for {memberToCheck.Username}\n```\nID: {memberToCheck.ID}\nStatus: {memberToCheck.Status}";
                if (memberToCheck.CurrentGame != null)
                    msg += $"\nCurrent Game: {memberToCheck.CurrentGame}";
                msg += $"\nAvatar: {memberToCheck.GetAvatarURL()}";
                msg += "\n```";
                Concordia.client.SendMessageToChannel(msg, message.Message.Channel);
            }
        }
    }
}
