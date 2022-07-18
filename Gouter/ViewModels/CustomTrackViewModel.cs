using System.Windows.Data;
using Gouter.MediaPlayer;
using Gouter.Playlists;

namespace Gouter.ViewModels;

internal class CustomPlaylistTrackViewModel : TracksViewModelBase<CustomPlaylist, PlaylistInfo, PlaylistTrackInfo>
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="playlist"></param>
    public CustomPlaylistTrackViewModel(CustomPlaylist playlist, ObservableList<CustomPlaylist> playlists)
        : base(playlist, playlist.Playlist, playlist.Tracks, CreateCollectionViewSource(playlist), playlists)
    {
    }


    /// <summary>
    /// CollectionViewSourceを生成する
    /// </summary>
    /// <param name="playlist">プレイリスト情報</param>
    /// <returns></returns>
    private static CollectionViewSource CreateCollectionViewSource(CustomPlaylist playlist)
    {
        var trackViewSource = new CollectionViewSource
        {
            Source = playlist.Tracks,
        };

        return trackViewSource;
    }
}
