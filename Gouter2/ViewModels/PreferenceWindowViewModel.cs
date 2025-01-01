using Gouter.Collections;
using Gouter.Models;
using Gouter.Services;

namespace Gouter.ViewModels;

public class PreferenceWindowViewModel : ViewModelBase
{
    private ApplicationSetting _settings;

    public PreferenceWindowViewModel(SettingsService settingService)
    {
        this._settings = settingService.Settings;
        this.RestoreSetting(this._settings);
    }

    /// <summary>楽曲ファイルの検索ディレクトリリスト</summary>
    public ObservableList<string> FindDirectories { get; } = [];

    /// <summary>検索から除外するディレクトリ リスト</summary>
    public ObservableList<string> ExcludeDirectories { get; } = [];

    /// <summary>
    /// 設定情報をUIに反映する
    /// </summary>
    /// <param name="settings">設定情報</param>
    private void RestoreSetting(ApplicationSetting settings)
    {
        this.FindDirectories.Reset(settings.MusicDirectories ?? ["test"]);
        this.ExcludeDirectories.Reset(settings.ExcludeDirectories ?? []);
    }

    /// <summary>
    /// UIの状態を設定情報に反映する
    /// </summary>
    /// <param name="settings">設定情報</param>
    private void ApplySetting(ApplicationSetting settings)
    {
        settings.MusicDirectories = [.. this.FindDirectories];
        settings.ExcludeDirectories = [.. this.ExcludeDirectories];
    }
}
