using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gouter
{
    [MessagePackObject]
    internal class ApplicationSetting : NotificationObject
    {
        [Key("directory.musics")]
        private List<string> _musicDirectories;

        [IgnoreMember]
        public List<string> MusicDirectories
        {
            get => this._musicDirectories ?? (this._musicDirectories = new List<string>());
        }
    }
}
