using System;

namespace Gouter.Players;

/// <summary>
/// プレーヤ設定
/// </summary>
internal class PlayerOptions
{
    /// <summary>
    /// ループモードを設定する。
    /// </summary>
    public LoopMode LoopMode { get; set; }

    /// <summary>
    /// シャッフルモードを設定する。
    /// </summary>
    public ShuffleMode ShuffleMode { get; set; }

    /// <summary>
    /// シャッフル再生時の次トラック選択で現在のトラックを回避するかどうかを取得する。
    /// </summary>
    public bool IsShuffleAvoidCurrentTrack { get; set; }

    /// <summary>
    /// フェードイン／アウトを有効にするか取得する。
    /// </summary>
    public bool IsEnableFadeInOut { get; set; }

    /// <summary>
    /// フェードイン・アウトに要する時間を取得する。
    /// </summary>
    public TimeSpan FadeInOutDuration { get; set; }
}
