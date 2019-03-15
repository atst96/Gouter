using Gouter.Commands.MainWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gouter.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public SortedNotifiableCollectionWrapper<AlbumInfo> Albums { get; }

        public MainWindowViewModel() : base()
        {
            this.Albums = new SortedNotifiableCollectionWrapper<AlbumInfo>(App.AlbumManager.Albums, AlbumComparer.Instance);

            BindingOperations.EnableCollectionSynchronization(this.Albums, new object());
        }

        private Command _initializeCommand;
        public Command InitializeCommand => this._initializeCommand ?? (this._initializeCommand = new InitializeCommand(this));

        private IPlaylist _currentPlaylist;
        public IPlaylist CurrentPlaylist
        {
            get => this._currentPlaylist;
            set => this.SetProperty(ref this._currentPlaylist, value);
        }

        private AlbumInfo _selectedAlbum;
        public AlbumInfo SelectedAlbum
        {
            get => this._selectedAlbum;
            set
            {
                if (this.SetProperty(ref this._selectedAlbum, value))
                {
                    this.CurrentPlaylist = new AlbumPlaylist(value);
                }
            }
        }
        
        public StandardProgressReceiver LoadProgress { get; } = new StandardProgressReceiver();
    }
}
