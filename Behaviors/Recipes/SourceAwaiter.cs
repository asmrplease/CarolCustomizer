using CarolCustomizer.Assets;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarolCustomizer.Behaviors.Recipes;
public class SourceAwaiter : IDisposable
{
    public static readonly Dictionary<SourceDescriptor, HashSet<ISourceAwaiter>> SourceToRecipeTable = [];

    public SourceAwaiter()
    {
        OutfitAssetManager.OnOutfitLoaded += HandleOutfitLoaded;
        OutfitAssetManager.OnHairLoaded += HandleHairLoaded;
    }

    static void HandleHairLoaded((List<Models.Accessories.StoredHair>, List<Onirism.Gameplay.HairDye>) obj)
    {
        obj.Item1
            .Select(x => x as ISourceDescriptor)
            .ForEach(x => HandleSourceLoaded(x.Descriptor));
        var idk = OutfitAssetManager.HairDyes as IMaterialProvider;
        HandleSourceLoaded(idk.Descriptor);
    }

    static void HandleOutfitLoaded(Outfit outfit)
    {
        HandleSourceLoaded(outfit.Descriptor);
    }

    static void HandleSourceLoaded(SourceDescriptor descriptor)
    {
        if (!SourceToRecipeTable.TryGetValue(descriptor, out var awaitees)) return;

        awaitees.ToList().ForEach(x => x.HandleSourceLoaded(descriptor));
    }


    public static void Register(SourceDescriptor descriptor, ISourceAwaiter awaiter)
    {
        if (!SourceToRecipeTable.TryGetValue(descriptor, out var existing))
        {
            existing = SourceToRecipeTable[descriptor] = [];
        }

        existing.Add(awaiter);
    }

    public void Dispose()
    {
        OutfitAssetManager.OnOutfitLoaded -= HandleOutfitLoaded;
        OutfitAssetManager.OnHairLoaded -= HandleHairLoaded;
    }

}

public interface ISourceAwaiter
{
    void HandleSourceLoaded(SourceDescriptor descriptor);
}
