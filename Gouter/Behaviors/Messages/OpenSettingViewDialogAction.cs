using System.Windows;
using Gouter.Messaging;
using Gouter.Utils;
using Livet.Behaviors.Messaging;
using Livet.Messaging;

namespace Gouter.Behaviors.Messages;

/// <summary>
/// <see cref="OpenSettingDialogMessage"/>を実行するアクション
/// </summary>
internal sealed class OpenSettingViewDialogAction : InteractionMessageAction<FrameworkElement>
{
    /// <summary>
    /// Actionを実行する
    /// </summary>
    /// <param name="message"></param>
    protected override void InvokeAction(InteractionMessage message)
    {
        if (message is not OpenSettingDialogMessage msg)
        {
            return;
        }

        DialogUtils.OpenSettingWindow(Window.GetWindow(this.AssociatedObject));
    }
}
