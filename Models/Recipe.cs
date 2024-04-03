using CarolCustomizer.Behaviors;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CarolCustomizer.Models;
public record Recipe
{
    public readonly string Name;
    public readonly string Path;
    public readonly Status Error;
    public readonly RecipeDescriptor Descriptor;

    public Recipe(string path)
    {
        this.Path = path;
        this.Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        var results = RecipeLoader.ValidateRecipeFile(Path);
        this.Error = results.Status;
        this.Descriptor = results.Recipe;
    }

    public enum Status
    {
        NoError,
        MissingSource,
        InvalidJson,
        FileError
    }
}
