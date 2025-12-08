using UnityEngine;

namespace CarolCustomizer.Contracts;
public interface IRecipeProvider : ISourceDescriptor
{
    IRecipe GetRecipe(string name);
}

public interface IRecipe
{
    string Name { get; }
    RecipeDescriptor RecipeData { get; }
    Sprite Thumbnail { get; }
}