

using Concordia.Commands;
using Concordia.Entities;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Concordia.Managers
{
    class DiscordCommandManager
    {
        ConcurrentQueue<DiscordUserMessage> _messageQ;
        readonly static DiscordCommandManager _instance = new DiscordCommandManager();
        bool _killWorkerThreads = false;

        public static DiscordCommandManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public DiscordCommandManager()
        {
            _messageQ = new ConcurrentQueue<DiscordUserMessage>();
            //build thread worker pool or task worker pool
            //so we might want to read some settings to figure out how big to make the worker pool.
            Thread t = new Thread(MessageQWorker);
            t.Name = "DiscordCommandManager1";
            t.IsBackground = true;
            t.Start();

            Thread t2 = new Thread(MessageQWorker);
            t2.Name = "DiscordCommandManager2";
            t2.IsBackground = true;
            t2.Start();

            Thread t3 = new Thread(MessageQWorker);
            t3.Name = "DiscordCommandManager3";
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
    }
}
