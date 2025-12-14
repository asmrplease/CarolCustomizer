using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Contracts;

public interface IListable : IContextMenuActions
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

