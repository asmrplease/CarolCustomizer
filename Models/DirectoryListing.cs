using CarolCustomizer.Contracts;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models;

internal partial class DirectoryListing 
{
    public readonly PathDescriptor DirectoryPath;
    public readonly Sprite Thumbnail;
    public readonly string DisplayName;
    public DirectoryListing(PathDescriptor pathDescriptor)
    {
        this.DirectoryPath = pathDescriptor;
        this.Thumbnail = FindSprite();
        this.DisplayName = this.DirectoryPath.Path
            .Split("Onirism\\")
            .Last();
    }

    Sprite FindSprite()
    {
        //look for a file that meets the definition 
        //definition might be a specific filename on a compatible image format
        return null;
    }
}

internal partial class DirectoryListing : IPath
{
    PathDescriptor IPath.PathDescriptor => this.DirectoryPath;
}

internal partial class DirectoryListing : IListable
{
    Sprite IListable.Thumbnail => this.Thumbnail;

    string IListable.Header => this.DisplayName;

    string IListable.Subheader => "Folder";

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => [];

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        throw new NotImplementedException();
        //rename
        //overwrite
        //delete
    }
}
