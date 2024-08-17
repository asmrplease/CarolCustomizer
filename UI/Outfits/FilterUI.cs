using CarolCustomizer.Events;
using CarolCustomizer.Models.Accessories;
using CarolCustomizer.Models.Materials;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Outfits;
internal class FilterUI : MonoBehaviour
{
    const string filtersAddress = "Filters";
    const string SearchModeHint = "Click here to search!";
    const string searchBoxAddress = "Search Box";

    InputField searchBox;
    Text searchBoxHint;
    Toggle favoriteFilter;
    Toggle activeFilter;

    string searchString = "";

    public event Action<UIFilterChangedEvent> FilterChanged;

    public FilterUI Constructor()
    {
        favoriteFilter = transform
            .Find(filtersAddress + "/Favorites")
            .GetComponent<Toggle>();
        activeFilter = transform
            .Find(filtersAddress + "/Active")
            .GetComponent<Toggle>();
        searchBox = transform
            .Find(searchBoxAddress)
            .GetComponent<InputField>();
        searchBoxHint = searchBox.transform
            .Find("Placeholder")
            .GetComponent<Text>();
        favoriteFilter.onValueChanged.AddListener(ProcessFilters);
        activeFilter.onValueChanged.AddListener(ProcessFilters);    
        searchBox.onEndEdit.AddListener(OnSearchBoxChanged);
        searchBoxHint.text = SearchModeHint;
        Log.Debug($"search boxt set: {SearchModeHint}");
        return this;
    }
    void OnSearchBoxChanged(string searchString)
    {
        this.searchString = searchString;
        ProcessFilters();
    }

    public void ProcessFilters(bool unusedParam = true)
    {
        bool favorites = favoriteFilter.isOn;
        bool active = activeFilter.isOn;
        string textFilter = searchString.Trim();

        FilterChanged?.Invoke(new(textFilter, favorites, active));
    }

    public static bool CheckOutfit(Outfit outfit, UIFilterChangedEvent e)
    {
        if (!e.HasText) return false; 

        if (outfit.DisplayName.Contains(e.Text, StringComparison.OrdinalIgnoreCase)) return true;
        if (outfit.Author.Contains(e.Text, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public static bool CheckAccessory(AccessoryDescriptor acc, UIFilterChangedEvent e)
    {
        if (!e.HasText) return false;

        if (acc.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase)) return true;
        return acc.Materials.Select(mat => CheckMaterial(mat, e)).Any(b => b is true);
    }

    public static bool CheckMaterial(MaterialDescriptor material, UIFilterChangedEvent e)
    {
        if (!e.HasText) return false;

        if (material.Name.Contains(e.Text, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

}
