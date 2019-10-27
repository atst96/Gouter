using System;

namespace Gouter
{
    internal class PlayerStateEventArgs : EventArgs
    {
        public PlayState State { get; }

        public PlayerStateEventArgs(PlayState state)
        {
            this.State = state;
        }
    }
}
