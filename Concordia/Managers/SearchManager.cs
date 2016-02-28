using Concordia.Commands;
using Concordia.Entities;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Managers
{
    class SearchManager
    {
        ConcurrentQueue<DiscordUserMessage> _messageQ;
        readonly static SearchManager _instance = new SearchManager();
        bool _killWorkerThreads = false;

        public static SearchManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public SearchManager()
        {
            _messageQ = new ConcurrentQueue<DiscordUserMessage>();
            //build thread worker pool or task worker pool
            //so we might want to read some settings to figure out how big to make the worker pool.
            Thread t = new Thread(MessageQWorker);
            t.Name = "SearchManager1";
            t.IsBackground = true;
            t.Start();

            Thread t2 = new Thread(MessageQWorker);
            t2.Name = "SearchManager2";
            t2.IsBackground = true;
            t2.Start();

            Thread t3 = new Thread(MessageQWorker);
            t3.Name = "SearchManager3";
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

        private async void ParseMessage(DiscordUserMessage message)
        {
            switch (message.BotCommand)
            {
                case Command.UrbanDictionary:
                    await UrbanDictionary(message);
                    break;
                case Command.HashTag:
                    await HashTag(message);
                    break;
            }
        }

        private async Task UrbanDictionary(DiscordUserMessage message)
        {
            Helper.WriteCommand(message.CommandText);
            var headers = new WebHeaderCollection();
            var res = await SearchHelper.GetResponseAsync($"http://api.urbandictionary.com/v0/define?term={message.CommandParams[0]}");
            try
            {
                var items = JObject.Parse(res);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"`Term:` {items["list"][0]["word"].ToString()}");
                sb.AppendLine($"`Definition:` {items["list"][0]["definition"].ToString()}");
                sb.AppendLine($"`Example:` {items["list"][0]["example"].ToString()}");
                sb.AppendLine($":thumbsup:: {items["list"][0]["thumbs_up"].ToString()} \t:thumbsdown:: {items["list"][0]["thumbs_down"].ToString()}");

                message.Message.Channel.SendMessage(sb.ToString());
            }
            catch
            {
                message.Message.Channel.SendMessage("💢 Failed finding a definition for that term.");
            }
        }              

        private async Task HashTag(DiscordUserMessage message)
        {
            Helper.WriteCommand(message.CommandText);
            var headers = new WebHeaderCollection();
            var res = await SearchHelper.GetResponseAsync($"http://api.tagdef.com/one.{message.CommandParams[0]}.json");
            try
            {
                var items = JObject.Parse(res);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"`#:` {items["defs"]["def"]["hashtag"].ToString()}");
                sb.AppendLine($"`Definition:` {items["defs"]["def"]["text"].ToString()}");
                sb.AppendLine($":thumbsup:: {items["defs"]["def"]["upvotes"].ToString()} \t:thumbsdown:: {items["defs"]["def"]["downvotes"].ToString()}");

                message.Message.Channel.SendMessage(sb.ToString());
            }
            catch
            {
                message.Message.Channel.SendMessage("💢 Failed finding a definition for that term.");
            }
        }
    }
}
