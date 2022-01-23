using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ATL;
using Gouter.Utils;

namespace Gouter.Managers;

internal static class TrackFinder
{
    /// <summary>
    /// 未登録の楽曲情報を取得する。
    /// </summary>
    /// <param name="registeredTracks">登録済みトラック情報</param>
    /// <param name="musicDirectories">音楽ファイルのディレクトリ</param>
    /// <param name="excludeDirectories">除外するディレクトリ</param>
    /// <param name="excludeFilePaths">除外するディレクトリのパス</param>
    /// <returns>楽曲情報リスト</returns>
    public static IReadOnlyList<Track> FindUnregistered(
        IEnumerable<TrackInfo> registeredTracks,
        IReadOnlyCollection<string> musicDirectories,
        IReadOnlyCollection<string> excludeDirectories,
        IReadOnlyCollection<string> excludeFilePaths)
    {
        // 登録済みファイルのパスを列挙
        var registeredFilePaths = registeredTracks.Select(t => t.Path);

        var findDirs = PathUtil.ExcludeSubDirectories(musicDirectories);
        var excludeDirs = PathUtil.ExcludeSubDirectories(excludeDirectories);
        var excludePaths = excludeFilePaths.ToHashSet();

        // 未登録ファイルのパスを列挙
        var unregisteredFiles = findDirs
            .SelectMany(path => PathUtil.GetFiles(path, true))
            .Except(excludeFilePaths)
            .Except(registeredFilePaths)
            .Except(excludePaths)
            .Distinct()
            .AsParallel()
            .Where(path =>
                PathUtil.IsSupportedMediaExtension(path)
                && !PathUtil.IsContains(path, excludeDirs))
            .ToList();

        // トラック情報を取得する
        var tracks = new List<Track>(unregisteredFiles.Count);

        foreach (var path in unregisteredFiles)
        {
            try
            {
                tracks.Add(new Track(path));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"読み込み失敗: {path}, {ex}");
            }
        }

        return tracks;
    }
}
