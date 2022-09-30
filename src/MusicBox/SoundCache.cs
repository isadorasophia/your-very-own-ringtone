using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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

            ISampleProvider sample = reader;
            if (reader.WaveFormat.SampleRate != MusicBoxEngine.DEFAULT_SAMPLE_RATE)
            {
                // Right now, we only support our default sample rate. Manually resample that if this is not the case.
                sample = new WdlResamplingSampleProvider(reader, MusicBoxEngine.DEFAULT_SAMPLE_RATE);
            }

            WaveFormat = sample.WaveFormat;

            // This is just a capacity guess anyway, so don't bother getting the actual length from the sample.
            int dataLength = (int)Math.Round(reader.Length / 4f);

            List<float> wholeFile = new(capacity: dataLength);
            float[] readBuffer = new float[WaveFormat.SampleRate * WaveFormat.Channels];

            int samplesRead;
            while ((samplesRead = sample.Read(readBuffer, 0, readBuffer.Length)) > 0)
            {
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            }

            AudioData = wholeFile.ToArray();
        }
    }
}
