using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Gouter.Controls
{
    /// <summary>
    /// スライダーを拡張したコントロール
    /// </summary>
    internal class SliderEx : Slider
    {
        private Track _track;
        private bool _isTemplateApplied;

        private static FieldInfo _sliderTooltipField;
        private ToolTip _sliderTooltip;

        public event DragCompletedEventHandler DragCompleted;
        public event DragDeltaEventHandler DragDelta;
        public event DragStartedEventHandler DragStarted;

        /// <summary>
        /// IsDraggingのプロパティキー
        /// </summary>
        private static readonly DependencyPropertyKey ReadOnlyIsDraggingKey
            = DependencyProperty.RegisterReadOnly(nameof(IsDragging), typeof(bool), typeof(SliderEx), new PropertyMetadata(false));

        /// <summary>
        /// IsDraggingの読み取り専用プロパティ
        /// </summary>
        public static readonly DependencyProperty IsDraggingProeprty = ReadOnlyIsDraggingKey.DependencyProperty;

        /// <summary>
        /// スライダーがドラッグ中かどうかを取得する
        /// </summary>
        public bool IsDragging
        {
            get => (bool)this.GetValue(IsDraggingProeprty);
            private set => this.SetValue(ReadOnlyIsDraggingKey, value);
        }

        /// <summary>
        /// ツールチップ表示時の書式を取得または設定する
        /// </summary>
        public string AutoToolTipStringFormat { get; set; }

        /// <summary>
        /// ツールチップ表示時のコンバータを取得または設定する
        /// </summary>
        public IValueConverter AutoToolTipConverter { get; set; }

        /// <summary
        /// </summary>
        public static readonly DependencyProperty AutoTooltipConverterParameterProperty =
            DependencyProperty.Register(nameof(AutoTooltipConverterParameter), typeof(object), typeof(SliderEx), new PropertyMetadata(null));

        /// <summary>
        /// ツールチップ表示時にコンバータに渡すパラメータを取得または設定する
        /// </summary>
        public object AutoTooltipConverterParameter
        {
            get => this.GetValue(AutoTooltipConverterParameterProperty);
            set => this.SetValue(AutoTooltipConverterParameterProperty, value);
        }

        /// <summary>
        /// テンプレート適用時
        /// </summary>
        public override void OnApplyTemplate()
        {
            this.DetachTemplate();

            base.OnApplyTemplate();

            this._track = this.GetTemplatePart<Track>("PART_Track");

            this.AttachTemplate();
        }

        /// <summary>
        /// Sliderコントロールの_autoToolTipフィールドを取得する
        /// </summary>
        /// <returns></returns>
        private static FieldInfo GetSliderTooltipField()
        {
            return _sliderTooltipField ??= typeof(Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// スライダーのToolTipを取得する
        /// </summary>
        /// <returns></returns>
        private ToolTip GetSliderTooltip()
        {
            return this._sliderTooltip ??= (GetSliderTooltipField().GetValue(this) as ToolTip);
        }

        /// <summary>
        /// テンプレート適用時
        /// </summary>
        private void AttachTemplate()
        {
            var track = this._track;
            track.MouseMove += this.OnTrackMouseMove;

            this._isTemplateApplied = true;
        }

        /// <summary>
        /// テンプレート変更時
        /// </summary>
        private void DetachTemplate()
        {
            if (!this._isTemplateApplied)
            {
                return;
            }

            var track = this._track;
            track.MouseMove -= this.OnTrackMouseMove;
        }

        /// <summary>
        /// Trackコントロール上でマウスが移動した
        /// </summary>
        /// <param name="sender">イベント発行元</param>
        /// <param name="e">イベント引数</param>
        private void OnTrackMouseMove(object sender, MouseEventArgs e)
        {
            // スライダーで任意の場所で押下＆マウス移動での値変更対応
            // マウスの左ボタン押下中にThumbコントロールにイベントを伝播する
            // IsMoveToPointEnabledがtrueの場合に有効
            if (e.LeftButton == MouseButtonState.Pressed && !this._track.Thumb.IsDragging)
            {
                var args = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
                {
                    RoutedEvent = MouseLeftButtonDownEvent,
                    Source = e.Source,
                };
                this._track.Thumb.RaiseEvent(args);
            }
        }

        /// <summary>
        /// Thumb移動開始時
        /// </summary>
        /// <param name="e">イベント引数</param>
        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);

            this.IsDragging = true;
            this.DragStarted?.Invoke(this, e);

            this.UpdateSliderToolTipFormat();
        }

        /// <summary>
        /// Thumb移動時
        /// </summary>
        /// <param name="e">イベント引数</param>
        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);

            this.UpdateSliderToolTipFormat();

            this.DragDelta?.Invoke(this, e);
        }

        /// <summary>
        /// Thum移動終了時
        /// </summary>
        /// <param name="e">イベント引数</param>
        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);

            this.IsDragging = false;
            this.DragCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// SliderのToolTipの表示内容を更新する
        /// </summary>
        private void UpdateSliderToolTipFormat()
        {
            var tooltip = this.GetSliderTooltip();
            if (tooltip == null)
            {
                return;
            }

            var converterParameter = this.AutoTooltipConverterParameter;
            var value = this.AutoToolTipConverter != null
                ? this.AutoToolTipConverter.Convert(this.Value, typeof(object), converterParameter, CultureInfo.CurrentUICulture)
                : this.Value;

            var toolTipFormat = this.AutoToolTipStringFormat;
            if (!string.IsNullOrEmpty(toolTipFormat))
            {
                tooltip.Content = string.Format(toolTipFormat, value);
            }
            else if (this.AutoToolTipConverter != null)
            {
                tooltip.Content = value;
            }
        }

        /// <summary>
        /// テンプレートから指定の要素を取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public T GetTemplatePart<T>(string elementName) where T : FrameworkElement
        {
            return this.Template.FindName(elementName, this) as T ?? throw new KeyNotFoundException("Invalid elementName.");
        }

        /// <summary>
        /// スライダーのドラッグをキャンセルする
        /// </summary>
        public void CancelDrag()
        {
            this._track?.Thumb?.CancelDrag();
        }
    }
}
