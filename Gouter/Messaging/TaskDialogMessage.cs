using System.Windows;
using System.Windows.Forms;
using Livet.Messaging;

namespace Gouter.Messaging;

internal class TaskDialogMessage : InteractionMessage
{
    /// <summary>
    /// タスクダイアログ情報
    /// </summary>
    public TaskDialogPage TaskDialogPage { get; }

    /// <summary>
    /// <see cref="TaskDialogMessage"/>を生成する
    /// </summary>
    /// <param name="taskDialogPage">オプション</param>
    public TaskDialogMessage(TaskDialogPage taskDialogPage) : base()
    {
        this.TaskDialogPage = taskDialogPage;
    }

    /// <summary>
    /// <see cref="TaskDialogMessage"/>を生成する
    /// </summary>
    /// <param name="messageKey">メッセージキー</param>
    /// <param name="taskDialogPage">オプション</param>
    public TaskDialogMessage(string messageKey, TaskDialogPage taskDialogPage) : base()
    {
        this.TaskDialogPage = taskDialogPage;
        this.MessageKey = messageKey;
    }

    protected override Freezable CreateInstanceCore()
        => new TaskDialogMessage(this.MessageKey, this.TaskDialogPage);
}
