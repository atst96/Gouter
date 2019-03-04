using Gouter.Commands.SettingWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.ViewModels
{
    internal class SettingWindowViewModel : ViewModelBase
    {
        public SettingWindowViewModel()
        {
            this.Setting = App.Instance.Setting;
            this.MusicDirectories = this.Setting.MusicDirectories;

            if (this.MusicDirectories.Count == 0)
            {
                var musicDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                this.MusicDirectories.Add(musicDirectory);
            }
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
    }
}
