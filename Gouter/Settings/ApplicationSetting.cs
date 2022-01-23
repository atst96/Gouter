using MessagePack;

namespace Gouter;

[MessagePackObject]
internal class ApplicationSetting : NotificationObject
{
    [Key("directory.musics")]
    private ObservableList<string> _musicDirectories;

    [IgnoreMember]
    public ObservableList<string> MusicDirectories => this._musicDirectories ??= new ObservableList<string>();

    [Key("directory.excludes")]
    private ObservableList<string> _excludeDirectories;

    [IgnoreMember]
    public ObservableList<string> ExcludeDirectories => this._excludeDirectories ??= new ObservableList<string>();

    /// <summary>
    /// プレーやの音量
    /// </summary>
    [Key("player.volume")]
    public float SoundVolumne { get; set; } = 1.0f;

    /// <summary>
    /// オーディオバックエンドの種別
    /// </summary>
    [Key("device.backend_type")]
    public BackendType SoundOutType { get; set; } = BackendType.Wasapi;


    /// <summary>
    /// DirectSoundオーディオデバイスのID
    /// </summary>
    [Key("device.direct_sound.device")]
    public string DirectSoundDevice { get; set; } = null;


    /// <summary>
    /// ASIOオーディオデバイスのID
    /// </summary>
    [Key("device.wasapi.device")]
    public string WasapiDevice { get; set; } = null;


    /// <summary>
    /// ASIOデバイスの占有モード設定
    /// </summary>
    [Key("device.wasapi.is_exclusive")]
    public bool IsWasapiExclusiveMode { get; set; } = false;


    /// <summary>
    /// ASIOオーディオデバイスのID
    /// </summary>
    [Key("device.asio.device")]
    public string AsioDevice { get; set; } = null;

    /// <summary>
    /// 最後に再生したトラックのID
    /// </summary>
    [Key("player.last_track")]
    public int? LastTrackId { get; set; } = null;

    /// <summary>
    /// 最後の再生したプレイリストのID
    /// </summary>
    [Key("player.last_playlist")]
    public int? LastPlaylistId { get; set; } = null;

    /// <summary>
    /// アルバムリストのスクロール位置
    /// </summary>
    [Key("interface.albums.scroll_position")]
    public double AlbumListScrollPosition { get; set; } = 0.0d;
}
