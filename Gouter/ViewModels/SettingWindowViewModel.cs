using System;
using System.Collections.Generic;
using Gouter.Managers;
using Gouter.Messaging;
using Gouter.Players;
using Livet.Messaging;

namespace Gouter.ViewModels
{
    internal class SettingWindowViewModel : ViewModelBase
    {
        private static readonly App AppInstance = App.Instance;

        private readonly MediaManager _mediaManager = AppInstance.MediaManager;
        private readonly PlaylistPlayer _mediaPlayer = AppInstance.MediaPlayer;
        private readonly SoundDeviceListener _soundDeviceListener = AppInstance.SoundDeviceListener;

        /// <summary>
        /// Messenger
        /// </summary>
        public InteractionMessenger Messenger { get; } = new();

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

        public ObservableList<string> MusicDirectories { get; }

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

        private Command<FolderSelectionMessage> _addMusicDirectoryCommand;
        public Command<FolderSelectionMessage> AddMusicDirectoryCommand => this._addMusicDirectoryCommand ??= this.Commands.Create<FolderSelectionMessage>(msg =>
        {
            var (path, directories) = (msg.Response, this.MusicDirectories);

            if (!string.IsNullOrEmpty(path) && !directories.Contains(path))
            {
                directories.Add(path);
            }
        });

        private Command<string> _removeMusicDirectoryCommand;
        public Command<string> RemoveMusicDirectoryCommand => this._removeMusicDirectoryCommand
            ??= this.Commands.Create<string>(path => this.MusicDirectories.Remove(path));

        public ObservableList<string> ExcludeDirectories { get; }

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

        private Command<FolderSelectionMessage> _addExcludeDirectoryCommand;
        public Command<FolderSelectionMessage> AddExcludeDirectoryCommand => this._addExcludeDirectoryCommand ??= this.Commands.Create<FolderSelectionMessage>(msg =>
        {
            var (path, directories) = (msg.Response, this.ExcludeDirectories);

            if (!string.IsNullOrEmpty(path) && !directories.Contains(path))
            {
                directories.Add(path);
            }
        });

        private Command<string> _removeExcludeDirectoryCommand;
        public Command<string> RemoveExcludeDirectoryCommand => this._removeExcludeDirectoryCommand
            ??= this.Commands.Create<string>(path => this.ExcludeDirectories.Remove(path));

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

        public ObservableList<SoundDeviceInfo> SoundDevices { get; }
    }
}
