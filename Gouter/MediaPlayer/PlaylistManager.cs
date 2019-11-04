using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    /// <summary>
    /// プレイリストの管理を行うクラス
    /// </summary>
    internal class PlaylistManager : IAlbumObserver, IDisposable
    {
        /// <summary>データベース</summary>
        private readonly Database _database;

        /// <summary>アルバムマネージャ</summary>
        private readonly AlbumManager _albumManager;

        /// <summary>アルバムプレイリストの一覧</summary>
        public NotifiableCollection<AlbumPlaylist> Albums { get; } = new NotifiableCollection<AlbumPlaylist>();

        /// <summary>PlaylistManagerを生成する</summary>
        /// <param name="database">データベース</param>
        /// <param name="albumManager">アルバム情報</param>
        public PlaylistManager(Database database, AlbumManager albumManager)
        {
            this._database = database ?? throw new InvalidOperationException();
            this._albumManager = albumManager ?? throw new InvalidOperationException();

            albumManager.Subscribe(this);
        }

        /// <summary>アルバム情報の登録通知</summary>
        /// <param name="albumInfo">アルバム情報</param>
        void IAlbumObserver.OnRegistered(AlbumInfo albumInfo)
        {
            this.Albums.Add(albumInfo.Playlist);
        }

        /// <summary>アルバム情報の削除通知</summary>
        /// <param name="albumInfo">アルバム情報</param>
        void IAlbumObserver.OnRemoved(AlbumInfo albumInfo)
        {
            this.Albums.Remove(albumInfo.Playlist);
        }

        /// <summary>リソースを破棄する</summary>
        public void Dispose()
        {
            this._albumManager.Describe(this);
        }
    }
}
