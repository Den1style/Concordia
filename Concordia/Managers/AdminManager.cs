

using Concordia.Commands;
using Concordia.Entities;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Concordia.Managers
{
    class AdminManager
    {
        ConcurrentQueue<DiscordUserMessage> _messageQ;
        readonly static AdminManager _instance = new AdminManager();
        bool _killWorkerThreads = false;

        public static AdminManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public AdminManager()
        {
            _messageQ = new ConcurrentQueue<DiscordUserMessage>();
            //build thread worker pool or task worker pool
            //so we might want to read some settings to figure out how big to make the worker pool.
            Thread t = new Thread(MessageQWorker);
            t.Name = "AdminManager1";
            t.IsBackground = true;
            t.Start();

            Thread t2 = new Thread(MessageQWorker);
            t2.Name = "AdminManager2";
            t2.IsBackground = true;
            t2.Start();

            Thread t3 = new Thread(MessageQWorker);
            t3.Name = "AdminManager3";
            t3.IsBackground = true;
            t3.Start();           
        }

        public void ProcessDiscordCommandMessage(DiscordUserMessage message)
        {
            _messageQ.Enqueue(message);
        }

        private void MessageQWorker()
        {
            while (!_killWorkerThreads)
            {
                DiscordUserMessage message;
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

        private void ParseMessage(DiscordUserMessage message)
        {
            switch (message.BotCommand)
            {
                case Command.Kick:
                    KickUser(message);
                    break;
                case Command.Say:
                    Say(message);
                    break;
                case Command.WhoIs:
                    WhoIs(message);
                    break;
            }
        }

        private void KickUser(DiscordUserMessage message)
        {
            Helper.WriteCommand(message.CommandText);
            DiscordMember toKick = message.Message.Channel.parent.members.Find(x => x.Username == message.CommandParams[0]);
            if(toKick == null)
            {
                Helper.WriteWarning($"Unable to find user {message.CommandParams[0]}");
            }
            try
            {
                Concordia.client.KickMember(toKick);
            }
            catch(Exception ex)
            {
                Helper.WriteError(ex.Message);
            }            
        }

        private void Say(DiscordUserMessage message)
        {
            Helper.WriteCommand(message.CommandText);
            string echoMe = "";
            foreach(string s in message.CommandParams)
            {
                echoMe += s + " ";
            }
            try
            {
                Concordia.client.SendMessageToChannel(echoMe, message.Message.Channel);
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
