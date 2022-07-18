using System.Windows;
using Livet.Behaviors.Messaging;
using Livet.Messaging;
using Gouter.Messaging;
using Gouter.ViewModels;
using Gouter.Views;

namespace Gouter.Behaviors;

/// <summary>
/// プロンプトダイアログのMessageAction
/// </summary>
internal class PromptAction : InteractionMessageAction<FrameworkElement>
{
    /// <summary>
    /// 実行
    /// </summary>
    /// <param name="message">メッセージ</param>
    protected override void InvokeAction(InteractionMessage message)
    {
        if (message is not PromptMessage prompt)
        {
            return;
        }

        var viewModel = new PromptViewModel
        {
            Title = prompt.Title,
            Description = prompt.Description,
            Text = prompt.Text,
            IsAllowEmpty = prompt.IsAllowEmpty,
            Validation = prompt.Validation,
        };

        prompt.Response = null;

        var window = new PromptWindow
        {
            DataContext = viewModel,
            Owner = Window.GetWindow(this.AssociatedObject),
        };

        if (window.ShowDialog() ?? false)
        {
            prompt.Response = viewModel.Text;
        }
    }
}
