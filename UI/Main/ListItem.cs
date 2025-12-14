using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Events;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Main;

internal partial class ListItem : MonoBehaviour
{
    static readonly string thumbnailAddress = "ListItemUI/Icon";
    static readonly string toggleAddress = "ListItemUI/Toggle";
    static readonly string headerAddress = "ListItemUI/Text/Header";
    static readonly string subheaderAddress = "ListItemUI/Text/Subheader";
    Image background;
    Image displayImage;
    Text header;
    Text subheader;
    Toggle toggle;
    IListable source;
    public ListItem parent { get; private set; }
    public int Width { get; private set; } = 240;
    readonly List<ListItem> children = [];
    ContextMenu contextMenu;
    Action<Transform> uiInit;
    bool initialized = false;
    bool expanded = false;

    public ListItem Constructor(IListable source, ContextMenu contextMenu, Action<Transform> uiInit)
    {
        Log.Debug($"ListItem {source.Header}");
        this.source = source;
        this.contextMenu = contextMenu;
        this.uiInit = uiInit;
        this.name = $"ListItem({source.GetType().Name}): {source.Header}";
        if (source is IUpdateable dynamic) { dynamic.OnChange += Refresh; }
        return this;
    }

    void OnEnable() => Initialize();

    void Initialize()
    {
        if (this.initialized) return;

        this.uiInit(this.transform);
        this.background = transform.GetChild(0).gameObject.GetComponent<Image>();
        this.displayImage = transform.Find(thumbnailAddress)?.GetComponent<Image>();
        this.header = transform.Find(headerAddress)?.GetComponentInChildren<Text>();
        this.subheader = transform.Find(subheaderAddress)?.GetComponent<Text>();
        this.toggle = transform.Find(toggleAddress)?.GetComponent<Toggle>();
        var rect = transform.GetChild(0) as RectTransform;
        rect.sizeDelta = new Vector2(this.Width, rect.sizeDelta.y);
        this.initialized = true;
        Refresh();
    }

    void Refresh() 
    {
        if (!this.initialized) return;

        this.header.text = source.Header;
        this.subheader.text = source.Subheader;
        this.background.color = source.BaseColor;

        if (source.Thumbnail is not null)
        {
            this.displayImage.sprite = source.Thumbnail;
            this.displayImage.color = new(1,1,1,1);
        }
        else
        {
            this.displayImage.color = new(0,0,0,0);
        }
        if (source is IToggleable toggle)
        {
            //Log.Debug($"ListItem({this.header.text}).Refresh() is IToggleable");
            this.toggle.onValueChanged.RemoveAllListeners();
            this.toggle.onValueChanged.AddListener(toggle.OnToggle);
            this.toggle.SetIsOnWithoutNotify(toggle.ToggleState);
            this.toggle.gameObject.SetActive(true);
            this.displayImage.gameObject.SetActive(false);
            //Log.Debug("toggle setup complete");
        }
        else
        {
            this.toggle.gameObject.SetActive(false);
            this.displayImage.gameObject.SetActive(true);
        }
    }

    void Expand(bool expanded) 
    {
        if (expanded && this.parent) { this.parent.gameObject.SetActive(expanded); }
        this.children.ForEach(x => x.gameObject.SetActive(expanded)); 
    }

    public static void Parent(ListItem parent, ListItem child)
    {
        child.parent = parent;
        child.transform.SetParent(parent.transform);
        child.Width = parent.Width - 10;
        parent.children.Add(child);
    }
}

internal partial class ListItem : IComparable<ListItem>
{
    public int CompareTo(ListItem other)
    {
        var header = this.header.text.CompareTo(other.header.text);
        if (header != 0) return header;

        return this.subheader.text.CompareTo(other.subheader.text);
    }
}

internal partial class ListItem : IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { contextMenu.Show(source); }
        if (eventData.button == PointerEventData.InputButton.Left) { Expand(expanded.Flip()); }
    }
}

internal partial class ListItem : IFilterable<UIFilterChangedEvent>
{
    bool IFilterable<UIFilterChangedEvent>.MatchesFilter(UIFilterChangedEvent e)
    {
        if (this.source is null) return false;

        if (this.source is IToggleable toggle)
        {
            var state = toggle.ToggleState;
            if (state) Log.Debug($"Toggle {this.source.Header} is {state}.");
            if (state) return true;
        }
        if (e.HasText)
        {
            if (source.Header    is not null && source.Header.Contains(e.Text, StringComparison.InvariantCultureIgnoreCase)) return true;
            if (source.Subheader is not null && source.Subheader.Contains(e.Text, StringComparison.InvariantCultureIgnoreCase)) return true;
        } 
        return false;
    }
}

//listitem visibility and relationships
//setting relationships and having predictable behavior for when a listitem will be displayed is crucial
//all listitems 