using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Gouter.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gouter.Behaviors
{
    internal class ListViewFillColumnBehavior : Behavior<ListView>
    {
        private ScrollViewer _scrollContainer;
        private IList<GridViewColumnHeader> _freeWidthHeaders;
        private GridViewColumnHeader _fillWidthHeader;

        public int MinWidth { get; set; } = 60;

        public int FillColumnIndex { get; set; } = -1;

        protected override void OnAttached()
        {
            base.OnAttached();

            var container = this.AssociatedObject;

            if (container.IsLoaded)
            {
                this.InitializeBehavior();
            }
            else
            {
                container.Loaded += this.OnContaienrLoaded;
            }
        }

        private void OnContaienrLoaded(object sender, RoutedEventArgs e)
        {
            this.InitializeBehavior();
        }

        private void InitializeBehavior()
        {
            var listView = this.AssociatedObject;

            if (listView?.IsLoaded != true)
            {
                return;
            }

            int fillColumnIndex = this.FillColumnIndex;

            this._scrollContainer = this.AssociatedObject.FindVisualChild<ScrollViewer>();
            this._scrollContainer.LayoutUpdated += this.OnLayoutUpdated;

            var headerPresenter = listView.FindVisualChild<GridViewHeaderRowPresenter>();

            var headers = headerPresenter
                .EnumerateChildren<GridViewColumnHeader>()
                .Where(h => h.Column != null)
                .Reverse()
                .ToList();

            if (headers.Count <= fillColumnIndex)
            {
                return;
            }

            this._fillWidthHeader = fillColumnIndex == -1
                ? headers.Last()
                : headers[fillColumnIndex];

            this._freeWidthHeaders = headers;
            this._freeWidthHeaders.Remove(this._fillWidthHeader);

            foreach (var header in this._freeWidthHeaders)
            {
                header.LayoutUpdated += this.OnLayoutUpdated;
            }

            this.AlignColumnSizes();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this._scrollContainer.LayoutUpdated -= this.OnLayoutUpdated;

            if (this._fillWidthHeader == null)
            {
                return;
            }

            foreach (var header in this._freeWidthHeaders)
            {
                header.LayoutUpdated -= this.OnLayoutUpdated;
            }

            this._freeWidthHeaders = null;
            this._fillWidthHeader = null;
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.AlignColumnSizes();
        }

        private void AlignColumnSizes()
        {
            if (this._fillWidthHeader == null)
            {
                return;
            }

            double otherColumnWidth = this._freeWidthHeaders.Sum(h => h.ActualWidth);
            double columnWidth = Math.Max(this.MinWidth, this._scrollContainer.ViewportWidth - otherColumnWidth);

            if (this._fillWidthHeader.Column.Width != columnWidth)
            {
                this._fillWidthHeader.Column.Width = columnWidth;
            }
        }
    }
}
