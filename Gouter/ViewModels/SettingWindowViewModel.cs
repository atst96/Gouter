using CSCore.CoreAudioAPI;
using Gouter.Commands.SettingWindow;
using Gouter.Managers;
using Gouter.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.ViewModels
{
    internal class SettingWindowViewModel : ViewModelBase
    {
        private static readonly App AppInstance = App.Instance;

        private readonly MediaManager _mediaManager = AppInstance.MediaManager;
        private readonly MediaPlayer _mediaPlayer = AppInstance.MediaPlayer;
        private readonly SoundDeviceListener _soundDeviceListener = AppInstance.SoundDeviceListener;

        public SettingWindowViewModel()
        {
            this.Setting = App.Instance.Setting;

            this.MusicDirectories = this.Setting.MusicDirectories;
            this.ExcludeDirectories = this.Setting.ExcludeDirectories;

            if (this.MusicDirectories.Count == 0)
            {
                var musicDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                this.MusicDirectories.Add(musicDirectory);
            }

            this.SoundDevices = this._soundDeviceListener.Devices;
        }

        public ApplicationSetting Setting { get; }

        public NotifiableCollection<string> MusicDirectories { get; }

        private string _selectedMusicDirectory;
        public string SelectedMusicDirectory
        {
            get => this._selectedMusicDirectory;
            set
            {
                if (this.SetProperty(ref this._selectedMusicDirectory, value))
                {
                    this._removeMusicDirectoryCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private Command _addMusicDirectoryCommand;
        public Command AddMusicDirectoryCommand => this._addMusicDirectoryCommand ?? (this._addMusicDirectoryCommand = new AddMusicDirectoryCommand(this));

        private Command<string> _removeMusicDirectoryCommand;
        public Command<string> RemoveMusicDirectoryCommand => this._removeMusicDirectoryCommand ?? (this._removeMusicDirectoryCommand = new RemoveMusicDirectoryCommand(this));

        public NotifiableCollection<string> ExcludeDirectories { get; }

        private string _selectedExcludeDirectory;
        public string SelectedExcludeDirectory
        {
            get => this._selectedExcludeDirectory;
            set
            {
                if (this.SetProperty(ref this._selectedExcludeDirectory, value))
                {
                    this._removeExcludeDirectoryCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private Command _addExcludeDirectoryCommand;
        public Command AddExcludeDirectoryCommand => this._addExcludeDirectoryCommand ??= new AddExcludeDirectoryCommand(this);

        private Command<string> _removeExcludeDirectoryCommand;
        public Command<string> RemoveExcludeDirectoryCommand => this._removeExcludeDirectoryCommand ??= new RemoveExcludeDirectoryCommand(this);

        public SoundOutType SelectedSoundOutType
        {
            get => this.Setting.SoundOutType;
            set => this.Setting.SoundOutType = value;
        }

        public string SelectedOutputDeviceId
        {
            get => this.Setting.SoundOutDeviceId;
            set => this.Setting.SoundOutDeviceId = value;
        }

        public IReadOnlyDictionary<SoundOutType, string> SoundOutputTypes { get; } = new Dictionary<SoundOutType, string>
        {
            [SoundOutType.DirectSound] = "DirectSound",
            [SoundOutType.Wasapi_Shraed] = "WASAPI (共有モード)",
            [SoundOutType.Wasapi_Exclusive] = "WASAPI (専有モード)"
        };

        public NotifiableCollection<SoundDeviceInfo> SoundDevices { get; }
    }
}
