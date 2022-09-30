using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace YourVeryOwnRingtone.MusicBox
{
    public class MusicBoxEngine : IDisposable
    {
        private readonly IWavePlayer _outputDevice;
        private readonly MixingSampleProvider _mixer;

        public const int DEFAULT_SAMPLE_RATE = 44100;

        public MusicBoxEngine(int sampleRate = DEFAULT_SAMPLE_RATE, int channelCount = 2)
        {
            _outputDevice = new WaveOutEvent();
            _mixer = new(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            _mixer.ReadFully = true;

            _outputDevice.Init(_mixer);
            _outputDevice.Play();
        }

        public void PlaySound(SoundCache sound)
        {
            AddMixerInput(new SoundCacheProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            _mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == _mixer.WaveFormat.Channels)
            {
                return input;
            }

            if (input.WaveFormat.Channels == 1 && _mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }

            throw new NotImplementedException("Invalid channel count for this sound!");
        }

        public void Dispose()
        {
            _outputDevice.Dispose();
        }
    }
}
