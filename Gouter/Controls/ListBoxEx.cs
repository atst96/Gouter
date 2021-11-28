using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gouter
{
    /// <summary>
    /// 拡張リストボックス
    /// </summary>
    internal class ListBoxEx : ListBox
    {
        /// <summary>ListBoxEx固有のコマンドのパラメータにアイテムにバインドされているデータを使用するかどうかを取得または設定する</summary>
        public bool SetSelectingItemToCommandParameter { get; set; }

        /// <summary>ダブルクリック時のコマンド</summary>
        public ICommand ItemDoubleClickCommand
        {
            get => (ICommand)this.GetValue(ItemDoubleClickCommandProperty);
            set => this.SetValue(ItemDoubleClickCommandProperty, value);
        }

        /// <summary>ItemDoubleClickCommandプロパティ</summary>
        public static readonly DependencyProperty ItemDoubleClickCommandProperty =
            DependencyProperty.Register("ItemDoubleClickCommand", typeof(ICommand), typeof(ListBoxEx), new PropertyMetadata(null));

        /// <summary>ListBoxItemがリストボックスのアイテムとして登録された</summary>
        /// <param name="element">ListBoxItem</param>
        /// <param name="item">データ</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var listItem = (ListBoxItem)element;

            listItem.MouseDoubleClick += this.OnItemMouseDoubleClicked;

            base.PrepareContainerForItemOverride(element, item);
        }

        /// <summary>ListBoxItemがリストボックスのアイテムが除外された</summary>
        /// <param name="element">ListBoxItem</param>
        /// <param name="item">データ</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var listITem = (ListBoxItem)element;

            listITem.MouseDoubleClick -= this.OnItemMouseDoubleClicked;

            base.ClearContainerForItemOverride(element, item);
        }

        /// <summary>アイテムのダブルクリック通知</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemMouseDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var praameter = this.SetSelectingItemToCommandParameter
                ? ((ListBoxItem)sender).DataContext
                : null;

            if (this.ItemDoubleClickCommand?.CanExecute(praameter) ?? false)
            {
                this.ItemDoubleClickCommand.Execute(praameter);
            }
        }
    }
}
