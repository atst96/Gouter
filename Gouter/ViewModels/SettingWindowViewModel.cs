using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gouter.Managers;
using Gouter.Messaging;
using Gouter.Players;
using Livet.Messaging;
using NAudio.Wave;
using NAudio.Wave.Asio;

namespace Gouter.ViewModels;

internal class SettingWindowViewModel : ViewModelBase
{
    private static readonly App AppInstance = App.Instance;

    private readonly MediaManager _mediaManager = AppInstance.MediaManager;
    private readonly PlaylistPlayer _mediaPlayer = AppInstance.MediaPlayer;
    private readonly SoundDeviceListener _soundDeviceListener = AppInstance.SoundDeviceListener;

    /// <summary>
    /// Messenger
    /// </summary>
    public InteractionMessenger Messenger { get; } = new();

    /// <summary>
    /// ViewModelを生成する
    /// </summary>
    public SettingWindowViewModel()
    {
        this.Setting = App.Instance.Setting;

        this.MusicDirectories = this.Setting.MusicDirectories;
        this.ExcludeDirectories = this.Setting.ExcludeDirectories;

        if (this.MusicDirectories.Count == 0)
        {
            var musicDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            this.MusicDirectories.Add(musicDirectory);
        }

        this.SoundDevices = this._soundDeviceListener.Devices;

        this.AsioDeviceNames = new Collection<string>(AsioOut.GetDriverNames().ToList());

        this.UpdateAudioDeviceOptions();
    }

    public ApplicationSetting Setting { get; }

    public ObservableList<string> MusicDirectories { get; }

    private string _selectedMusicDirectory;
    public string SelectedMusicDirectory
    {
        get => this._selectedMusicDirectory;
        set
        {
            if (this.SetProperty(ref this._selectedMusicDirectory, value))
            {
                this._removeMusicDirectoryCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private Command<FolderSelectionMessage> _addMusicDirectoryCommand;
    public Command<FolderSelectionMessage> AddMusicDirectoryCommand => this._addMusicDirectoryCommand ??= this.Commands.Create<FolderSelectionMessage>(msg =>
    {
        var (path, directories) = (msg.Response, this.MusicDirectories);

        if (!string.IsNullOrEmpty(path) && !directories.Contains(path))
        {
            directories.Add(path);
        }
    });

    private Command<string> _removeMusicDirectoryCommand;
    public Command<string> RemoveMusicDirectoryCommand => this._removeMusicDirectoryCommand
        ??= this.Commands.Create<string>(path => this.MusicDirectories.Remove(path));

    public ObservableList<string> ExcludeDirectories { get; }

    private string _selectedExcludeDirectory;
    public string SelectedExcludeDirectory
    {
        get => this._selectedExcludeDirectory;
        set
        {
            if (this.SetProperty(ref this._selectedExcludeDirectory, value))
            {
                this._removeExcludeDirectoryCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private Command<FolderSelectionMessage> _addExcludeDirectoryCommand;
    public Command<FolderSelectionMessage> AddExcludeDirectoryCommand => this._addExcludeDirectoryCommand ??= this.Commands.Create<FolderSelectionMessage>(msg =>
    {
        var (path, directories) = (msg.Response, this.ExcludeDirectories);

        if (!string.IsNullOrEmpty(path) && !directories.Contains(path))
        {
            directories.Add(path);
        }
    });

    private Command<string> _removeExcludeDirectoryCommand;
    public Command<string> RemoveExcludeDirectoryCommand => this._removeExcludeDirectoryCommand
        ??= this.Commands.Create<string>(path => this.ExcludeDirectories.Remove(path));

    public BackendType SelectedBackendType
    {
        get => this.Setting.SoundOutType;
        set
        {
            this.Setting.SoundOutType = value;
            this.UpdateAudioDeviceOptions();
        }
    }

    /// <summary>
    /// DirectSound出力デバイス
    /// </summary>
    public string SelectedDirectSoundDevice
    {
        get => this.Setting.DirectSoundDevice;
        set => this.Setting.DirectSoundDevice = value;
    }

    /// <summary>
    /// WASAPI出力デバイス
    /// </summary>
    public string SelectedWasapiDevice
    {
        get => this.Setting.WasapiDevice;
        set => this.Setting.WasapiDevice = value;
    }

    /// <summary>
    /// WASAPIの占有モード設定
    /// </summary>
    public bool IsWasapiExclusiveMode
    {
        get => this.Setting.IsWasapiExclusiveMode;
        set => this.Setting.IsWasapiExclusiveMode = value;
    }

    /// <summary>
    /// ASIO出力デバイス
    /// </summary>
    public string SelectedAsioDevice
    {
        get => this.Setting.AsioDevice;
        set
        {
            if (this.Setting.AsioDevice != value)
            {
                this.Setting.AsioDevice = value;
                this._openAsioControlPanel?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// オーディオバックエンドの種別([Type]: 種別名)
    /// </summary>
    public IReadOnlyDictionary<BackendType, string> BackendTypes { get; } = new Dictionary<BackendType, string>
    {
        [BackendType.DirectSound] = "DirectSound",
        [BackendType.Wasapi] = "WASAPI",
        [BackendType.ASIO] = "ASIO",
    };

    /// <summary>
    /// サウンドデバイス情報リスト
    /// </summary>
    public ObservableList<SoundDeviceInfo> SoundDevices { get; }

    /// <summary>
    /// ASIOデバイス名リスト
    /// </summary>
    public Collection<string> AsioDeviceNames { get; }

    private Command _openAsioControlPanel;

    /// <summary>
    /// ASIOコントロールパネルを開くコマンド
    /// </summary>
    public Command OpenAsioControlPanel => this._openAsioControlPanel ??= this.Commands.Create(() =>
    {
        var asio = AsioDriver.GetAsioDriverByName(this.SelectedAsioDevice);

        try
        {
            asio.ControlPanel();
        }
        catch
        {
            // pass
        }
    });

    /// <summary>
    /// DirectSoundオプションの表示フラグ
    /// </summary>
    public bool IsVisibleDirectSoundOptions { get; private set; }

    /// <summary>
    /// WASAPIオプションの表示フラグ
    /// </summary>
    public bool IsVisibleWasapiOptions { get; private set; }

    /// <summary>
    /// ASIOオプションの表示フラグ
    /// </summary>
    public bool IsVisibleAsioOptions { get; private set; }

    /// <summary>
    /// オーディオ設定の表示切替
    /// </summary>
    public void UpdateAudioDeviceOptions()
    {
        this.IsVisibleDirectSoundOptions = this.SelectedBackendType == BackendType.DirectSound;
        this.IsVisibleWasapiOptions = this.SelectedBackendType == BackendType.Wasapi;
        this.IsVisibleAsioOptions = this.SelectedBackendType == BackendType.ASIO;

        this.RaisePropertyChanged(nameof(this.IsVisibleDirectSoundOptions));
        this.RaisePropertyChanged(nameof(this.IsVisibleWasapiOptions));
        this.RaisePropertyChanged(nameof(this.IsVisibleAsioOptions));
    }
}
