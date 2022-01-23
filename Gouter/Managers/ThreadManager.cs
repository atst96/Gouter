using System.Windows.Threading;

namespace Gouter.Managers;

internal static class ThreadManager
{
    public static Dispatcher DeviceDispatcher { get; } = Dispatcher.CurrentDispatcher;
}
