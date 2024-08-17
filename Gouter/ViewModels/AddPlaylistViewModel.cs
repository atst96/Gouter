using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.ViewModels;

internal class MusicAddToPlaylistViewModel : NotificationObject
{
    private IPlaylistInfo _playlist;

    public MusicAddToPlaylistViewModel(IPlaylistInfo playlist)
    {
        this._playlist = playlist;
    }
}
