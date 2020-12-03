using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gouter.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gouter.Behaviors
{
    internal class HorizontalScrollBehavior : Behavior<ListBox>
    {
        private ScrollViewer _scrollViewer;

        private void FindTemplateChild()
        {
            var element = this.AssociatedObject;
            this._scrollViewer = element.FindVisualChild<ScrollViewer>();
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            var element = this.AssociatedObject;
            element.Loaded += this.OnLoaded;
            element.PreviewMouseWheel += this.OnPreviewMouseWheel;

            this.FindTemplateChild();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        /// <summary>
        /// マウスホイール操作時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewr = this._scrollViewer;
            if (scrollViewr == null)
            {
                return;
            }

            if (e.Delta < 0)
            {
                scrollViewr.LineRight();
            }
            else if (e.Delta > 0)
            {
                scrollViewr.LineLeft();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.FindTemplateChild();
        }
    }
}
