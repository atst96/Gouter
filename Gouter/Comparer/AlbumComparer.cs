using System.Collections.Generic;
using Gouter.Managers;

namespace Gouter;

/// <summary>
/// アルバム情報の比較を行うComparer
/// </summary>
internal class AlbumComparer : IComparer<AlbumInfo>, IComparer<AlbumPlaylist>
{
    /// <summary>インスタンス</summary>
    public static readonly AlbumComparer Instance = new AlbumComparer();

    /// <summary>Comparer</summary>
    private static readonly IComparer<string> _stringComparer = AlbumManager.AlbumNameComparer;

    /// <summary>AlbumInfo同士の比較を行う</summary>
    /// <param name="x">左辺</param>
    /// <param name="y">右辺</param>
    /// <returns>比較結果</returns>
    public int Compare(AlbumInfo x, AlbumInfo y)
    {
        return _stringComparer.Compare(x.Key, y.Key);
    }

    /// <summary>AlbumPlaylist同士の比較を行う</summary>
    /// <param name="x">左辺</param>
    /// <param name="y">右辺</param>
    /// <returns>比較結果</returns>
    public int Compare(AlbumPlaylist x, AlbumPlaylist y)
    {
        return _stringComparer.Compare(x.Album.Key, y.Album.Key);
    }
}
