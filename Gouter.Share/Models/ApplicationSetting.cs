using MessagePack;

namespace Gouter.Models;

/// <summary>
/// アプリケーション設定
/// </summary>
[MessagePackObject]
public class ApplicationSetting : NotificationObject
{
    /// <summary>楽曲ファイルのディレクトリ</summary>
    [Key("directory.musics")]
    public List<string> MusicDirectories { get; set; } = new();

    /// <summary>楽曲ファイルの探索から除外するディレクトリ</summary>
    [Key("directory.excludes")]
    public List<string> ExcludeDirectories { get; set; } = new();

    /// <summary>プレーヤの音量</summary>
    [Key("player.volume")]
    public float SoundVolume { get; set; } = 1.0f;

    /// <summary>オーディオバックエンドの種別</summary>
    [Key("device.backend_type")]
    public BackendType SoundOutType { get; set; } = BackendType.Wasapi;

    /// <summary>DirectSoundオーディオデバイスのID</summary>
    [Key("device.direct_sound.device")]
    public Guid? DirectSoundDevice { get; set; } = null;

    /// <summary>ASIOオーディオデバイスのID</summary>
    [Key("device.wasapi.device")]
    public string? WasapiDevice { get; set; } = null;

    /// <summary>ASIOデバイスの占有モード設定</summary>
    [Key("device.wasapi.is_exclusive")]
    public bool IsWasapiExclusiveMode { get; set; } = false;

    /// <summary>ASIOオーディオデバイスのID</summary>
    [Key("device.asio.device")]
    public string? AsioDevice { get; set; } = null;

    /// <summary>最後に再生したトラックのID</summary>
    [Key("player.last_track")]
    public int? LastTrackId { get; set; } = null;

    /// <summary>最後の再生したプレイリストのID</summary>
    [Key("player.last_playlist")]
    public int? LastPlaylistId { get; set; } = null;

    /// <summary>アルバムリストのスクロール位置</summary>
    [Key("interface.albums.scroll_position")]
    public double AlbumListScrollPosition { get; set; } = 0.0d;
}
