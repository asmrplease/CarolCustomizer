using CarolCustomizer.Behaviors.Recipes;

namespace CarolCustomizer.Models.Recipes;
public record Recipe
{
    public readonly string Name;
    public readonly string Path;
    public readonly string Extension;
    public readonly Status Error;
    public readonly RecipeDescriptor24 Descriptor;

    public Recipe(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        Extension = System.IO.Path.GetExtension(Path);
        var results = RecipeLoader.ValidateRecipeFile(Path);
        Error = results.Status;
        Descriptor = results.Recipe;
    }

    public enum Status
    {
        NoError,
        MissingSource,
        InvalidJson,
        FileError
    }
}
