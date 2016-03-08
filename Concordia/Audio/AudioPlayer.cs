using DiscordSharp;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concordia.Audio
{
    internal class AudioPlayer
    {
        WaveCallbackInfo callbackInfo;
        WaveOut outputDevice;
        BufferedWaveProvider bufferedWaveProvider;
        DiscordVoiceConfig config;

        public AudioPlayer(DiscordVoiceConfig __config)
        {
            config = __config;
            callbackInfo = WaveCallbackInfo.FunctionCallback();
            outputDevice = new WaveOut(callbackInfo);
            bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, config.Channels));
        }

        public void EnqueueBytes(byte[] bytes)
        {
            //bufferedWaveProvider.BufferLength = bytes.Length;
            //bufferedWaveProvider.BufferDuration = new TimeSpan(0, 0, bytes.Length / 88497);
            bufferedWaveProvider.AddSamples(bytes, 0, bytes.Length);
        }

        public Task PlayAudio()
        {
            return Task.Run(() =>
            {
                outputDevice.Init(bufferedWaveProvider);
                outputDevice.Play();
            });
        }

        public void StopAudio()
        {
            outputDevice.Stop();
        }

        public void SendVoice(string file)
        {
            DiscordVoiceClient vc = Concordia.voice;

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

    }
}
