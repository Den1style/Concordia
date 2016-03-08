
using Concordia.Entities;
using Concordia.Managers.Interfaces;
using DiscordSharp.Objects;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading;

namespace Concordia.Managers
{
    class AdminManager : Manager
    {
        readonly static AdminManager _instance = new AdminManager();

        public static AdminManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public override void Init()
        {
            StartWorkers("AdminManager");
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            CommandManager.RegisterCommand(new BotCommand("kick", this, (object x) => { KickUser(x); }));
            CommandManager.RegisterCommand(new BotCommand("say", this, (object x) => { Say(x); }));
            CommandManager.RegisterCommand(new BotCommand("prune", this, (object x) => { Prune(x); }));
            CommandManager.RegisterCommand(new BotCommand("join", this, (object x) => { Join(x); }));
            CommandManager.RegisterCommand(new BotCommand("cls", this, (object x) => { Cls(x); }));
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

        private void Cls(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;
            Helper.WriteCommand(bcMessage.userMessage.CommandText);
            int messageCount = 0;

            if (int.TryParse(bcMessage.userMessage.CommandParams[0], out messageCount))
            {
                var messagesToPrune = Concordia.client.GetMessageHistory(bcMessage.userMessage.Message.Channel, messageCount);
                foreach (var msg in messagesToPrune)
                {
                    if (msg.author.Username.ToLower() == Concordia.bot.Username.ToLower())
                    {
                        Concordia.client.DeleteMessage(msg);
                        Thread.Sleep(100);
                    }
                }
            }            
        }

        private void Prune(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;
            Helper.WriteCommand(bcMessage.userMessage.CommandText);
            int messageCount = 0;

            //Prune Messages or prune specific user message.

            if (bcMessage.userMessage.CommandParams.Length > 1)
            {
                
                if (int.TryParse(bcMessage.userMessage.CommandParams[1], out messageCount))
                {
                    var messagesToPrune = Concordia.client.GetMessageHistory(bcMessage.userMessage.Message.Channel, messageCount);
                    foreach (var msg in messagesToPrune)
                    {
                        if(msg.author.Username.ToLower() == bcMessage.userMessage.CommandParams[0].ToLower())
                        {
                            Concordia.client.DeleteMessage(msg);
                            Thread.Sleep(100);
                        }                        
                    }
                }
            }
            else
            {
                if (int.TryParse(bcMessage.userMessage.CommandParams[0], out messageCount))
                {
                    var messagesToPrune = Concordia.client.GetMessageHistory(bcMessage.userMessage.Message.Channel, messageCount);
                    foreach (var msg in messagesToPrune)
                    {
                        Concordia.client.DeleteMessage(msg);
                        Thread.Sleep(100);
                    }
                }
            }
                    
        }

        private void Join(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;
            Helper.WriteCommand(bcMessage.userMessage.CommandText);
            try
            {
                string substring = bcMessage.userMessage.Arguments.Substring(bcMessage.userMessage.Arguments.LastIndexOf('/') + 1);
                Concordia.client.AcceptInvite(substring);
            }
            catch(Exception ex)
            {
                Concordia.client.SendMessageToChannel(ex.Message, bcMessage.userMessage.Message.Channel);
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
