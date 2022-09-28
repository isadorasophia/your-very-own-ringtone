using NAudio.Wave;
using System;

namespace YourVeryOwnRingtone.MusicBox
{
    public class SoundCacheProvider : ISampleProvider
    {
        private readonly SoundCache _sound;
        private int _position;

        public SoundCacheProvider(SoundCache sound)
        {
            _sound = sound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int availableSamples = _sound.AudioData.Length - _position;
            int samplesToCopy = Math.Min(availableSamples, count);

            Array.Copy(_sound.AudioData, _position, buffer, offset, samplesToCopy);
            _position += samplesToCopy;

            return samplesToCopy;
        }

        public WaveFormat WaveFormat => _sound.WaveFormat;
    }
}
