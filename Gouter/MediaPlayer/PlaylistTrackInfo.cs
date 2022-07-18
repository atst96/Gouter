namespace Gouter.MediaPlayer;

internal class PlaylistTrackInfo : TrackInfo
{
    /// <summary>
    /// プレイリストID
    /// </summary>
    public PlaylistInfo Playlist { get; }

    public PlaylistTrackInfo(PlaylistInfo playlist, TrackInfo trackInfo)
        : base(trackInfo)
    {
        this.Playlist = playlist;
    }
}
