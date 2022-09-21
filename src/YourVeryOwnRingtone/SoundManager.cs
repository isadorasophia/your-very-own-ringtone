using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

#nullable enable

namespace YourVeryOwnRingtone
{
    [Export(typeof(SoundManager))]
    public sealed class SoundManager
    {
        private readonly string[] AvailableSounds = new string[] 
            { "breakpoint", "step", "exception" };

        public Dictionary<string, System.Media.SoundPlayer> _sounds = new();

        private readonly object _lock = new();

        /// <summary>
        /// Refresh all the sounds currently available in the library.
        /// </summary>
        public async Task RefreshSoundsAsync(string configurationFile)
        {
            lock (_lock)
            {
                _sounds.Clear();
            }

            if (!File.Exists(configurationFile))
            {
                return;
            }

            string jsonText = await ReadAllTextAsync(configurationFile);

            Dictionary<string, string> sounds;
            try
            {
                sounds = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText)!;
            }
            catch
            {
                throw new ArgumentException("Unable to deserialize configuration file.");
            }

            // The configuration file paths may be relative to the configuration file.
            string rootDirectory = Path.GetDirectoryName(configurationFile);

            lock (_lock)
            {
                foreach (string sound in AvailableSounds)
                {
                    if (sounds.TryGetValue(sound, out string path))
                    {
                        if (!Path.IsPathRooted(path))
                        {
                            path = Path.Join(rootDirectory, relativePath);
                        }
                        
                        path = Path.IsPathRooted(path) ? Path.Join(rootDirectory, relativePath);
                        if (File.Exists(rootPath))
                        {
                            System.Media.SoundPlayer player = new(rootPath);
                            player.LoadAsync();

                            _sounds[sound] = player;
                        }
                    } 
                }
            }
        }

        private async Task<string> ReadAllTextAsync(string file)
        {
            using StreamReader reader = File.OpenText(file);

            StringBuilder result = new();

            while (await reader.ReadLineAsync() is string line)
            {
                result.Append(line);
            }

            return result.ToString();
        }

        public void PlaySound(string name)
        {
            if (_sounds.TryGetValue(name, out var sound))
            {
                sound.Play();
            }
        }
    }
}
