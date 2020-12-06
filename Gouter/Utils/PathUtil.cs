using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Gouter.Utils
{
    /// <summary>
    /// ファイルパス関連のユーティリティクラス
    /// </summary>
    internal static class PathUtil
    {
        /// <summary>ディレクトリセパレータ</summary>
        private static readonly string DirectorySeparator = Path.DirectorySeparatorChar.ToString();

        /// <summary>
        /// ディレクトリンパスを<see cref="DirectorySeparator"/>終わりにする。
        /// </summary>
        /// <param name="path">ディレクトリパス</param>
        /// <returns></returns>
        public static string AlignDirectoryPath(string path)
        {
            if (path.EndsWith(DirectorySeparator))
            {
                return path;
            }

            return path + DirectorySeparator;
        }

        /// <summary>
        /// ディレクトリ
        /// </summary>
        /// <param name="directoryPaths">ディレクトリパス一覧</param>
        /// <returns>正規化済みディレクトリ一覧</returns>
        public static IReadOnlyList<string> ExcludeSubDirectories(IReadOnlyCollection<string> directoryPaths)
        {
            var directories = directoryPaths
                .Select(path => AlignDirectoryPath(path))
                .ToList();

            // サブディレクトリが含まれている場合は無視する
            for (int i = directories.Count - 1; i >= 0; --i)
            {
                if (PathUtil.IsContains(directories[i], directories))
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
        public static bool IsContains(string path, IReadOnlyCollection<string> directories)
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
        public static readonly ImmutableHashSet<string> SupportedMediaExtensions = ImmutableHashSet.Create(new []
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

            return PathUtil.SupportedMediaExtensions.Contains(extension);
        }

        /// <summary>
        /// 埋め込みリソースのパスを取得する。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>埋め込みリソースのパス</returns>
        internal static string GetEmbeddedResourcePath(string path)
            => $"pack://application:,,,/Resources/{path}";
    }
}
