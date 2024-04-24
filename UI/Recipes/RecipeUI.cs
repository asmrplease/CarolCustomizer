using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Recipes;
public class RecipeUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    Recipe recipe;
    OutfitManager outfitManager;
    DynamicContextMenu contextMenu;
    FilenameDialogue filenameDialogue;
    MessageDialogue messageDialogue;

    private Image background;
    private Image displayImage;
    private Text displayName;
    private Text pickupLocation;

    static readonly string displayImageAddress = "Outfit Header/Icon";
    static readonly string outfitNameAddress = "Outfit Header/Text/Outfit Name";
    static readonly string pickupLocationAddress = "Outfit Header/Text/Pickup Location";

    public void Constructor(
        Recipe recipe,
        TabbedUIAssetLoader loader,
        OutfitManager outfitManager,
        DynamicContextMenu contextMenu,
        FilenameDialogue filenameDialogue,
        MessageDialogue messageDialogue)
    {
        this.recipe = recipe;
        this.outfitManager = outfitManager;
        this.contextMenu = contextMenu;
        this.filenameDialogue = filenameDialogue;
        this.messageDialogue = messageDialogue;
        name = this.recipe.Name;

        displayImage = transform.Find(displayImageAddress)?.GetComponent<Image>();
        displayImage.sprite = loader.PirateIcon;
        displayImage.enabled = Settings.Plugin.shezaraRecipe.Value == recipe.Name;
        Settings.Plugin.shezaraRecipe.SettingChanged += OnShezaraChanged;

        displayName = transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = this.recipe.Name;

        background = transform.GetChild(0).gameObject.GetComponent<Image>();
        background.color = GetUIColor();

        pickupLocation = transform.Find(pickupLocationAddress)?.GetComponent<Text>();
        pickupLocation.text = GetStatusMessage();
    }

    void OnDestroy()
    {
        Settings.Plugin.shezaraRecipe.SettingChanged -= OnShezaraChanged;
    }

    private void OnShezaraChanged(object sender, EventArgs e)
    {
        displayImage.enabled = (e.AsConfigEntry<string>().Value == recipe.Name);
    }

    private string GetStatusMessage()
    {
        switch (recipe.Error)
        {
            case Recipe.Status.MissingSource: return $"Missing {RecipeApplier.GetMissingSources(recipe.Descriptor).Count()} sources";
            case Recipe.Status.InvalidJson: return "Recipe file invalid";
            case Recipe.Status.FileError: return "Error loading file";
            default: return "";
        }
    }

    private Color GetUIColor()
    {
        switch (recipe.Error)
        {
            case Recipe.Status.MissingSource: return Constants.DefaultColor;
            case Recipe.Status.InvalidJson: return Color.gray;
            case Recipe.Status.FileError: return Color.gray;
            default: return Constants.DefaultColor;
        }
    }

    private void OnContextMenuOverwrite()
    {
        string message = "Are you sure you'd like to overwrite this recipe?";
        messageDialogue.Show(message, confirmText: "Yes!", confirmAction: Overwrite);
    }

    private void Overwrite()
    {
        RecipeSaver.Save(new RecipeDescriptor21(outfitManager), recipe.Path);
    }

    private void OnContextMenuLoad()
    {
        RecipeApplier.ActivateRecipe(outfitManager, recipe.Descriptor);
    }

    private void OnContextMenuWarningLoad()
    {
        string message = "Some of the resources for this recipe aren't available: " + Environment.NewLine; ;
        var missingSources = RecipeApplier.GetMissingSources(recipe.Descriptor);
        foreach (var source in missingSources) { message += source + Environment.NewLine; }
        messageDialogue.Show(message, cancelText: "Nevermind", confirmText: "Load Anyway", confirmAction: OnContextMenuLoad);
    }

    private void OnContextMenuRename()
    {
        Log.Debug("OnRename");
        filenameDialogue.Show(null, OnRename);
    }

    private void OnContextMenuDelete()
    {
        string message = "Are you sure you want to delete this recipe!?";
        messageDialogue.Show(message, cancelText: "Wait, no.", confirmText: "Delete!", confirmAction: DeleteRecipe);
    }

    private void DeleteRecipe() => File.Delete(recipe.Path);

    void SetAsShezara()
    {
        Log.Debug($"Setting {recipe.Name} as shezara recipe");
        Settings.Plugin.shezaraRecipe.Value = recipe.Name;
    }

    private void OnContextMenuListMissing()
    {
        var missingSources = RecipeApplier.GetMissingSources(recipe.Descriptor);
        string message = string.Empty;
        foreach (var source in missingSources) { message += source + Environment.NewLine; }
        messageDialogue.Show(message, cancelText: "Done");
    }

    private void OnRename(RecipeDescriptor21 unused, string newName)
    {
        if (newName.Trim() == "") return;
        foreach (var character in Path.GetInvalidFileNameChars()) { if (newName.Contains(character)) return; }

        if (!newName.ToLower().EndsWith(Constants.RecipeExtension)) newName += Constants.RecipeExtension;
        var newPath = RecipeSaver.RecipeFilenameToPath(newName);

        File.Move(recipe.Path, newPath);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) contextMenu.Show(this);
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var output = new List<(string, UnityAction)>();
        if (recipe.Error != Recipe.Status.FileError)
        {
            output.Add(("Overwrite", OnContextMenuOverwrite));
            output.Add(("Delete", OnContextMenuDelete));
            output.Add(("Rename", OnContextMenuRename));
            output.Add(("Set Shezara", SetAsShezara));
        }
        if (recipe.Error == Recipe.Status.NoError)
        {
            output.Add(("Load", OnContextMenuLoad));
        }
        if (recipe.Error == Recipe.Status.MissingSource)
        {
            output.Add(("Load*", OnContextMenuWarningLoad));
            output.Add(("Show Missing", OnContextMenuListMissing));
        }
        return output;
    }
}
