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

        private Func<string, Task>? _onUpdate;

        public async Task InitializeAsync(SoundManager manager)
        {
            _onUpdate = manager.RefreshSoundsAsync;
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            if (_onUpdate is null)
            {
                return;
            }

            await _onUpdate.Invoke(_configurationFile);
        }

        [Category("Sounds Settings")]
        [DisplayName("Configuration File")]
        [Description("Path to your json specifying a list of .wav sounds.")]
        public string ConfigurationFile
        { 
            get => _configurationFile;
            set
            {
                _configurationFile = value;
                _ = RefreshAsync();
            }
        }
    }
}
