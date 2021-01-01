using System;
using System.Windows;
using System.Windows.Controls;
using Gouter.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gouter.Behaviors
{
    /// <summary>
    /// スクロール操作に関するビヘイビア
    /// </summary>
    internal class ScrollBehavior : Behavior<FrameworkElement>
    {
        private ScrollViewer _scrollViewer;

        /// <summary>
        /// ビヘイビアのアタッチ時
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            var viewer = this.AssociatedObject;
            if (viewer.IsLoaded)
            {
                this.InitializeBehavior();
            }
            else
            {
                void OnLoaded(object sender, RoutedEventArgs e)
                {
                    viewer.Loaded -= OnLoaded;
                    this.InitializeBehavior();
                }

                viewer.Loaded += OnLoaded;
            }
        }

        /// <summary>
        /// ビヘイビアのデタッチ時
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            this._scrollViewer.ScrollChanged -= this.OnScrollChanged;
        }

        /// <summary>
        /// ビヘイビア初期足
        /// </summary>
        private void InitializeBehavior()
        {
            var scrollViewer = this._scrollViewer = this.AssociatedObject.FindVisualChild<ScrollViewer>()
                ?? throw new NotSupportedException();

            scrollViewer.ScrollChanged += this.OnScrollChanged;

            // 初期位置を設定する
            scrollViewer.ScrollToVerticalOffset(this.VerticalPosition);
            scrollViewer.ScrollToHorizontalOffset(this.HorizontalPosition);
        }

        /// <summary>
        /// スクロール時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0.0d)
            {
                this.SetCurrentValue(HorizontalPositionProperty, e.HorizontalOffset);
            }

            if (e.VerticalChange != 0d)
            {
                this.SetCurrentValue(VerticalPositionProperty, e.VerticalOffset);
            }
        }

        /// <summary>
        /// 水平のスクロール位置を取得または設定する。
        /// </summary>
        public double HorizontalPosition
        {
            get => (double)this.GetValue(HorizontalPositionProperty);
            set => this.SetValue(HorizontalPositionProperty, value);
        }

        /// <summary>
        /// 水平スクロール位置のプロパティ
        /// </summary>
        public static readonly DependencyProperty HorizontalPositionProperty =
            DependencyProperty.Register(nameof(HorizontalPosition), typeof(double), typeof(ScrollBehavior), new(0d, OnHorizontalPositionChanged));

        /// <summary>
        /// 水平スクロール位置変更じ
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnHorizontalPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as ScrollBehavior;
            var scrollViewer = behavior._scrollViewer;

            if (e.OldValue != e.NewValue)
            {
                scrollViewer?.ScrollToHorizontalOffset((double)e.NewValue);
            }
        }

        /// <summary>
        /// 垂直のスクロール位置を取得または設定する。
        /// </summary>
        public double VerticalPosition
        {
            get => (double)this.GetValue(VerticalPositionProperty);
            set => this.SetValue(VerticalPositionProperty, value);
        }

        /// <summary>
        /// 垂直スクロール位置のプロパティ
        /// </summary>
        public static readonly DependencyProperty VerticalPositionProperty =
            DependencyProperty.Register(nameof(VerticalPosition), typeof(double), typeof(ScrollBehavior), new(0d, OnVerticalPositionChanged));

        /// <summary>
        /// 垂直スクロール位置の変更時
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnVerticalPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as ScrollBehavior;
            var scrollViewer = behavior._scrollViewer;

            if (e.OldValue != e.NewValue)
            {
                scrollViewer?.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}
