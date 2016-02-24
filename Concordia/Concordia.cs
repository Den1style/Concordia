using System;
using DiscordSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Concordia
{
    class Concordia
    {
        public static DiscordClient client;
        CancellationToken cancelToken;
        DateTime loginDate;
        

        static void Main(string[] args) => new Concordia().Start(args);

        private void Start(string[] args)
        {
            //Set the console Title Window
            Console.Title = $"Concordia Discord Bot";

            //Lets figure out what this does later. .. 
            client.RequestAllUsersOnStartup = true;

            SetupEvents(cancelToken);

        }

        private void SetupEvents(CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {
                //When we receive a message ...
                client.MessageReceived += (sender, e) =>
                {
                    Console.WriteLine($"[Message from {e.author.Username} in {e.Channel.Name} on {e.Channel.parent}]: {e.message.content} ");

                };
                client.GuildCreated += (sender, e) =>
                {
                    Console.WriteLine($"[Joined Server]: {e.server.name}");
                };
                client.Connected += (sender, e) =>
                {
                    Console.WriteLine($"[Connected As]: {e.user.Username}");
                    loginDate = DateTime.Now;

                    //if(!String.IsNullOrWhiteSpace())

                };

            });
        }
    }
}
