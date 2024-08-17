using System.Collections.ObjectModel;
using Gouter.Components;
using Xunit;
using System.Collections.Specialized;
using System;

namespace Gouter.Test.Components;

public class TranslateObservableListTest
{
    private class TestClass : TranslateObservableList<int, string>
    {
        public TestClass(ObservableCollection<int> collection) : base(collection)
        {
        }

        protected override string ConvertTo(int item)
        {
            return item.ToString();
        }
    }

    [Fact]
    public void TestAdd()
    {
        var oldList = new ObservableCollection<int>();
        var list = new TestClass(oldList);

        int i = 0;

        NotifyCollectionChangedEventHandler func = (s, e) =>
        {
            Assert.Equal(1, e.NewItems.Count);
            Assert.Equal(i == 0 ? "1" : "2", e.NewItems[0]);
            ++i;
        };

        list.CollectionChanged += func;

        oldList.Add(1);
        oldList.Add(2);

        list.CollectionChanged -= func;
    }
}
