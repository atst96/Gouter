using System.Windows;
using System.Windows.Controls;
using Gouter.Extensions;
using Microsoft.Xaml.Behaviors;

namespace Gouter.Behaviors;

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
            // 初期化済みであればビヘイビアの初期化処理を行う
            this.InitializeBehavior(true);
        }
        else
        {
            viewer.Loaded += this.OnLoaded;
        }
    }

    /// <summary>
    /// オブジェクト初期化時(ビヘイビア初期化未実施時)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var viewer = this.AssociatedObject;
        viewer.Loaded -= this.OnLoaded;

        this.InitializeBehavior(true);
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
    /// 対象要素初期化時
    /// (ビヘイビア初期化が行われていない場合のみ実行)
    /// <param name="retry">初期化処理のリトライ実施フラグ</param>
    /// </summary>
    private void InitializeBehavior(bool retry)
    {
        var scrollViewer = this._scrollViewer = this.AssociatedObject.FindVisualChild<ScrollViewer>();
        if (scrollViewer == null)
        {
            if (retry)
            {
                // テンプレート未適用により要素が取得できない場合はフォーカスイベントで初期化処理を行う
                this.AssociatedObject.GotFocus += this.OnLoadFocus;
            }

            return;
        }

        scrollViewer.ScrollChanged += this.OnScrollChanged;

        // 初期位置を設定する
        scrollViewer.ScrollToVerticalOffset(this.VerticalPosition);
        scrollViewer.ScrollToHorizontalOffset(this.HorizontalPosition);
    }

    /// <summary>
    /// 要素フォーカス時
    /// (ビヘイビア初期化が行われていない場合のみ実行)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLoadFocus(object sender, RoutedEventArgs e)
    {
        var viewer = this.AssociatedObject;

        // フォーカスイベントの購読を解除
        viewer.GotFocus -= this.OnLoadFocus;

        // 初期化処理を行う
        // ここで要素が取得できなければ、以降 初期化処理のリトライは行わない
        this.InitializeBehavior(retry: false);
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
