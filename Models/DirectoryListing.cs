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
        this.DisplayName = this.DirectoryPath.Path.Split(Path.PathSeparator).Last();
        
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

    string IListable.Subheader => this.DirectoryPath.Path;

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => [];

    UnityAction<bool> IListable.OnToggle => null;

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        throw new NotImplementedException();
        //rename
        //overwrite
        //delete
    }
}

//how do we connect this to the files that have been loaded?
//if each object we're adding to the UI has a path element, we could match or lookup the path
//game assets could have mock paths, such as their source descriptor, asset type, scene, etc
//dividing game sources
    //hair
    //outfits
    //accessories
    //scenes
//while making the top level separation by scene might eb simple and make sense from a programming perspective
//sorting by type would make more sense for the player, especially beginners
