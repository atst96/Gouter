using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Gouter.Extensions;

namespace Gouter.Selectors;

internal class TrackGroupContainerStyleSelector : StyleSelector
{
    /// <summary>
    /// ヘッダ無しのスタイル
    /// </summary>
    private readonly Style _noHeaderGroupContainerStyle;

    /// <summary>
    /// ヘッダありのスタイル
    /// </summary>
    private readonly Style _headeredGroupContainerStyle;

    public TrackGroupContainerStyleSelector() : base()
    {
        var app = App.Instance;
        this._headeredGroupContainerStyle = app.TryFindResource("HeaderedGroupItemStyle") as Style;
        this._noHeaderGroupContainerStyle = app.TryFindResource("NoHeaderGroupItemStyle") as Style;
    }

    public override Style SelectStyle(object item, DependencyObject container)
    {
        if (container is GroupItem groupItem)
        {
            var itemsControl = groupItem.FindAncestor<ItemsControl>();
            if (itemsControl is ListBox)
            {
                if (itemsControl.ItemsSource is CollectionView collectionView
                    && collectionView.Groups.Count <= 1)
                {
                    return this._noHeaderGroupContainerStyle;
                }
            }
        }

        return this._headeredGroupContainerStyle;
    }
}
