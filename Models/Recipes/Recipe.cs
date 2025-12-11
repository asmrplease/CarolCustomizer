using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using PngHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Recipes;
public partial record RecipeFile
{
    public readonly string Name;
    public readonly string Path;
    public readonly string Extension;
    public Status Error;
    public readonly RecipeDescriptor Descriptor;
    public readonly string Json;
    public readonly List<AccessoryDescriptor> MissingAccessories;
    public readonly HashSet<SourceDescriptor> MissingSources;
    public readonly RichPng Png;
    public event Action OnStatusChanged;

    public RecipeFile(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        Extension = System.IO.Path.GetExtension(Path);
        var results = RecipeLoader.ValidateRecipeFile(Path);
        Error = results.Status;
        Descriptor = results.Recipe;
        Json = results.Json;
        MissingSources = results.MissingSources.ToHashSet();
        MissingAccessories = results.MissingAccs.ToList();
        Png = results.Png;
        results.MissingSources
            .ToList()
            .ForEach(x => SourceAwaiter.Register(x, this));
    }

    [Flags]
    public enum Status
    {
        NoError = 0,
        Incomplete = 1,
        SlowSource = 2,
        InvalidJson = 4,
        FileError = 8,
    }
}

public partial record RecipeFile : ISourceAwaiter
{
    void ISourceAwaiter.HandleSourceLoaded(SourceDescriptor source)
    {
        MissingSources.Remove(source);
        if (MissingSources.Any()) return;

        bool slow = SceneResourceProvider
            .CheckMaterialsReady(RecipeApplier.GetWorldMats(this.Descriptor))
            .Any();
        if (this.Error == Status.Incomplete) this.Error = Status.NoError;
        if (this.Error == Status.NoError && slow) this.Error = Status.SlowSource;

        OnStatusChanged?.Invoke();
    }

}

public partial record RecipeFile : IListable
{
    Sprite IListable.Thumbnail => throw new NotImplementedException();

    string IListable.Header => this.Name;

    string IListable.Subheader => this.Error.ToString();

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => throw new NotImplementedException();//sources, accessories, materials

    UnityAction<bool> IListable.OnToggle => null;

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }
}

public partial record RecipeFile : IRecipe
{
    string IRecipe.Name => this.Name;
    RecipeDescriptor IRecipe.RecipeData => this.Descriptor;
    Sprite IRecipe.Thumbnail => throw new NotImplementedException();//convert png to sprite here?
}

public partial record RecipeFile : IContextMenuActions
{
    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        throw new NotImplementedException();
    }
}

public partial record RecipeFile : IPath
{
    PathDescriptor IPath.PathDescriptor => new(this.Path, PathType.Filesystem);
}