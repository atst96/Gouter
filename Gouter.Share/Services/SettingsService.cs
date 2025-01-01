using Gouter.Constants;
using Gouter.Models;
using Gouter.Utils;

namespace Gouter.Services;

/// <summary>
/// 設定情報管理クラス
/// </summary>
public class SettingsService
{
    private string _filePath;
    private ApplicationSetting? _settings;

    public ApplicationSetting Settings
        => this._settings ?? throw new InvalidOperationException();

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SettingsService()
    {
        this._filePath = AssemblyUtils.GetLocalPath(Config.SettingFileName);
    }

    /// <summary>
    /// 設定情報を読み込む
    /// </summary>
    /// <returns></returns>
    public async ValueTask Load()
    {
        this._settings = await MessagePackUtil.DeserializeFileAsync<ApplicationSetting>(this._filePath)
            .ConfigureAwait(false) ?? this.GetNewSettings();
    }

    /// <summary>
    /// 新しい設定情報を生成する
    /// </summary>
    /// <returns></returns>
    private ApplicationSetting GetNewSettings()
        => new()
        {
            MusicDirectories = [Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)],
        };

    /// <summary>
    /// 設定情報を保存する
    /// </summary>
    /// <returns></returns>
    public ValueTask Save()
    {
        // 書き込み前にバイト配列を取り出す。
        // (例外発生時に0バイトデータが作成されるのを防ぐ)
        return MessagePackUtil.SerializeFileAsync(this.Settings, this._filePath);
    }
}
