using CarolCustomizer.Events;
using System;
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
        favoriteFilter = transform.Find(filtersAddress + "/Favorites").GetComponent<Toggle>();
        favoriteFilter.onValueChanged.AddListener(ProcessFilters);
        activeFilter = transform.Find(filtersAddress + "/Active").GetComponent<Toggle>();
        activeFilter.onValueChanged.AddListener(ProcessFilters);
        searchBox = transform.Find(searchBoxAddress).GetComponent<InputField>();
        searchBox.onEndEdit.AddListener(OnSearchBoxChanged);
        searchBoxHint = searchBox.transform.GetChild(1).GetComponent<Text>();
        searchBoxHint.text = SearchModeHint;
        return this;
    }
    void OnSearchBoxChanged(string searchString)
    {
        this.searchString = searchString;
        ProcessFilters();
    }

    void ProcessFilters(bool unusedParam = true)
    {
        bool favorites = favoriteFilter.isOn;
        bool active = activeFilter.isOn;
        string textFilter = searchString.Trim();

        FilterChanged?.Invoke(new(textFilter, favorites, active));
    }

}
