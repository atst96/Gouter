using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Gouter.Components.Mvvm;
using Gouter.Playlists;

namespace Gouter.ViewModels;


internal class TracksViewModelBase<TPlaylist, TPlaylistInfo, TTrack> : ViewModelBase
    where TPlaylist : IPlaylist
    where TPlaylistInfo : IPlaylistInfo
    where TTrack : TrackInfo
{
    /// <summary>
    /// アルバム情報
    /// </summary>
    public TPlaylist Playlist { get; }

    /// <summary>
    /// アルバム情報
    /// </summary>
    public TPlaylistInfo Album { get; }

    /// <summary>
    /// トラックリスト
    /// </summary>
    public ObservableList<TTrack> Tracks { get; }

    /// <summary>
    /// </summary>
    public CollectionViewSource TrackViewSource { get; }

    /// <summary>
    /// プレイリスト
    /// </summary>
    public ObservableList<CustomPlaylist> Playlists { get; }

    public IEnumerable<object> SelectedTracks { get; set; }

    protected TracksViewModelBase(TPlaylist playlist, TPlaylistInfo album, ObservableList<TTrack> tracks, CollectionViewSource trackViewSource, ObservableList<CustomPlaylist> playlists)
    {
        this.Playlist = playlist;
        this.Album = album;
        this.Tracks = tracks;
        this.TrackViewSource = trackViewSource;
        this.Playlists = playlists;
    }

    private TrackInfo _selectedTrack;

    /// <summary>
    /// 選択中のトラック
    /// </summary>
    public TrackInfo SelectedTrack
    {
        get => this._selectedTrack;
        set => this.SetProperty(ref this._selectedTrack, value);
    }

    private Command<TrackInfo> _trackPlayCommand;

    /// <summary>
    /// トラックのダブルクリック時のコマンド
    /// </summary>
    public Command<TrackInfo> TrackPlayCommand => this._trackPlayCommand
        ??= this.Commands.Create<TrackInfo>(this.OnPlayCommandExecute, track => track != null);

    private void OnPlayCommandExecute(TrackInfo track)
    {
        var player = App.Instance.MediaPlayer;

        player.Play(track, this.Playlist);
    }

    /// <summary>
    /// 選択中のすべてのトラックを取得する
    /// </summary>
    /// <returns></returns>
    protected IList<TrackInfo> GetSelectedTracks()
    {
        return this.SelectedTracks.OfType<TrackInfo>().ToList();
    }

    private Command<CustomPlaylist> _addPlaylistCommand;
    public Command<CustomPlaylist> AddPlaylistCommand => this._addPlaylistCommand
        ??= this.Commands.Create<CustomPlaylist>(i =>
        {
            App.Instance.MediaManager.CustomPlaylists.Add(i.Playlist, this.GetSelectedTracks());
        }, i => i != null);
}
