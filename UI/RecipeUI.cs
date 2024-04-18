using CarolCustomizer.Behaviors;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CarolCustomizer.Utils;
using CarolCustomizer.Contracts;
using System.Collections.Generic;
using UnityEngine.Events;
using BepInEx;
using System;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Behaviors.Settings;

namespace CarolCustomizer.UI;
public class RecipeUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    Recipe recipe;
    RecipeListUI ui;
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
        RecipeListUI ui, 
        OutfitManager outfitManager, 
        DynamicContextMenu contextMenu, 
        FilenameDialogue filenameDialogue, 
        MessageDialogue messageDialogue)
    {
        this.ui = ui;
        this.recipe = recipe;
        this.outfitManager = outfitManager;
        this.contextMenu = contextMenu;
        this.filenameDialogue = filenameDialogue;
        this.messageDialogue = messageDialogue;
        this.name = this.recipe.Name;

        displayImage = this.transform.Find(displayImageAddress)?.GetComponent<Image>();
        displayImage.enabled = false;

        displayName = this.transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = this.recipe.Name;

        background = this.transform.GetChild(0).gameObject.GetComponent<Image>();
        background.color = GetUIColor();

        pickupLocation = this.transform.Find(pickupLocationAddress)?.GetComponent<Text>();
        pickupLocation.text = GetStatusMessage();
    }

    private string GetStatusMessage()
    {
        switch (this.recipe.Error)
        {
            case Recipe.Status.MissingSource:  return $"Missing {RecipeApplier.GetMissingSources(this.recipe.Descriptor).Count()} sources";
            case Recipe.Status.InvalidJson:    return "Recipe file invalid";
            case Recipe.Status.FileError:      return "Error loading file";
            default: return "";
        }
    }

    private Color GetUIColor()
    {
        switch (this.recipe.Error)
        {
            case Recipe.Status.MissingSource: return Constants.DefaultColor;
            case Recipe.Status.InvalidJson:   return Color.gray;
            case Recipe.Status.FileError:     return Color.gray;
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
        RecipeSaver.Save(new RecipeDescriptor20(this.outfitManager), this.recipe.Path);
    }

    private void OnContextMenuLoad()
    {
        RecipeApplier.ActivateRecipe(outfitManager, this.recipe.Descriptor);
    }

    private void OnContextMenuWarningLoad()
    {
        string message = "Some of the resources for this recipe aren't available: " + Environment.NewLine; ;
        var missingSources = RecipeApplier.GetMissingSources(this.recipe.Descriptor);
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

    private void DeleteRecipe()
    {
        System.IO.File.Delete(this.recipe.Path);
    }

    private void OnContextMenuListMissing()
    {
        var missingSources = RecipeApplier.GetMissingSources(this.recipe.Descriptor);
        string message = string.Empty;
        foreach (var source in missingSources) { message += source + Environment.NewLine; }
        messageDialogue.Show(message, cancelText: "Done");
    }

    private void OnRename(RecipeDescriptor20 unused, string newName)
    {
        if (newName.IsNullOrWhiteSpace()) return;
        foreach (var character in Path.GetInvalidFileNameChars()) { if (newName.Contains(character)) return; }

        if (!newName.ToLower().EndsWith(Constants.RecipeExtension)) newName += Constants.RecipeExtension;
        var newPath = RecipeSaver.RecipeFilenameToPath(newName);

        File.Move(this.recipe.Path, newPath);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) contextMenu.Show(this);
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var output = new List<(string, UnityAction)>();
        if (this.recipe.Error != Recipe.Status.FileError)
        {
            output.Add(("Overwrite", OnContextMenuOverwrite));
            output.Add(("Delete", OnContextMenuDelete));
            output.Add(("Rename", OnContextMenuRename));
        }
        if (this.recipe.Error == Recipe.Status.NoError)
        {
            output.Add(("Load", OnContextMenuLoad));
        }
        if (this.recipe.Error == Recipe.Status.MissingSource)
        {
            output.Add(("Load*", OnContextMenuWarningLoad));
            output.Add(("Show Missing", OnContextMenuListMissing));
        }
        return output;
    }
}
