using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Gouter
{
    /// <summary>
    /// BusyIndicator.xaml の相互作用ロジック
    /// </summary>
    public partial class BusyIndicator : UserControl
    {
        /// <summary>インディケータのStoryboard</summary>
        private readonly Storyboard _animator;

        /// <summary>ビジーインディケータを生成する</summary>
        public BusyIndicator()
        {
            this.InitializeComponent();

            this._animator = this.TryFindResource("animator") as Storyboard;
        }

        /// <summary>アニメーションを開始する</summary>
        private void StartAnimation()
        {
            this.canvas.BeginStoryboard(this._animator);
        }

        /// <summary>アニメーションを停止する</summary>
        private void StopAnimation()
        {
            this._animator.Stop();
        }

        /// <summary>ビジーアニメーションの表示可否を設定する</summary>
        public bool IsBusy
        {
            get => (bool)this.GetValue(IsBusyProperty);
            set => this.SetValue(IsBusyProperty, value);
        }

        /// <summary>IsBusyプロパティ</summary>
        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(BusyIndicator), new FrameworkPropertyMetadata(false, IsBusyPropertyChagned));

        /// <summary>IsBusyプロパティの変更通知</summary>
        /// <param name="d">変更された要素</param>
        /// <param name="e">イベント引数</param>
        private static void IsBusyPropertyChagned(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue && d is BusyIndicator indicator && e.NewValue is bool isAnimate)
            {
                if (isAnimate)
                {
                    indicator.StartAnimation();
                }
                else
                {
                    indicator.StopAnimation();
                }
            }
        }
    }
}
