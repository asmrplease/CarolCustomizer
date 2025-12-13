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
    ListItem parent;
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
        if (source.OnToggle is not null)
        {
            this.toggle.onValueChanged.AddListener(source.OnToggle);
            this.toggle.gameObject.SetActive(true);
            this.displayImage.gameObject.SetActive(false);
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

    public void OnFilterEvent(UIFilterChangedEvent predicate)
    {
        //this.gameObject.SetActive(source.Filter(predicate));
    }

    public void ParentTo(ListItem parent)
    {
        this.parent = parent;
        parent.AttachChild(this);
        this.transform.SetParent(parent.transform);
        this.Width = parent.Width - 10;
    }

    public void AttachChild(ListItem child)
    {
        if (!child) return;

        this.children.Add(child);
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