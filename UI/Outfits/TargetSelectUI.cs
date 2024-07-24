using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Outfits;
internal class TargetSelectUI : MonoBehaviour
{
    const string targetSelectAddress = "Target Select";
    const string scrollViewAddress = "Scroll View";
    const int TargetSelectHeight = 32;

    Dropdown TargetSelect;
    RectTransform scrollView;
    int playerCount = 0;

    public static event Action<CarolInstance> OnCarolSelectionChanged;

    void Awake()
    {
        TargetSelect = transform
            .Find(targetSelectAddress)
            .GetComponent<Dropdown>();
        scrollView = transform.Find(scrollViewAddress) as RectTransform;
        TargetSelect.onValueChanged.AddListener(HandleSelection);
        MenuToggle.OnMenuToggle += HandleMenuToggle;
    }

    void OnDestroy()
    {
        MenuToggle.OnMenuToggle -= HandleMenuToggle;
        TargetSelect.onValueChanged.RemoveListener(HandleSelection);
    }

    void HandleSelection(int index)
    {
        var selectedCarol = PlayerInstances.DefaultPlayer;
        if (index < PlayerInstances.ValidPlayers.Count()) selectedCarol = PlayerInstances.ValidPlayers.ToList()[index];
        OnCarolSelectionChanged?.Invoke(selectedCarol);
    }

    void HandleMenuToggle(bool state)
    {
        Log.Debug("OutfitListUI.HandleMenuToggle()");
        var currentPlayerCount = PlayerInstances.ValidPlayers.Count();
        if (playerCount != currentPlayerCount)
        {
            playerCount = currentPlayerCount;
            UpdateTargetDropdownSelection();
        }
        if (playerCount > 1) ShowTargetSelect();
        else HideTargetSelect();
    }

    void UpdateTargetDropdownSelection()
    {
        TargetSelect.options.Clear();
        PlayerInstances.ValidPlayers
            .Select((x, i) => new Dropdown.OptionData($"Player {i + 1}"))
            .ForEach(TargetSelect.options.Add);
        TargetSelect.RefreshShownValue();
    }

    void ShowTargetSelect()
    {
        if (TargetSelect.gameObject.activeSelf) return;

        TargetSelect.gameObject.SetActive(true);
        
        var size = scrollView.sizeDelta;
        size.y -= TargetSelectHeight;
        scrollView.sizeDelta = size;
    }

    void HideTargetSelect()
    {
        if (!TargetSelect.gameObject.activeSelf) return;

        TargetSelect.gameObject.SetActive(false);
        var size = scrollView.sizeDelta;
        size.y += TargetSelectHeight;
        scrollView.sizeDelta = size;
        HandleSelection(0);
    }
}
