using MessagePack;
using System.IO;
using System.Threading.Tasks;

namespace Gouter.Utils
{
    /// <summary>
    /// MessagePack関連のユーティリティクラス
    /// </summary>
    internal static class MessagePackUtil
    {
        /// <summary>
        /// 既定のシリアライズ／デシリアライズオプション
        /// </summary>
        private static readonly MessagePackSerializerOptions _messagPackOptions = MessagePack.Resolvers.StandardResolverAllowPrivate.Options;

        /// <summary>
        /// ファイルからデシリアライズする。
        /// </summary>
        /// <typeparam name="T">デシリアライズする型</typeparam>
        /// <param name="path">ファイルパス</param>
        /// <returns>デシリアライズ結果</returns>
        public static async ValueTask<T> DeserializeFile<T>(string path)
        {
            var stream = FileUtil.OpenRead(path);
            if (stream == null)
            {
                return default;
            }

            using (stream)
            {
                return await DeserializeAsync<T>(stream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ストリームからデシリアライズする。
        /// </summary>
        /// <typeparam name="T">デシリアライズする型</typeparam>
        /// <param name="stream">ストリーム</param>
        /// <returns>デシリアライズ結果</returns>
        public static ValueTask<T> DeserializeAsync<T>(Stream stream)
            => MessagePackSerializer.DeserializeAsync<T>(stream, _messagPackOptions);

        /// <summary>
        /// オブジェクトをシリアライズし、結果をファイルに書き込む。
        /// </summary>
        /// <typeparam name="T">オブジェクトの型</typeparam>
        /// <param name="object">オブジェクト</param>
        /// <param name="path">書き出し先のファイルパス</param>
        public static async Task SerializeFile<T>(T @object, string path)
        {
            using (var bufferStream = new MemoryStream())
            {
                await SerializeAsync(@object, bufferStream).ConfigureAwait(false);

                using (var outputStream = FileUtil.OpenCreate(path))
                {
                    bufferStream.Seek(0, SeekOrigin.Begin);

                    await bufferStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// オブジェクトをシリアライズし、結果をストリームに書き込む。
        /// </summary>
        /// <typeparam name="T">オブジェクトの型</typeparam>
        /// <param name="object">オブジェクト</param>
        /// <param name="stream">書き出し先ストリーム</param>
        public static Task SerializeAsync<T>(T @object, Stream stream)
            => MessagePackSerializer.SerializeAsync(stream, @object, _messagPackOptions);
    }
}
