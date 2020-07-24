using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Utils
{
    internal static class MessagePackUtility
    {
        private static readonly MessagePackSerializerOptions _messagPackOptions = MessagePack.Resolvers.StandardResolverAllowPrivate.Options;

        public static async ValueTask<T> DeserializeFile<T>(string path)
        {
            var stream = FileContentUtility.OpenRead(path);
            if (stream == null)
            {
                return default;
            }

            using (stream)
            {
                return await DeserializeAsync<T>(stream).ConfigureAwait(false);
            }
        }

        public static ValueTask<T> DeserializeAsync<T>(Stream stream)
        {
            return MessagePackSerializer.DeserializeAsync<T>(stream, _messagPackOptions);
        }

        public static async Task SerializeFile<T>(T @object, string path)
        {
            using (var bufferStream = new MemoryStream())
            {
                await SerializeAsync(@object, bufferStream).ConfigureAwait(false);

                using (var outputStream = FileContentUtility.OpenCreate(path))
                {
                    bufferStream.Seek(0, SeekOrigin.Begin);

                    await bufferStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }
            }
        }

        public static Task SerializeAsync<T>(T @object, Stream stream)
        {
            return MessagePackSerializer.SerializeAsync(stream, @object, _messagPackOptions);
        }
    }
}
