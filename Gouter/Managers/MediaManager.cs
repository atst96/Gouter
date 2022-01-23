using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ATL;

namespace Gouter.Managers;

/// <summary>
/// メディアの管理を行うクラス
/// </summary>
internal class MediaManager : IDisposable
{
    /// <summary>
    /// トラック情報の登録状況変更時
    /// </summary>
    public event EventHandler<TrackRegisterProgress> TrackRegisterStateChanged;

    /// <summary>
    /// データベース情報
    /// </summary>
    private Database _database;

    /// <summary>
    /// ライブラリのファイル名
    /// </summary>
    public string LibraryPath { get; private set; }

    /// <summary>
    /// 初期化済みかどうかのフラグ
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// トラック情報管理
    /// </summary>
    public TrackManager Tracks { get; private set; }

    /// <summary>
    /// アートワーク
    /// </summary>
    public ArtworkManager Artwork { get; private set; }

    /// <summary>
    /// アルバム情報管理
    /// </summary>
    public AlbumManager Albums { get; private set; }

    /// <summary>
    /// プレイリスト情報管理
    /// </summary>
    public PlaylistManager Playlists { get; private set; }

    /// <summary>
    /// 読み込み完了イベント
    /// </summary>
    public event EventHandler Loaded;

    /// <summary>
    /// メディア管理クラスを生成する。
    /// </summary>
    /// <param name="libInfo"></param>
    public MediaManager(string artworkPath)
    {
        this._database = new Database();

        this.Tracks = new TrackManager(this._database);
        this.Artwork = new ArtworkManager(artworkPath);
        this.Albums = new AlbumManager(this._database, this.Artwork);
        this.Playlists = new PlaylistManager(this._database, this.Albums);
    }

    /// <summary>
    /// メディア管理クラスを生成する。
    /// </summary>
    /// <param name="libraryPath"></param>
    /// <returns></returns>
    public static MediaManager CreateMediaManager(string libraryPath, string artworkPath)
    {
        var manager = new MediaManager(artworkPath);
        manager.Initialize(libraryPath);

        return manager;
    }

    /// <summary>
    /// 初期化処理を行う。
    /// </summary>
    public void Initialize(string libraryPath)
    {
        if (this.IsInitialized)
        {
            // 初期化処理は1度のみ
            throw new InvalidOperationException();
        }

        this.IsInitialized = true;

        // DB接続と準備処理
        var db = this._database;
        db.Connect(libraryPath);
    }

    /// <summary>
    /// ライブラリを読み込む。
    /// </summary>
    /// <returns></returns>
    public Task LoadLibrary() => Task.Run(() =>
    {
        this.Albums.Load();
        this.Tracks.Load(this.Albums);
        this.Playlists.Load();
        this.Loaded?.Invoke(this, new());
    });

    /// <summary>
    /// トラックを登録する
    /// </summary>
    /// <param name="track">トラック情報</param>
    public void RegisterTrack(Track track)
    {
        int newTrackId = this.Tracks.GenerateId();
        var albumInfo = this.Albums.GetOrAddAlbum(track);

        var trackInfo = new TrackInfo(newTrackId, track, albumInfo);
        this.Tracks.Add(trackInfo);
    }

    /// <summary>トラックを一括登録する</summary>
    /// <param name="newTracks">トラック</param>
    /// <param name="progress"></param>
    public void RegisterTracks(IReadOnlyCollection<Track> newTracks)
    {
        _ = newTracks ?? throw new ArgumentNullException(nameof(newTracks));

        int count = 0;
        int maxCount = newTracks.Count;

        using var transaction = this._database.BeginTransaction();

        try
        {
            // トラック情報をアルバム毎にグループ化
            var tracksByAlbumKey = newTracks
                .AsParallel()
                .GroupBy(t => t.GetAlbumKey());

            foreach (var albumTracks in tracksByAlbumKey)
            {
                var albumKey = albumTracks.Key;

                AlbumInfo albumInfo;
                if (!this.Albums.TryGetFromKey(albumKey, out albumInfo))
                {
                    // 新規トラックを登録する

                    // 1トラック目のアルバム
                    var firstTrack = albumTracks
                        .Aggregate((left, right) => TrackComparer.Instance.Compare(left, right) < 0 ? left : right);

                    albumInfo = this.Albums.GetOrAddAlbum(firstTrack);
                }

                var tracks = albumTracks.Select(t => new TrackInfo(this.Tracks.GenerateId(), t, albumInfo)).ToImmutableList();
                this.Tracks.Add(tracks);
                albumInfo.Playlist.Tracks.AddRange(tracks);

                count += tracks.Count;
                this.TrackRegisterStateChanged?.Invoke(this, new(TrackRegisterState.InProgress, count, maxCount));
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Flush()
    {
        this._database.Flush();
    }

    /// <summary>
    /// リソースを解放する
    /// </summary>
    public void Dispose()
    {
        this._database?.Dispose();
    }

    /// <summary>
    /// 新しい楽曲ファイルの検索を行い、アルバムとトラックに登録する
    /// </summary>
    /// <param name="musicDirectories">楽曲ファイルの検索を行うディレクトリ</param>
    /// <param name="excludeDirectories">除外する楽曲ファイルのディレクトリ</param>
    /// <param name="excludePaths">除外する楽曲ファイルのパス</param>
    /// <returns></returns>
    public void SearchAndRegisterNewTracks(
        IReadOnlyCollection<string> musicDirectories,
        IReadOnlyCollection<string> excludeDirectories,
        IReadOnlyCollection<string> excludePaths)
    {
        // 新規トラック情報検索開始
        this.TrackRegisterStateChanged?.Invoke(this, new(TrackRegisterState.Collecting));

        var newTracks = TrackFinder.FindUnregistered(this.Tracks,
            musicDirectories, excludeDirectories, excludePaths);

        int trackCount = newTracks.Count;
        if (trackCount <= 0)
        {
            // 新規トラック情報なし
            this.TrackRegisterStateChanged?.Invoke(this, new(TrackRegisterState.NotFound));
            return;
        }

        // トラック情報登録開始
        this.TrackRegisterStateChanged?.Invoke(this, new(TrackRegisterState.InProgress, 0, trackCount));

        this.RegisterTracks(newTracks);
        this.Flush();

        // トラック情報登録完了
        this.TrackRegisterStateChanged?.Invoke(this, new(TrackRegisterState.Complete, trackCount, trackCount));

        // TODO: メッセージのメモ
        // this._viewModel.Status = $"{newTracks.Count}件の楽曲が見つかりました。楽曲情報をライブラリに登録しています...";
        // this._viewModel.Status = $"{newTracks.Count}件の楽曲が追加されました";
    }
}
