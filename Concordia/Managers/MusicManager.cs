using Concordia.Commands;
using Concordia.Entities;
using NAudio;
using DiscordSharp.Events;
using DiscordSharp.Objects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using YoutubeExtractor;
using System.Diagnostics;

namespace Concordia.Managers
{
    class MusicManager
    {
        ConcurrentQueue<DiscordUserMessage> _messageQ;
        readonly static MusicManager _instance = new MusicManager();
        bool _killWorkerThreads = false;

        public static MusicManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public MusicManager()
        {
            _messageQ = new ConcurrentQueue<DiscordUserMessage>();
            //build thread worker pool or task worker pool
            //so we might want to read some settings to figure out how big to make the worker pool.
            Thread t = new Thread(MessageQWorker);
            t.Name = "MusicManager1";
            t.IsBackground = true;
            t.Start();

            Thread t2 = new Thread(MessageQWorker);
            t2.Name = "MusicManager2";
            t2.IsBackground = true;
            t2.Start();

            Thread t3 = new Thread(MessageQWorker);
            t3.Name = "MusicManager3";
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
                case Command.Youtube:
                    Youtube(message);
                    break;
                case Command.Say:
                    Echo(message);
                    break;
            }
        }

        private void Echo(DiscordUserMessage message)
        {
            Helper.WriteCommand(message.CommandText);
        }

        private void Youtube(DiscordUserMessage message)
        {
            Helper.WriteCommand(message.CommandText);
            string link = message.Arguments;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(VideoInfo => VideoInfo.AudioBitrate)
                .First();

            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            string audioFile = Path.Combine("E:\\Downloads\\", video.Title + video.AudioExtension);
            var audioDownloader = new AudioDownloader(video, audioFile);
            audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);
            audioDownloader.Execute();

            //File Exists at this point.

            //Join channel
            Concordia.client.ConnectToVoiceChannel(message.Message.Channel.parent.channels.Find(x => (x.Name.ToLower() == "music!") && (x.Type == ChannelType.Voice)));

            var bytes = File.ReadAllBytes(audioFile);
            Concordia.audioPlayer.EnqueueByets(bytes);
            Concordia.audioPlayer.PlayAudio();
        }        
    }
}