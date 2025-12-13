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
    bool Filter<T>(Predicate<T> predicate);
    IEnumerable<IListable> Children { get; }
    UnityAction<bool> OnToggle { get; }
}

public interface IUpdateable
{
    Action OnChange { get; set; }
}