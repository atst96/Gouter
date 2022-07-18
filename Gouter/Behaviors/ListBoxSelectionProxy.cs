using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Gouter;

/// <summary>
/// ListBox系コントロールの選択状況を依存関係プロパティに伝播するクラス
/// </summary>
internal class ListBoxSelectionProxy : Behavior<ListBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        this.AssociatedObject.SelectionChanged += this.OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        this.AssociatedObject.SelectionChanged -= this.OnSelectionChanged;

        base.OnDetaching();
    }

    /// <summary>
    /// プロパティ変更時
    /// </summary>
    /// <param name="sender">イベント発火元</param>
    /// <param name="e">リスト項目選択イベント</param>
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this.SetValue(SelectedItemsProperty, this.AssociatedObject.SelectedItems);
    }

    /// <summary>
    /// 選択中の項目リスト
    /// </summary>
    public IEnumerable SelectedItems
    {
        get => this.GetValue(SelectedItemsProperty) as IEnumerable;
        set => this.SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// <seealso cref="SelectedItems"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IEnumerable), typeof(ListBoxSelectionProxy), new PropertyMetadata(null));
}
