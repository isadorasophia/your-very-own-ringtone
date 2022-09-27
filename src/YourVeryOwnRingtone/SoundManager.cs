using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Media;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Reflection;
using Microsoft.VisualStudio.Threading;

#nullable enable

namespace YourVeryOwnRingtone
{
    [Export(typeof(SoundManager))]
    public sealed class SoundManager
    {
        private readonly HashSet<string> AvailableSounds = new()
        { 
            "apply",
            "build.start",
            "build.onsuccess",
            "build.onfail",
            "breakpoint",
            "continue",
            "exception",
            "find",
            "restart",
            "save",
            "start",
            "step",
            "stepover",
            "stepinto",
            "stepout",
            "stop",
            "undo"
        };

        private readonly string _defaultConfigurationFile = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "themes", "lofi", "settings.json");

        public Dictionary<string, List<SoundPlayer>> _sounds = new();

        private readonly object _lock = new();

        private bool _isEnabled = true;

        private readonly Random _random = new();

        /// <summary>
        /// Used to write responsive messages to the user.
        /// </summary>
        private IVsStatusbar? _bar;
        private IVsOutputWindowPane? _pane;

        private readonly Guid _paneGuid = new Guid("397DC4BA-0F26-4AF2-B920-3AEB3F551482");
        private const string _paneTitle = "Your very own ringtone!";

        /// <summary>
        /// Tracks whether the session is currently being debugged.
        /// </summary>
        private bool _isDebugging;

        public async Task InitializeAsync(IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _bar = await serviceProvider.GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;

            // Initialize output window.
            IVsOutputWindow? outputWindow = await serviceProvider.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid _guid = _paneGuid;
            outputWindow?.CreatePane(_guid, _paneTitle, fInitVisible: 0, fClearWithSolution: 0);
            outputWindow?.GetPane(_guid, out _pane);

            _pane?.OutputStringThreadSafe("Your very own ringtone! was just loaded.\n");
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
                if (!string.IsNullOrEmpty(configurationFile))
                {
                    _pane?.OutputStringThreadSafe($"Unable to load sounds for configuration file: {configurationFile}.\n");
                    _ = _bar?.SetText("Your very own ringtone! was unable to load the configuration file.\n");
                }

                _pane?.OutputStringThreadSafe("Loading default sounds...\n");
                configurationFile = _defaultConfigurationFile;
            }
            else
            {
                _pane?.OutputStringThreadSafe($"Loading sounds for {configurationFile}\n");
            }

            string jsonText = await ReadAllTextAsync(configurationFile);

            Dictionary<string, string> sounds;
            try
            {
                sounds = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText)!;
            }
            catch
            {
                _pane?.OutputStringThreadSafe("Invalid .json. Sounds will be unavailable.\n");

                throw new ArgumentException("Unable to deserialize configuration file.");
            }

            // The configuration file paths may be relative to the configuration file.
            string rootDirectory = Path.GetDirectoryName(configurationFile);

            lock (_lock)
            {
                foreach ((string sound, string valuePath) in sounds)
                {
                    if (!AvailableSounds.Contains(sound))
                    {
                        _pane?.OutputStringThreadSafe($"Skipping sound for {sound} event.\n");
                        continue;
                    }

                    // We support multiple paths in the same sound, so iterate over all of them.
                    string[] paths = valuePath.Split(',');
                    for (int i = 0; i < paths.Length; ++i)
                    {
                        string path = paths[i];
                        if (!Path.IsPathRooted(path))
                        {
                            path = Path.Combine(rootDirectory, path);
                        }

                        if (File.Exists(path))
                        {
                            SoundPlayer player = new(path);
                            player.LoadAsync();

                            if (!_sounds.TryGetValue(sound, out List<SoundPlayer> soundPlayers))
                            {
                                soundPlayers = new();
                                _sounds[sound] = soundPlayers;
                            }

                            soundPlayers.Add(player);

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

        public async Task PlaySoundAsync(string name)
        {
            // Ensures that the rest of the method runs on a background thread. Simply using ConfigureAwait(false) won't necessarily ensure that's the case,
            // see https://devblogs.microsoft.com/dotnet/configureawait-faq/#does-configureawaitfalse-guarantee-the-callback-wont-be-run-in-the-original-context
            await TaskScheduler.Default;

            switch (name)
            {
                case "start":
                case "step":
                case "breakpoint":
                    _isDebugging = true;
                    break;

                case "continue":
                    if (!_isDebugging)
                    {
                        // ignore continue commands if debugging is not active. "start" will be fired for those instead.
                        return;
                    }

                    break;

                case "stop":
                    _isDebugging = false;
                    break;
            }

            if (!_isEnabled)
            {
                return;
            }

            if (_sounds.TryGetValue(name, out List<SoundPlayer> soundPlayers))
            {
                int soundIndex = _random.Next(soundPlayers.Count);
                soundPlayers[soundIndex].Play();
            }
        }
    }
}
