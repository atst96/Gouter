using System.Windows;
using System.Windows.Data;
using Microsoft.Xaml.Behaviors;

namespace Gouter;

internal class DependencyPropertyProxy : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        var target = this.AssociatedObject;
        target.SetBinding(ValueProperty, new Binding
        {
            Source = target,
            Path = new PropertyPath(this.Property),
            Mode = BindingMode.OneWay,
        });
    }

    protected override void OnDetaching()
    {
        var target = this.AssociatedObject;
        BindingOperations.ClearBinding(target, ValueProperty);

        base.OnDetaching();
    }

    /// <summary>
    /// プロパティ
    /// </summary>
    public DependencyProperty Property
    {
        get => this.GetValue(PropertyProperty) as DependencyProperty;
        set => this.SetValue(PropertyProperty, value);
    }

    /// <summary>
    /// <see cref="Property"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty PropertyProperty
        = DependencyProperty.Register(nameof(Property), typeof(DependencyProperty), typeof(DependencyPropertyProxy), new PropertyMetadata(null));

    /// <summary>
    /// 値
    /// </summary>
    public object Value
    {
        get => this.GetValue(ValueProperty);
        set => this.SetValue(ValueProperty, value);
    }

    /// <summary>
    /// <see cref="Value"/>の依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(nameof(Value), typeof(object), typeof(DependencyPropertyProxy), new PropertyMetadata(null));
}
