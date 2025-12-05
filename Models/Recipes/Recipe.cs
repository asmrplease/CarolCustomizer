using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using PngHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using static MissionManager;

namespace CarolCustomizer.Models.Recipes;
public record Recipe : ISourceAwaiter
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

    public Recipe(string path)
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



    public enum Status
    {
        NoError,
        Incomplete,
        SlowSource,
        InvalidJson,
        FileError,
    }
}
