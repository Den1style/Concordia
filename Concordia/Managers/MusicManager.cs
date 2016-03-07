using Concordia.Commands;
using Concordia.Entities;
using NAudio;
using NAudio.Wave;
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
using DiscordSharp;

namespace Concordia.Managers
{
    class MusicManager : Manager
    {
        readonly static MusicManager _instance = new MusicManager();

        public static MusicManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public override void Init()
        {
            StartWorkers("MusicManager");
            //or
            //StartWorkers("MusicManagerWorkers");
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            CommandManager.RegisterCommand(new BotCommand("youtube", this, (object x) => { Youtube(x); }));
            CommandManager.RegisterCommand(new BotCommand("echo", this, (object x) => { Echo(x); }));
        }

        private void Echo(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;
            Helper.WriteCommand(bcMessage.commandText);
        }       

        private void Youtube(object objMessage)
        {
            BotCommand bcMessage = (BotCommand)objMessage;
            Helper.WriteCommand(bcMessage.commandText);
            string link = bcMessage.userMessage.Arguments;

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
            Console.WriteLine($"{video.Title} - Started Download");
            //audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
            //audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);
            audioDownloader.DownloadFinished += (sender, args) => Console.WriteLine($"{video.Title} - Download Complete");
            audioDownloader.Execute();

            //File Exists at this point.
            var musicStream = ConvertToPCM(audioFile);



            // //audioDownloader.DownloadProgressChanged -= null;
            // //audioDownloader.AudioExtractionProgressChanged -= null;
            //audioDownloader.DownloadStarted -= null;
            audioDownloader.DownloadFinished -= null;


            // //Join channel
            //Concordia.client.ConnectToVoiceChannel(bcMessage.userMessage.Message.Channel.parent.channels.Find(x => (x.Name.ToLower() == "music!") && (x.Type == ChannelType.Voice)));
            Concordia.client.ConnectToVoiceChannel(Concordia.client.GetChannelByName("music!"));          

            SendVoice(audioFile);

            ////var bytes = File.ReadAllBytes(audioFile);
            //Concordia.audioPlayer.EnqueueBytes(musicStream.ToArray());
            Concordia.audioPlayer.PlayAudio();
        }

        private void SendVoice(string file)
        {
            DiscordVoiceClient vc = Concordia.client.GetVoiceClient();
            
            try
            {
                int ms = 60;
                int channels = 1;
                int sampleRate = 48000;

                int blockSize = 48 * 2 * channels * ms; //sample rate * 2 * channels * milliseconds
                byte[] buffer = new byte[blockSize];
                var outFormat = new WaveFormat(sampleRate, 16, channels);

                vc.SetSpeaking(true);
                if (file.EndsWith(".wav"))
                {
                    using (var waveReader = new WaveFileReader(file))
                    {
                        int byteCount;
                        while ((byteCount = waveReader.Read(buffer, 0, blockSize)) > 0)
                        {
                            if (vc.Connected)
                                vc.SendVoice(buffer);
                            else
                                break;
                        }
                    }
                }
                else if (file.EndsWith(".mp3"))
                {
                    using (var mp3Reader = new MediaFoundationReader(file))
                    {
                        using (var resampler = new MediaFoundationResampler(mp3Reader, outFormat) { ResamplerQuality = 60 })
                        {
                            //resampler.ResamplerQuality = 60;
                            int byteCount;
                            while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0)
                            {
                                if (vc.Connected)
                                {
                                    vc.SendVoice(buffer);
                                }
                                else
                                    break;
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Voice finished enqueuing");
                            Console.ForegroundColor = ConsoleColor.White;
                            resampler.Dispose();
                            mp3Reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during voice: `" + ex.Message + "`\n\n```" + ex.StackTrace + "\n```");
            }
        }

        private MemoryStream ConvertToPCM(string audioFile)
        {
            MemoryStream convertedStream = new MemoryStream();

            using (Mp3FileReader reader = new Mp3FileReader(audioFile))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    CreateWaveFile(convertedStream, pcmStream);
                }
            }

            return convertedStream;
        }

        public static void CreateWaveFile(Stream waveMemoryStream, WaveStream stream)
        {
            using (WaveFileWriter writer =
                new WaveFileWriter(waveMemoryStream, stream.WaveFormat))
            {
                byte[] buffer = new byte[stream.WaveFormat.SampleRate * stream.WaveFormat.Channels * 16];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    writer.WriteData(buffer, 0, bytesRead);
                }
            }
        }
    }
}