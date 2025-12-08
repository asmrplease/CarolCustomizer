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
    static readonly string thumbnailAddress = "Outfit Header/Icon";
    static readonly string headerAddress = "Outfit Header/Text/Outfit Name";
    static readonly string subheaderAddress = "Outfit Header/Text/Pickup Location";

    Image background;
    Image displayImage;
    Text header;
    Text subheader;

    IListable source;

    GameObject parent;
    readonly List<GameObject> children = [];

    ContextMenu contextMenu;

    void Awake()
    {
        Log.Debug("ListItem.Awake()");
        this.background = transform.GetChild(0).gameObject.GetComponent<Image>();
        this.displayImage = transform.Find(thumbnailAddress)?.GetComponent<Image>();
        this.header = transform.Find(headerAddress)?.GetComponentInChildren<Text>();
        this.subheader = transform.Find(subheaderAddress)?.GetComponent<Text>();
    }


    public ListItem Constructor(IListable source, ContextMenu contextMenu)
    {
        Log.Debug($"ListItem {source.Header}");
        this.source = source;
        this.name = $"ListItem({source.GetType().Name}): {source.Header}";
        this.header.text = source.Header;
        this.subheader.text = source.Subheader;
        this.displayImage.sprite = source.Thumbnail;
        this.background.color = source.BaseColor;
        this.contextMenu = contextMenu;
        
        return this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) { contextMenu.Show(source); }
        //if (eventData.button == PointerEventData.InputButton.Left) { source.OnClick()?.Invoke(); }
    }

    void Expand(bool expanded) 
    {
        if (expanded) { this.parent.SetActive(expanded); }
        this.children.ForEach(x => x.SetActive(expanded)); 
    }

    public void OnFilterEvent(UIFilterChangedEvent predicate)
    {
        //this.gameObject.SetActive(source.Filter(predicate));
    }

    public void ParentTo(GameObject parent)
    {
        this.parent = parent;        
    }

    public void AttachChild(GameObject child)
    {
        if (!child) return;

        this.children.Add(child);
    }
}
