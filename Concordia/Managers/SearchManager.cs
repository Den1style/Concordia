
using Concordia.Entities;
using Concordia.Managers.Interfaces;
using DiscordSharp.Objects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Concordia.Managers
{
    class SearchManager : Manager
    {
        readonly static SearchManager _instance = new SearchManager();        

        public static SearchManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public override void Init()
        {
            StartWorkers("SearchManager");
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            CommandManager.RegisterCommand(new BotCommand("search", this, (object x) => { UrbanDictionary(x); }));
        }    

        private async void UrbanDictionary(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;

            Helper.WriteCommand(bcMessage.userMessage.CommandText);

            var headers = new WebHeaderCollection();
            var res = await SearchHelper.GetResponseAsync($"http://api.urbandictionary.com/v0/define?term={bcMessage.userMessage.CommandParams[0]}");
            try
            {
                var items = JObject.Parse(res);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"`Term:` {items["list"][0]["word"].ToString()}");
                sb.AppendLine($"`Definition:` {items["list"][0]["definition"].ToString()}");
                sb.AppendLine($"`Example:` {items["list"][0]["example"].ToString()}");
                sb.AppendLine($":thumbsup:: {items["list"][0]["thumbs_up"].ToString()} \t:thumbsdown:: {items["list"][0]["thumbs_down"].ToString()}");

                bcMessage.userMessage.Message.Channel.SendMessage(sb.ToString());
            }
            catch
            {
                bcMessage.userMessage.Message.Channel.SendMessage("💢 Failed finding a definition for that term.");
            }
        }
    }
}
