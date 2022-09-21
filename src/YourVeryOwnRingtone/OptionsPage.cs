using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

#nullable enable

namespace YourVeryOwnRingtone
{
    public sealed class OptionsPage : DialogPage
    {
        private string _configurationFile = string.Empty;
        private bool _disableSounds = false;

        private SoundManager? _manager;

        public async Task InitializeAsync(SoundManager manager)
        {
            _manager = manager;

            await RefreshAsync();
        }

        /// <summary>
        /// Refresh all settings of the options page to the manager.
        /// </summary>
        public async Task RefreshAsync()
        {
            if (_manager is null)
            {
                return;
            }

            await _manager.RefreshSoundsAsync(_configurationFile);
            _manager.SetSound(!_disableSounds);
        }

        [Category("Sounds Settings")]
        [DisplayName("Configuration File")]
        [Description("Path to your json specifying a list of .wav sounds.")]
        public string ConfigurationFile
        { 
            get => _configurationFile;
            set
            {
                if (_configurationFile != value)
                {
                    _configurationFile = value;
                    _ = _manager?.RefreshSoundsAsync(value);
                }
            }
        }

        [Category("Sounds Settings")]
        [DisplayName("Disable Sounds")]
        [Description("Check to disable any sounds.")]
        public bool DisableSounds
        {
            get => _disableSounds;
            set
            {
                if (_disableSounds != value)
                {
                    _disableSounds = value;
                    _manager?.SetSound(!_disableSounds);
                }
            }
        }
    }
}
