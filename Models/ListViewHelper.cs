using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DotNetResourcesExtractor;

public static class ListViewHelper
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached("SelectedItems",
                                            typeof(IList),
                                            typeof(ListViewHelper),
                                            new PropertyMetadata(null, OnSelectedItemsChanged));

    public static void SetSelectedItems(DependencyObject element, IList value)
    {
        element.SetValue(SelectedItemsProperty, value);
    }

    public static IList GetSelectedItems(DependencyObject element)
    {
        return (IList)element.GetValue(SelectedItemsProperty);
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListView listView)
        {
            listView.SelectionChanged -= ListView_SelectionChanged;
            listView.SelectionChanged += ListView_SelectionChanged;
        }
    }

    private static void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView listView)
        {
            return;
        }

        IList selectedItems = GetSelectedItems(listView);
        if (selectedItems == null)
        {
            return;
        }

        // selected
        foreach (var item in e.AddedItems)
        {
            if (!selectedItems.Contains(item))
            {
                selectedItems.Add(item);
            }
        }

        // unselected
        foreach (var item in e.RemovedItems)
        {
            if (selectedItems.Contains(item))
            {
                selectedItems.Remove(item);
            }
        }
    }

}
