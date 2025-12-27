using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace CarolCustomizer.Contracts;

public interface IListable
{
    Sprite Thumbnail { get; }
    string Header { get; }
    string Subheader { get; }
    Color BaseColor { get; }
    Color HighlightColor { get; }
    IEnumerable<IListable> Children { get; }
}

public interface IFilterable<T>
{
    bool MatchesFilter(T filterEvent);
}

public interface IToggleable
{
    UnityAction<bool> OnToggle { get; }
    bool ToggleState { get; }
}

public interface IUpdateable
{
    Action OnChange { get; set; }
}

public static class ListableHelpers
{
    static int index = 0;

    public static TreeViewItemData<IListable> ToTVID(this IListable listable)
    {
        var children = listable.Children
            .Select(x => x.ToTVID())
            .ToList();
        var idk = new TreeViewItemData<IListable>(index++, listable, children);
        return idk;
    }
}