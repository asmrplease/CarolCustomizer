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

public record Recipe : IRecipe
{
    string Name;
    RecipeDescriptor RecipeData;
    Sprite Thumbnail;
    public Recipe(string name, RecipeDescriptor recipe, Sprite thumbnail)
    {
        this.Name = name;
        this.RecipeData = recipe;
        this.Thumbnail = thumbnail;
    }

    string IRecipe.Name => this.Name;
    RecipeDescriptor IRecipe.RecipeData => this.RecipeData;
    Sprite IRecipe.Thumbnail => this.Thumbnail; 

}