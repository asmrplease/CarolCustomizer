using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Contracts;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Recipes;
public partial record Recipe
{
    public readonly string Name;
    public readonly RecipeDescriptor Descriptor;
    public readonly Sprite Thumbnail;
    public Recipe(string name, RecipeDescriptor recipe, Sprite thumbnail)
    {
        this.Name = name;
        this.Descriptor = recipe;
        this.Thumbnail = thumbnail;
    }
}

public partial record Recipe : IRecipe
{
    string IRecipe.Name => this.Name;
    RecipeDescriptor IRecipe.RecipeData => this.Descriptor;
    Sprite IRecipe.Thumbnail => this.Thumbnail;

}

public partial record Recipe : IListable
{
    Sprite IListable.Thumbnail => this.Thumbnail;

    string IListable.Header => this.Name;

    string IListable.Subheader => "";

    Color IListable.BaseColor => Constants.DefaultColor;

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => [];//sources, accessories, materials?

    UnityAction<bool> IListable.OnToggle => null;

    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        var output = new List<(string, UnityAction)>();
        PlayerInstances.ValidPlayers
                .ForEach(player =>
                    output.Add(
                        ($"Load on P{player.playerIndex + 1}"
                        , () => RecipeApplier.ActivateRecipe(player.outfitManager, this.Descriptor))));
        if (PlayerInstances.ValidPlayers.Count() == 0)
        {
            output.Add(("Load", () => RecipeApplier.ActivateRecipe(PlayerInstances.DefaultPlayer.outfitManager, this.Descriptor)));
        }
        //if (this.Name.Contains(Constants.AutoSave, StringComparison.CurrentCultureIgnoreCase))
        return output;
    }
}