using CarolCustomizer.Behaviors.Recipes;

namespace CarolCustomizer.Models.Recipes;
public record Recipe
{
    public readonly string Name;
    public readonly string Path;
    public readonly Status Error;
    public readonly RecipeDescriptor21 Descriptor;

    public Recipe(string path)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(Path);
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
