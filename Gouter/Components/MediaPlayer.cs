using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    internal class MediaPlayer : IDisposable
    {
        public LibraryManager Library { get; } = new LibraryManager();

        public MediaPlayer()
        {
        }

        public void Initialize(string libraryFilePath)
        {
            this.Library.Initialize(libraryFilePath);
        }

        public void Close()
        {
            this.Library?.Dispose();
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
