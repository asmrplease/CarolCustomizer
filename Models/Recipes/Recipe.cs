using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Outfits;
using PngHelper;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Models.Recipes;
public record Recipe
{
    public readonly string Name;
    public readonly string Path;
    public readonly string Extension;
    public readonly Status Error;
    public readonly RecipeDescriptor Descriptor;
    public readonly string Json;
    public readonly List<AccessoryDescriptor> MissingAccessories;
    public readonly List<SourceDescriptor> MissingSources;
    public readonly RichPng Png;

    public Recipe(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        Extension = System.IO.Path.GetExtension(Path);
        var results = RecipeLoader.ValidateRecipeFile(Path);
        Error = results.Status;
        Descriptor = results.Recipe;
        Json = results.Json;
        MissingSources = results.MissingSources.ToList();
        MissingAccessories = results.MissingAccs.ToList();
        Png = results.Png;
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
