namespace Gouter.Players
{
    internal interface IMediaPlayerObserver : ISubscribableObject
    {
        void OnPlayStateChanged(PlayState state);
    }
}
