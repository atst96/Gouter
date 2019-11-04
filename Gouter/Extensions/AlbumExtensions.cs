using System;
using System.Collections.Generic;
using System.Text;
using ATL;

namespace Gouter
{
    /// <summary>
    /// アルバムに関する拡張メソッド
    /// </summary>
    internal static class AlbumExtensions
    {
        public const string AdditionalFields_Compilation = "cpil";

        /// <summary>トラック情報からアルバムキーを生成する</summary>
        /// <param name="track">トラック情報</param>
        /// <returns>アルバムキー</returns>
        public static string GenerateAlbumKey(this Track track)
        {
            string albumName = track.Album;
            string albumArtist = track.GetAlbumArtist("unknown", "###compilation###");

            return $"--#name={{{albumName}}};\n--#artist={{{albumArtist}}};";
        }

        /// <summary>コンピレーションアルバムかどうかを取得する</summary>
        /// <param name="track">トラック情報</param>
        /// <returns>コンピレーションアルバムか否か</returns>
        public static bool GetIsCompiatilnAlbum(this Track track)
        {
            return track.AdditionalFields.TryGetValue(AdditionalFields_Compilation, out var isCompilation)
                && isCompilation == "1";
        }

        /// <summary>トラック情報からアルバムアーティストを取得する</summary>
        /// <param name="track">トラック情報</param>
        /// <param name="unknownValue">不明の戻り値</param>
        /// <param name="compilationValue">コンピレーションアルバム時の戻り値</param>
        /// <returns></returns>
        public static string GetAlbumArtist(this Track track, string unknownValue = "Unknown", string compilationValue = "Various Artists")
        {
            if (track.GetIsCompiatilnAlbum())
            {
                // コンピレーションアルバムの場合
                return compilationValue;
            }

            if (!string.IsNullOrEmpty(track.AlbumArtist))
            {
                // アルバムアーティストが登録されている場合
                return track.AlbumArtist;
            }

            if (!string.IsNullOrEmpty(track.Artist))
            {
                // アーティスト情報が登録されている場合
                return track.Artist;
            }

            // アルバムアーティスト名不明
            return unknownValue;
        }
    }
}
