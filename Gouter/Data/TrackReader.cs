#nullable enable

using System;
using System.IO;
using ATL;
using Gouter.Utils;

namespace Gouter.Data
{
    /// <summary>
    /// トラック情報読み取りクラス
    /// </summary>
    internal class TrackReader : IDisposable
    {
        /// <summary>
        /// FileStreamのバッファサイズ
        /// </summary>
        private const int BufferSize = 2048;

        /// <summary>
        /// FileStreamのオプション
        /// </summary>
        private const FileOptions StreamOptions = FileOptions.Asynchronous
            // | FileOptions.RandomAccess
            ;

        /// <summary>
        /// 破棄済みフラグ
        /// </summary>
        private bool _idDisposed = false;

        /// <summary>
        /// ストリーム
        /// </summary>
        private Stream? _stream;

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// ファイルのMIME-TYPE
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// トラック情報
        /// </summary>
        public Track Track { get; private set; }

        /// <summary>
        /// トラック情報読み取りクラスの生成
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="mimeType">MIME-TYPE</param>
        public TrackReader(string path, string? mimeType = null)
        {
            this.Path = path;
            this.MimeType = mimeType ?? TrackUtil.GetMimeTypeFromPath(path) ?? throw new NotSupportedException();
            this._stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, StreamOptions);
            this.Track = new Track(this._stream, this.MimeType);
        }

        /// <summary>
        /// <see cref="TrackReader"/>を生成する。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="mimeType">MIME-TYPE</param>
        /// <returns></returns>
        public static TrackReader Crate(string path, string? mimeType = null)
            => new TrackReader(path, mimeType);

        /// <summary>
        /// トラック情報を取得する。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="mimeType">MIME-TYPE</param>
        /// <returns></returns>
        public static Track? GetTrack(string path, string? mimeType = null)
        {
            using var reader = Crate(path, mimeType);
            return reader?.Track;
        }

        /// <summary>
        /// インスタンスを破棄する。
        /// </summary>
        void IDisposable.Dispose()
        {
            if (!this._idDisposed)
            {
                this._stream?.Dispose();
                this._stream = null;
                this._idDisposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}
