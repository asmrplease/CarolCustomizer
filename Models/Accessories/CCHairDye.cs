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
}

public partial record CCHairDye : IListable
{
    Sprite IListable.Thumbnail => this.HairDye.visual;
    string IListable.Header => this.DisplayName;
    string IListable.Subheader => "";
    Color IListable.BaseColor => Constants.DefaultColor;
    Color IListable.HighlightColor => Constants.Highlight;
    IEnumerable<IListable> IListable.Children => [];
}

public partial record CCHairDye : IContextMenuActions
{
    [MenuItem("Apply to Hair")]
    void ApplyToOutfit()
    {
        Log.Debug($"CCHairDye({this.DisplayName}).ApplyToOutfit");
        var outfit = PlayerInstances.DefaultPlayer.outfitManager;
        var hair = outfit.ActiveAccessories.Where(x => x.Source.Type == SourceType.Hair);
        hair
            .Select(x => OutfitAssetManager.Hairstyles[x.Source])
            .Select(x => x.hairstyle.mainMaterialIndex)
            .Zip(hair, (index, hair) => (index, hair))
            .ForEach(tup => PlayerInstances.DefaultPlayer.outfitManager
                .PaintAccessory(tup.hair, MaterialDescriptor, tup.index));
    }

    List<ContextButton> IContextMenuActions.GetContextMenuItems()
    {
        var results = this.MaterialDescriptor.GetContextMenuItems();
        results.AddRange(this.AutoMenuItems());
        return results;
    }
}
