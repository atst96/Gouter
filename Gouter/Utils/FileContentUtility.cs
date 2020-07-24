using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter.Utils
{
    internal static class FileContentUtility
    {
        public static FileStream OpenRead(string path)
        {
            var fileInfo = new FileInfo(path);

            return fileInfo.Exists && fileInfo.Length > 0
                ? fileInfo.OpenRead()
                : null;
        }

        public static FileStream OpenCreate(string path)
        {
            return File.Open(path, FileMode.Create, FileAccess.Write);
        }
    }
}
