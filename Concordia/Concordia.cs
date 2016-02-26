using System;
using Concordia.Audio;
using DiscordSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DiscordSharp.Objects;

namespace Concordia
{
    class Concordia
    {
        public static DiscordClient client;
        public static DiscordMember owner;
        public static AudioPlayer audioPlayer;
        public static WaitHandle waitHandle = new AutoResetEvent(false);
        CancellationToken cancelToken;
        DateTime loginDate;


        static void Main(string[] args) => new Concordia().Start(args);

        private void Start(string[] args)
        {
            Console.Title = $"Concordia Discord Bot";

            client = new DiscordClient();
            client.RequestAllUsersOnStartup = true;

            SetupEvents(cancelToken);

        }

        private void WriteError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
        }

        private void WriteWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Warning: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
        }

        private Task SetupEvents(CancellationToken cancelToken)
        {
            return Task.Run(() =>
            {

                client.MessageReceived += (sender, e) =>
                {
                    Console.WriteLine($"[Message from {e.author.Username} in {e.Channel.Name} on {e.Channel.parent}]: {e.message.content} ");
                //When we receive a message ...
                //TODO: Parse out commands

            };
                client.VoiceClientConnected += (sender, e) =>
                {
                    owner.SlideIntoDMs($"Voice connection complete.");
                };

                client.GuildCreated += (sender, e) =>
                {
                    owner.SlideIntoDMs($"[Joined Server]: {e.server.name}");
                };
                client.SocketClosed += (sender, e) =>
                {
                    if (e.Code != 1000 && !e.WasClean)
                    {
                        Console.WriteLine($"Socket Closed! Code: {e.Code}. Reason: {e.Reason}. Clear: {e.WasClean}.");
                        Console.WriteLine("Waiting 6 seconds to reconnect..");
                        Thread.Sleep(6 * 1000);
                        client.Connect();
                    }
                    else
                    {
                        Console.WriteLine($"Shutting down ({e.Code}, {e.Reason}, {e.WasClean})");
                    }

                };
                client.TextClientDebugMessageReceived += (sender, e) =>
                {
                    if (e.message.Level == MessageLevel.Error || e.message.Level == MessageLevel.Critical)
                    {
                        WriteError($"(Logger Error) {e.message.Message}");
                        try
                        {
                            owner.SlideIntoDMs($"Bot error ocurred: ({e.message.Level.ToString()})```\n{e.message.Message}\n```");
                        }
                        catch { }
                    }
                    if (e.message.Level == MessageLevel.Warning)
                        WriteWarning($"(Logger Warning) {e.message.Message}");
                };
                client.Connected += (sender, e) =>
                {
                    Console.WriteLine("Connected as " + e.user.Username);
                    loginDate = DateTime.Now;

                };
                if (client.SendLoginRequest() != null)
                {
                    client.Connect();
                }
            }, cancelToken);
        }

        public void Exit()
        {
            client.Logout();
            client.Dispose();
            Environment.Exit(0);
        }

    }
}
