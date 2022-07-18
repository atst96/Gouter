using System.ComponentModel;
using System.Windows.Data;
using Gouter.Playlists;

namespace Gouter.ViewModels;

/// <summary>
/// アルバムのトラック情報リスト
/// </summary>
internal class AlbumTrackViewModel : TracksViewModelBase<AlbumPlaylist, AlbumInfo, TrackInfo>
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="playlist"></param>
    public AlbumTrackViewModel(AlbumPlaylist playlist, ObservableList<CustomPlaylist> playlists)
        : base(playlist, playlist.Album, playlist.Tracks, CreateCollectionViewSource(playlist), playlists)
    {
    }

    /// <summary>
    /// CollectionViewSourceを生成する
    /// </summary>
    /// <param name="playlist">プレイリスト情報</param>
    /// <returns></returns>
    private static CollectionViewSource CreateCollectionViewSource(AlbumPlaylist playlist)
    {
        var trackViewSource = new CollectionViewSource
        {
            Source = playlist.Tracks,
        };

        var groupDescriptions = trackViewSource.GroupDescriptions;
        var liveGroupingProperties = trackViewSource.LiveGroupingProperties;

        groupDescriptions.Add(new PropertyGroupDescription(nameof(TrackInfo.DiskNumber)));
        liveGroupingProperties.Add(nameof(TrackInfo.DiskNumber));

        var sortDescriptions = trackViewSource.SortDescriptions;
        var liveSortingDescriptions = trackViewSource.LiveSortingProperties;
        sortDescriptions.Add(new SortDescription(nameof(TrackInfo.DiskNumber), ListSortDirection.Ascending));
        sortDescriptions.Add(new SortDescription(nameof(TrackInfo.TrackNumber), ListSortDirection.Ascending));
        sortDescriptions.Add(new SortDescription(nameof(TrackInfo.Title), ListSortDirection.Ascending));
        liveSortingDescriptions.Add(nameof(TrackInfo.DiskNumber));
        liveSortingDescriptions.Add(nameof(TrackInfo.TrackNumber));
        liveSortingDescriptions.Add(nameof(TrackInfo.Title));

        return trackViewSource;
    }
}
