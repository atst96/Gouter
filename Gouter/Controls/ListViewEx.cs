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
    internal class ListViewEx : ListView
    {
        public bool SetSelectingItemToCommandParameter { get; set; }

        public ICommand ItemDoubleClickCommand
        {
            get => (ICommand)this.GetValue(ItemSelectedCommandProperty);
            set => this.SetValue(ItemSelectedCommandProperty, value);
        }

        public static readonly DependencyProperty ItemSelectedCommandProperty =
            DependencyProperty.Register("ItemDoubleClickCommand", typeof(ICommand), typeof(ListViewEx), new PropertyMetadata(null));

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var listItem = (ListViewItem)element;

            listItem.MouseDoubleClick += this.OnItemMouseDoubleClicked;

            base.PrepareContainerForItemOverride(element, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var listITem = (ListViewItem)element;

            listITem.MouseDoubleClick -= this.OnItemMouseDoubleClicked;

            base.ClearContainerForItemOverride(element, item);
        }

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
