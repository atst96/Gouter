using System.IO;

namespace Gouter.Utils
{
    /// <summary>
    /// ファイル操作に関するユーティリティクラス
    /// </summary>
    internal static class FileUtil
    {
        /// <summary>
        /// 読み取りモートでファイルのストリームを開く。
        /// </summary>
        /// <param name="path"></param>
        /// <returns><see cref="FileStream"/></returns>
        public static FileStream OpenRead(string path)
        {
            var fileInfo = new FileInfo(path);

            return fileInfo.Exists && fileInfo.Length > 0
                ? fileInfo.OpenRead()
                : null;
        }

        /// <summary>
        /// 書き込みモートでファイルのストリームを開く。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns><see cref="FileStream"/></returns>
        public static FileStream OpenCreate(string path)
            => File.Open(path, FileMode.Create, FileAccess.Write);
    }
}
