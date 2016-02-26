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

        public void EnqueueByets(byte[] bytes)
        {
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

    }
}
