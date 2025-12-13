using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using Onirism.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace CarolCustomizer.Models.Accessories;
public partial record CCHairDye
{
    public readonly MaterialDescriptor MaterialDescriptor;
    public readonly HairDye HairDye;
    public readonly string DisplayName;
    SourceDescriptor SourceDescriptor = new(Constants.HairDyeSourceName, SourceType.Hair);

    public CCHairDye(HairDye source)
    {
        this.HairDye = source;
        this.MaterialDescriptor = new MaterialDescriptor(source.material, this.SourceDescriptor);
        this.DisplayName = LocalizationIndex.GetLine(source.localizationName);
    }

    void ApplyToOutfit(OutfitCoordinator outfit)
    {
        Log.Debug($"CCHairDye({this.DisplayName}).ApplyToOutfit");
        //foreach hairstyles in current outfit
        var hair = outfit.ActiveAccessories.Where(x => x.Source.Type == SourceType.Hair);
        hair
            .Select(x => OutfitAssetManager.Hairstyles[x.Source])
            .Select(x => x.hairstyle.mainMaterialIndex)
            .Zip(hair, (index, hair) => (index, hair))
            .ForEach(tup => PlayerInstances.DefaultPlayer.outfitManager
                .PaintAccessory(tup.hair, MaterialDescriptor, tup.index));
    }
}

public partial record CCHairDye : IListable
{
    Sprite IListable.Thumbnail => this.HairDye.visual;
    string IListable.Header => this.DisplayName;
    string IListable.Subheader => "";
    Color IListable.BaseColor => Constants.DefaultColor;
    Color IListable.HighlightColor => Constants.Highlight;
    IEnumerable<IListable> IListable.Children => [];
    UnityAction<bool> IListable.OnToggle => null;
    bool IListable.Filter<T>(Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }

    List<(string, UnityAction)> IContextMenuActions.GetContextMenuItems()
    {
        var results = ((IListable)this.MaterialDescriptor).GetContextMenuItems();
        results.Add(("Apply to Hair", () => this.ApplyToOutfit(PlayerInstances.DefaultPlayer.outfitManager)));
        return results;
    }
}
