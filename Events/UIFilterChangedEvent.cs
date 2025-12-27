using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.UI.Legacy.Outfits;
using System.Linq;

namespace CarolCustomizer.Events;
public class UIFilterChangedEvent
{
    public readonly string Text;
    public readonly bool ShowFavorites;
    public readonly bool ShowActive;
    public readonly bool AnyFilters;
    public readonly bool HasText;

    public UIFilterChangedEvent(string filterText, bool ShowFavorites, bool ShowActive)
    {
        this.Text = filterText.Trim();
        this.ShowFavorites = ShowFavorites;
        this.ShowActive = ShowActive;
        this.HasText = this.Text != string.Empty;
        this.AnyFilters = HasText
            || ShowActive
            || ShowFavorites;
    }

    public override string ToString()
    {
        return 
            $"Enabled: {AnyFilters}, " +
            $"HasText: {HasText}, " +
            $"Text: '{Text}', " +
            $"ShowFavorites: {ShowFavorites}, " +
            $"ShowActive: {ShowActive}.";
    }

    public bool Filter(IModelProvider source)
    {
        //if (this.ShowFavorites && Settings.Favorites.IsInFavorites(accessory)) return true;
        //if (this.ShowActive && OutfitListUI.TargetOutfit.ActiveAccessories.Contains(accessory)) return true;
        if (this.HasText && source.ToString().Contains(this.Text)) return true;
        return false;
    }

    public bool Filter(MaterialDescriptor material)
    {
        if (this.HasText && material.Name.Contains(this.Text)) return true;
        return false;
    }

    public bool Filter(AccessoryDescriptor accessory)
    {
        if (this.ShowFavorites && Settings.Favorites.IsInFavorites(accessory)) return true;
        if (this.ShowActive && OutfitListUI.TargetOutfit.ActiveAccessories.Contains(accessory)) return true;
        if (this.HasText && accessory.ToString().Contains(this.Text)) return true;
        return false;
    }
}
