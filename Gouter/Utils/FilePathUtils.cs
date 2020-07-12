using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

namespace Gouter.Utils
{
    /// <summary>
    /// ファイルパス関連のユーティリティクラス
    /// </summary>
    internal static class FilePathUtils
    {
        /// <summary>ディレクトリセパレータ</summary>
        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        /// <summary>
        /// ディレクトリパスを正規化する
        /// </summary>
        /// <param name="paths">ディレクトリパス一覧</param>
        /// <returns>正規化済みディレクトリ一覧</returns>
        public static IReadOnlyList<string> NormalizeDirectories(IReadOnlyCollection<string> paths)
        {
            var directories = paths
                .Select(path => path.EndsWith(DirectorySeparator) ? path : (path + DirectorySeparator))
                .ToList();

            for (int i = directories.Count - 1; i >= 0; --i)
            {
                if (FilePathUtils.IsContainsDirectory(directories[i], directories))
                {
                    directories.RemoveAt(i);
                }
            }

            return directories;
        }

        /// <summary>
        /// 再帰検索の際にディレクトリが重複しないかを検証する
        /// </summary>
        /// <param name="path">検証パス</param>
        /// <param name="directories">ディレクトリ一覧</param>
        /// <returns>ディレクトリの重複有無</returns>
        public static bool IsContainsDirectory(string path, IReadOnlyCollection<string> directories)
            => directories.Any(dir => !path.Equals(dir) && path.StartsWith(dir));

        /// <summary>
        /// ディレクトリ配下のファイルを列挙する。
        /// </summary>
        /// <param name="directoryPath">検索ディレクトリ</param>
        /// <param name="isRecursive">再帰的に検索を行うかどうかのフラグ</param>
        /// <returns>ファイルリスト</returns>
        public static IReadOnlyList<string> GetFiles(string directoryPath, bool isRecursive)
        {
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Directory.GetFiles(directoryPath, "*.*", searchOption);
        }

        /// <summary>
        /// 検索するファイルの拡張子
        /// </summary>
        public static readonly ImmutableHashSet<string> SupportedMediaExtensions = ImmutableHashSet.Create(new string[]
        {
            ".wav", ".mp3", ".acc", ".m4a", ".flac", ".ogg",
        });

        /// <summary>
        /// 対応するメディアの拡張子かどうかを判定する
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>対応メディア</returns>
        public static bool IsSupportedMediaExtension(string path)
        {
            var extension = Path.GetExtension(path);

            return FilePathUtils.SupportedMediaExtensions.Contains(extension);
        }
    }
}
