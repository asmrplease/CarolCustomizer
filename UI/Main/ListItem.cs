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

internal class ListItem : MonoBehaviour, IPointerClickHandler
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

        return this;
    }

    void OnEnable() => Setup();

    void Setup()
    {
        if (this.initialized) return;

        this.uiInit(this.transform);
        this.background = transform.GetChild(0).gameObject.GetComponent<Image>();
        this.displayImage = transform.Find(thumbnailAddress)?.GetComponent<Image>();
        this.header = transform.Find(headerAddress)?.GetComponentInChildren<Text>();
        this.subheader = transform.Find(subheaderAddress)?.GetComponent<Text>();
        this.toggle = transform.Find(toggleAddress)?.GetComponent<Toggle>();

        this.header.text = source.Header;
        this.subheader.text = source.Subheader;
        this.displayImage.sprite = source.Thumbnail;
        this.displayImage.color = new(1, 1, 1, 1);
        this.background.color = source.BaseColor;

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
        this.initialized = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { contextMenu.Show(source); }
        if (eventData.button == PointerEventData.InputButton.Left) { Expand(expanded.Flip()); }
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
        this.transform.parent = parent.transform;
    }

    public void AttachChild(ListItem child)
    {
        if (!child) return;

        this.children.Add(child);
    }
}
