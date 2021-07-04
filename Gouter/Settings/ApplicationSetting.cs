using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
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

        [Key("player.volume")]
        public float SoundVolumne { get; set; } = 1.0f;

        [Key("device.output_type")]
        public ushort SoundOutTypeInt { get; set; } = 0;

        [IgnoreMember]
        public SoundOutType SoundOutType
        {
            get => (SoundOutType)this.SoundOutTypeInt;
            set => this.SoundOutTypeInt = (ushort)value;
        }

        [Key("device.output_id")]
        public string SoundOutDeviceId { get; set; } = null;

        [Key("player.last_track")]
        public int? LastTrackId { get; set; } = null;

        [Key("player.last_playlist")]
        public int? LastPlaylistId { get; set; } = null;

        /// <summary>
        /// アルバムリストのスクロール位置
        /// </summary>
        [Key("interface.albums.scroll_position")]
        public double AlbumListScrollPosition { get; set; } = 0.0d;
    }
}
