using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Recipes;
public partial record Recipe
{
    public readonly string Name;
    public readonly string Path;
    public readonly string Extension;
    public readonly Status Error;
    public readonly RecipeDescriptor Descriptor;
    public readonly string Json;

    public Recipe(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        Extension = System.IO.Path.GetExtension(Path);
        var results = RecipeLoader.ValidateRecipeFile(Path);
        Error = results.Status;
        Descriptor = results.Recipe;
        Json = results.Json;
    }

    public enum Status
    {
        NoError,
        MissingSource,
        SlowSource,
        InvalidJson,
        FileError
    }
}

public partial record Recipe : IListable
{
    Sprite IListable.Thumbnail => throw new NotImplementedException();

    string IListable.Header => this.Name;

    string IListable.Subheader => this.Error.ToString();

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => throw new NotImplementedException();//sources, accessories, materials

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

}

public partial record Recipe : IContextMenuActions
{
    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        throw new NotImplementedException();
    }
}