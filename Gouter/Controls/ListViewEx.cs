using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gouter
{
    /// <summary>
    /// 拡張ListView
    /// </summary>
    internal class ListViewEx : ListView
    {
        /// <summary>ListViewEx固有のコマンドのパラメータにアイテムにバインドされているデータを使用するかどうかを取得または設定する</summary>
        public bool SetSelectingItemToCommandParameter { get; set; }

        /// <summary>ダブルクリック時のコマンド</summary>
        public ICommand ItemDoubleClickCommand
        {
            get => (ICommand)this.GetValue(ItemDoubleClickCommandProperty);
            set => this.SetValue(ItemDoubleClickCommandProperty, value);
        }

        /// <summary>ItemDoubleClickCommandプロパティ</summary>
        public static readonly DependencyProperty ItemDoubleClickCommandProperty = DependencyProperty
            .Register("ItemDoubleClickCommand", typeof(ICommand), typeof(ListViewEx), new PropertyMetadata(null));

        /// <summary>ListViewItemがリストボックスのアイテムとして登録された</summary>
        /// <param name="element">ListViewItem</param>
        /// <param name="item">データ</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var listItem = (ListViewItem)element;

            listItem.MouseDoubleClick += this.OnItemMouseDoubleClicked;

            base.PrepareContainerForItemOverride(element, item);
        }

        /// <summary>ListViewItemがリストボックスのアイテムが除外された</summary>
        /// <param name="element">ListViewItem</param>
        /// <param name="item">データ</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var listITem = (ListViewItem)element;

            listITem.MouseDoubleClick -= this.OnItemMouseDoubleClicked;

            base.ClearContainerForItemOverride(element, item);
        }

        /// <summary>アイテムのダブルクリック通知</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemMouseDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var parameter = this.SetSelectingItemToCommandParameter
                ? ((ListViewItem)sender).DataContext
                : null;

            if (this.ItemDoubleClickCommand?.CanExecute(parameter) ?? false)
            {
                this.ItemDoubleClickCommand.Execute(parameter);
            }
        }
    }
}
