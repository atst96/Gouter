using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gouter
{
    /// <summary>
    /// ジオメトリアイコン
    /// </summary>
    internal class GeometryIcon : Control
    {
        /// <summary>アイコンの表示色を取得または設定する</summary>
        public Brush Fill
        {
            get => (Brush)this.GetValue(GeometryIcon.FillProperty);
            set => this.SetValue(GeometryIcon.FillProperty, value);
        }

        /// <summary>Fillプロパティ</summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty
            .Register("Fill", typeof(Brush), typeof(GeometryIcon), new PropertyMetadata(null));

        /// <summary>表示データを取得または設定する</summary>
        public Geometry Data
        {
            get => (Geometry)this.GetValue(DataProperty);
            set => this.SetValue(DataProperty, value);
        }

        /// <summary>Dataプロパティ</summary>
        public static readonly DependencyProperty DataProperty = DependencyProperty
            .Register("Data", typeof(Geometry), typeof(GeometryIcon), new PropertyMetadata(null));
    }
}
