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
    internal class ListBoxEx : ListBox
    {
        public bool SetSelectingItemToCommandParameter { get; set; }

        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register("ItemClickCommand", typeof(ICommand), typeof(ListBoxEx), new PropertyMetadata(null));

        public ICommand ItemDoubleClickCommand
        {
            get => (ICommand)this.GetValue(ItemSelectedCommandProperty);
            set => this.SetValue(ItemSelectedCommandProperty, value);
        }

        public static readonly DependencyProperty ItemSelectedCommandProperty =
            DependencyProperty.Register("ItemDoubleClickCommand", typeof(ICommand), typeof(ListViewEx), new PropertyMetadata(null));

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var listItem = (ListBoxItem)element;

            listItem.MouseDoubleClick += this.OnItemMouseDoubleClicked;

            base.PrepareContainerForItemOverride(element, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            var listITem = (ListBoxItem)element;

            listITem.MouseDoubleClick -= this.OnItemMouseDoubleClicked;

            base.ClearContainerForItemOverride(element, item);
        }

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
