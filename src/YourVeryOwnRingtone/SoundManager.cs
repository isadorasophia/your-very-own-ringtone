using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

#nullable enable

namespace YourVeryOwnRingtone
{
    [Export(typeof(SoundManager))]
    public sealed class SoundManager
    {
        private readonly string[] AvailableSounds = new string[] 
            { 
                "apply",
                "build",
                "breakpoint",
                "exception",
                "find",
                "restart",
                "step",
                "stepover",
                "stepinto",
                "stepout",
                "undo"
            };

        public Dictionary<string, System.Media.SoundPlayer> _sounds = new();

        private readonly object _lock = new();

        private bool _isEnabled = true;

        /// <summary>
        /// Used to write responsive messages to the user.
        /// </summary>
        private IVsStatusbar? _bar;
        private IVsOutputWindowPane? _pane;

        private readonly Guid _paneGuid = new Guid("397DC4BA-0F26-4AF2-B920-3AEB3F551482");
        private const string _paneTitle = "Your very own ringtone!";

        public async Task InitializeAsync(IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _bar = await serviceProvider.GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;

            // Initialize output window.
            IVsOutputWindow? outputWindow = await serviceProvider.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid _guid = _paneGuid;
            outputWindow?.CreatePane(_guid, _paneTitle, fInitVisible: 1, fClearWithSolution: 0);
            outputWindow?.GetPane(_guid, out _pane);

            _pane?.OutputStringThreadSafe("Your very own ringtone! is all set.");
            _pane?.Activate();
        }

        public void SetSound(bool isOn)
        {
            _isEnabled = isOn;
        }

        /// <summary>
        /// Refresh all the sounds currently available in the library.
        /// </summary>
        public async Task RefreshSoundsAsync(string configurationFile)
        {
            lock (_lock)
            {
                _sounds.Clear();
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (!File.Exists(configurationFile))
            {
                _pane?.OutputStringThreadSafe($"Unable to load sounds for configuration file: {configurationFile}.\n");
                _ = _bar?.SetText("Your very own ringtone! was unable to load the configuration file.\n");

                return;
            }

            _pane?.OutputStringThreadSafe($"Loading sounds for {configurationFile}...\n");

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
                            path = Path.Combine(rootDirectory, path);
                        }
                        
                        if (File.Exists(path))
                        {
                            System.Media.SoundPlayer player = new(path);
                            player.LoadAsync();

                            _sounds[sound] = player;

                            _pane?.OutputStringThreadSafe($"Loaded sound for {sound} events.\n");
                        }
                    } 
                }
            }

            _pane?.OutputStringThreadSafe("Finished loading sounds.\n");
            _bar?.SetText($"Your very own ringtone! is all set.\n");
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
            if (!_isEnabled)
            {
                return;
            }

            if (_sounds.TryGetValue(name, out var sound))
            {
                sound.Play();
            }
        }
    }
}
