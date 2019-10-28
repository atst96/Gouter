using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    /// <summary>
    /// メディアの総括的管理を行うクラス
    /// </summary>
    internal class MediaPlayer : IDisposable
    {
        /// <summary>ライブラリ管理を行う</summary>
        public LibraryManager Library { get; } = new LibraryManager();

        /// <summary>再生管理を行う</summary>
        private readonly SoundPlayer _soundPlayer;

        /// <summary>デバイス管理を行う</summary>
        public SoundDeviceManager DeviceManager { get; } = new SoundDeviceManager();

        /// <summary>コンストラクタ</summary>
        public MediaPlayer()
        {
            this._soundPlayer = new SoundPlayer();
        }

        /// <summary>初期化する</summary>
        /// <param name="libraryFilePath"></param>
        public void Initialize(string libraryFilePath)
        {
            this.Library.Initialize(libraryFilePath);
        }

        public void Close()
        {
            this.Library.Dispose();
            this.DeviceManager.Dispose();
            this._soundPlayer.Dispose();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
