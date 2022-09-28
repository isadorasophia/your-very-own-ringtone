using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YourVeryOwnRingtone.MusicBox
{
    public class SoundCache
    {
        public float[] AudioData { get; }

        public WaveFormat WaveFormat { get; }

        public SoundCache(string file)
        {
            using AudioFileReader reader = new(file);

            WaveFormat = reader.WaveFormat;

            List<float> wholeFile = new(capacity: (int)Math.Round(reader.Length / 4f));
            float[] readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];

            int samplesRead;
            while ((samplesRead = reader.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }

            AudioData = wholeFile.ToArray();
        }
    }
}
