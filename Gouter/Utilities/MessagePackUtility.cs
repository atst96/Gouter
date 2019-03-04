using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Utilities
{
    internal static class MessagePackUtility
    {
        private static readonly IFormatterResolver _messagPackResolver = MessagePack.Resolvers.StandardResolverAllowPrivate.Instance;

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

        public static Task<T> DeserializeAsync<T>(Stream stream)
        {
            return MessagePackSerializer.DeserializeAsync<T>(stream, _messagPackResolver);
        }

        public static async Task SerializeFile<T>(T @object, string path)
        {
            using (var bufferStream = new MemoryStream())
            {
                await SerializeAsync(@object, bufferStream).ConfigureAwait(false);

                using (var outputStream = FileContentUtility.OpenCreate(path))
                {
                    bufferStream.Position = 0;

                    await bufferStream.CopyToAsync(outputStream).ConfigureAwait(false);
                }
            }
        }

        public static Task SerializeAsync<T>(T @object, Stream stream)
        {
            return MessagePackSerializer.SerializeAsync(stream, @object, _messagPackResolver);
        }
    }
}
