using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ATL;
using ATL.AudioData;

namespace Gouter.Utils
{
    internal static class TrackUtil
    {
        private static readonly Factory _audioFileUtil = AudioDataIOFactory.GetInstance();

        private static Dictionary<string, string> _extMimeTypeMap = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// ファイルパスからMIME-TYPEを取得する。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>取得できなかった場合はnullを返す。</returns>
        public static string? GetMimeTypeFromPath(string path)
        {
            var ext = Path.GetExtension(path);

            string mimeType;
            if (_extMimeTypeMap.TryGetValue(ext, out mimeType))
            {
                return mimeType;
            }

            var formats = _audioFileUtil.getFormatsFromPath(path);
            mimeType = formats?
                .FirstOrDefault()
                .MimeList
                .FirstOrDefault();

            _extMimeTypeMap.Add(ext, mimeType);

            return mimeType;
        }
    }
}
