using System.Windows;
using Livet.Messaging;

namespace Gouter.Messaging;

/// <summary>
/// 設定ダイアログを表示する相互作用メッセージ
/// </summary>
internal class OpenSettingDialogMessage : InteractionMessage
{
    /// <summary>
    /// 表示するページのインデックス
    /// </summary>
    public int? PageIndex
    {
        get => (int)this.GetValue(PageIndexProperty);
        set => this.SetValue(PageIndexProperty, value);
    }

    /// <summary>
    /// <see cref="PageIndex"/>のプロパティ
    /// </summary>
    public static readonly DependencyProperty PageIndexProperty =
        DependencyProperty.Register(nameof(PageIndex), typeof(int?), typeof(OpenSettingDialogMessage), new PropertyMetadata(null));
}
