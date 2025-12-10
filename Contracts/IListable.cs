using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CarolCustomizer.Contracts;

public interface IListable : IContextMenuActions
{
    Sprite Thumbnail { get; }
    string Header { get; }
    string Subheader { get; }
    //List<Sprite> Badges { get; }
    Color BaseColor { get; }
    Color HighlightColor { get; }
    //UnityAction OnClick();
    bool Filter<T>(Predicate<T> predicate);
    IEnumerable<IListable> Children { get; }
    UnityAction<bool> OnToggle { get; }

}
