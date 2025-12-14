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
    public List<Recipe> recipes = [];

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
        if (this.Png is not null) this.recipes = PngUtil.ReadRecipes(this.Png);
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

    Color GetUIColor()
    {
        return this.Error switch
        {
            RecipeFile.Status.Incomplete => Constants.DefaultColor,
            RecipeFile.Status.SlowSource => Constants.DefaultColor.RGBMultiplied(0.5f),
            RecipeFile.Status.InvalidJson => Color.gray,
            RecipeFile.Status.FileError => Color.gray,
            _ => Constants.DefaultColor,
        };
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
    Sprite IListable.Thumbnail => ((IRecipe)this.recipes.First()).Thumbnail;

    string IListable.Header => this.Name;

    string IListable.Subheader => this.Error.ToString();

    Color IListable.BaseColor => this.GetUIColor();

    Color IListable.HighlightColor => Constants.Highlight;

    IEnumerable<IListable> IListable.Children => this.recipes;//sources, accessories, materials
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
        var output = new List<(string, UnityAction)>();
        if (this.Error == RecipeFile.Status.NoError)
        {
            PlayerInstances.ValidPlayers
                .ForEach(player =>
                    output.Add(
                        ($"Load on P{player.playerIndex + 1}"
                        , () => RecipeApplier.ActivateRecipe(player.outfitManager, this.Descriptor))));
            if (PlayerInstances.ValidPlayers.Count() == 0)
            {
                output.Add(("Load", () => RecipeApplier.ActivateRecipe(PlayerInstances.DefaultPlayer.outfitManager, this.Descriptor)));
            }
            if (this.Name.Contains(Constants.AutoSave, StringComparison.CurrentCultureIgnoreCase))
                return output;
        }
        //if (this.Error != RecipeFile.Status.FileError)
        //{
        //    output.Add(("Overwrite", OnContextMenuOverwrite));
        //    output.Add(("Delete", OnContextMenuDelete));
        //    output.Add(("Rename", OnContextMenuRename));
        //    NPCManager.ValidNPCs().ForEach(x => output.Add(($"Set as {x}", () => ApplyToNPC(x))));
        //}
        //if (this.Error == RecipeFile.Status.Incomplete || recipe.Error == RecipeFile.Status.SlowSource)
        //{
        //    output.Add(("Load*", OnContextMenuWarningLoad));
        //    output.Add(("Show Missing", OnContextMenuListMissing));
        //}
        //if (this.Extension == Constants.JsonFileExtension
        //    && this.Error == RecipeFile.Status.NoError)
        //{
        //    output.Add(("Update to .png", ConvertToPNG));
        //}
        //if (this.Extension == Constants.PngFileExtension
        //    && this.Error != RecipeFile.Status.FileError)
        //{
        //    output.Add(("Show in Explorer", ShowInExplorer));
        //}
        //if (this.Error == RecipeFile.Status.FileError)
        //{
        //    output.Add(("Ignore", () => Destroy(this.gameObject)));
        //}
        return output;
    }
}

public partial record RecipeFile : IPath
{
    PathDescriptor IPath.PathDescriptor => new(System.IO.Path.GetDirectoryName(this.Path), PathType.Filesystem);
}