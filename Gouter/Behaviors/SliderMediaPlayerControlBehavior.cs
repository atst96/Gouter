using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Gouter.Players;
using Microsoft.Xaml.Behaviors;

namespace Gouter.Behaviors
{
    /// <summary>
    /// スライダーからMediaPlayerを制御するためのビヘイビア
    /// </summary>
    internal class SliderMediaPlayerControlBehavior : Behavior<Slider>
    {
        public PlaylistPlayer Player
        {
            get => (PlaylistPlayer)this.GetValue(PlayerProperty);
            set => this.SetValue(PlayerProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty
            = DependencyProperty.Register(nameof(Player), typeof(PlaylistPlayer), typeof(SliderMediaPlayerControlBehavior), new PropertyMetadata(null));

        public double Position
        {
            get => (double)this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty PositionProperty
            = DependencyProperty.Register(nameof(Position), typeof(double), typeof(SliderMediaPlayerControlBehavior), new PropertyMetadata(0.0d));



        public bool IsSeeking
        {
            get => (bool)this.GetValue(IsSeekingProperty);
            set => this.SetValue(IsSeekingProperty, value);
        }

        public static readonly DependencyProperty IsSeekingProperty
            = DependencyProperty.Register(nameof(IsSeeking), typeof(bool), typeof(SliderMediaPlayerControlBehavior), new PropertyMetadata(false));


        protected override void OnAttached()
        {
            // this.AssociatedObject.drag
            base.OnAttached();

            var slider = this.AssociatedObject;
            slider.AddHandler(Thumb.DragStartedEvent, (DragStartedEventHandler)this.OnDragStart);
            slider.AddHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)this.OnDragComplete);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            var slider = this.AssociatedObject;
            slider.RemoveHandler(Thumb.DragStartedEvent, (DragStartedEventHandler)this.OnDragStart);
            slider.RemoveHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)this.OnDragComplete);
        }

        private int _tempCount = 0;
        private PlayState _tempPlayState;

        private void OnDragStart(object sender, DragStartedEventArgs e)
        {
            if (this._tempCount != 0)
            {
                return;
            }

            ++this._tempCount;

            var player = this.Player;
            if (player != null && player.Track != null)
            {
                this.IsSeeking = true;
                this._tempPlayState = player.State;

                if (player.State == PlayState.Play)
                {
                    player.Pause();
                }
            }
        }

        private void OnDragComplete(object sender, DragCompletedEventArgs e)
        {
            var slider = this.AssociatedObject;

            var player = this.Player;
            if (player != null && player.Track != null)
            {
                double value = slider.Value;
                player.Seek(TimeSpan.FromMilliseconds(value));

                if (this._tempPlayState == PlayState.Play)
                {
                    player.Play();
                }

                this.Position = value;
            }

            this._tempCount = 0;
            this.IsSeeking = false;
        }
    }
}
