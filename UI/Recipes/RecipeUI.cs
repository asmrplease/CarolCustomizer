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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Recipes;
public class RecipeUI : MonoBehaviour, IPointerClickHandler, IContextMenuActions
{
    static readonly string displayImageAddress = "Outfit Header/Icon";
    static readonly string outfitNameAddress = "Outfit Header/Text/Outfit Name";
    static readonly string pickupLocationAddress = "Outfit Header/Text/Pickup Location";

    RecipeFile recipe;
    Main.ContextMenu contextMenu;
    FilenameDialogue filenameDialogue;
    MessageDialogue messageDialogue;

    Image background;
    Image displayImage;
    Text displayName;
    Text pickupLocation;

    public void Constructor(
        RecipeFile recipe,
        UIAssetLoader loader,
        Main.ContextMenu contextMenu,
        FilenameDialogue filenameDialogue,
        MessageDialogue messageDialogue)
    {
        this.recipe = recipe;
        this.recipe.OnStatusChanged += OnRecipeChanged;
        this.contextMenu = contextMenu;
        this.filenameDialogue = filenameDialogue;
        this.messageDialogue = messageDialogue;
        name = this.recipe.Name;

        displayImage = transform.Find(displayImageAddress)?.GetComponent<Image>();
        //displayImage.sprite = loader.PirateIcon;
        displayImage.enabled = false;

        if (recipe.Extension == Constants.PngFileExtension)
        {
            var (dimensions, compressedData) = recipe.Png.CompressedFrames[0];
            displayImage.sprite = PngUtil.BuildSprite(dimensions, compressedData);
            displayImage.enabled = true;
        }

        displayName = transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = this.recipe.Name;

        background = transform.GetChild(0).gameObject.GetComponent<Image>();
        pickupLocation = transform.Find(pickupLocationAddress)?.GetComponent<Text>();
        OnRecipeChanged();
    }

    void OnRecipeChanged()
    {
        pickupLocation.text = GetStatusMessage();
        background.color = GetUIColor();
    }


    string GetStatusMessage()
    {
        return recipe.Error switch
        {
            RecipeFile.Status.Incomplete    => $"Missing {recipe.MissingSources.Count()} sources, {recipe.MissingAccessories.Count()} accs",
            RecipeFile.Status.SlowSource    => $"{SceneResourceProvider.CheckMaterialsReady(RecipeApplier.GetWorldMats(recipe.Descriptor)).Count()} scenes required.",
            RecipeFile.Status.InvalidJson   => "Recipe data invalid",
            RecipeFile.Status.FileError     => "Error loading file",
            _ => "",
        };
    }

    Color GetUIColor()
    {
        return recipe.Error switch
        {
            RecipeFile.Status.Incomplete => Constants.DefaultColor,
            RecipeFile.Status.SlowSource    => Constants.DefaultColor.RGBMultiplied(0.5f),
            RecipeFile.Status.InvalidJson   => Color.gray,
            RecipeFile.Status.FileError     => Color.gray,
            _ => Constants.DefaultColor,
        };
    }

    void OnContextMenuOverwrite()
    {
        string message = "Are you sure you'd like to overwrite this recipe?";
        messageDialogue.Show(message, confirmText: "Yes!", confirmAction: Overwrite);
    }

    void Overwrite() => RecipeSaver.SavePNG(new RecipeDescriptor(PlayerInstances.DefaultPlayer.outfitManager), recipe.Path);

    void OnContextMenuLoad(PlayerCarolInstance player) => RecipeApplier.ActivateRecipe(player.outfitManager, recipe.Descriptor);

    void OnContextMenuWarningLoad()
    {
        string message = string.Empty;
        if (recipe.Error == RecipeFile.Status.Incomplete)
        {
            if (recipe.MissingSources.Any())
            {
                message = "Some of the resources for this recipe aren't available: " + Environment.NewLine;
                foreach (var source in recipe.MissingSources) { message += source + Environment.NewLine; }
            }
            if (recipe.MissingAccessories.Any())
            {
                message += "Some of the accessories for this recipe were not found: " + Environment.NewLine;
                foreach (var acc in recipe.MissingAccessories) { message += acc + Environment.NewLine; }
            }
            
        }
        if (recipe.Error == RecipeFile.Status.SlowSource)
        {
            message = "The following scenes will be loaded in the background to provide materials:" + Environment.NewLine; ;
            var mats = RecipeApplier.GetWorldMats(recipe.Descriptor);
            var unloadedScenes = SceneResourceProvider.CheckMaterialsReady(mats);
            foreach (var scene in unloadedScenes) { message += scene + Environment.NewLine; }
        }
        messageDialogue.Show(message, cancelText: "Don't Load", confirmText: "Load", confirmAction: () => OnContextMenuLoad(PlayerInstances.DefaultPlayer));
    }

    void OnContextMenuRename()
    {
        Log.Debug("OnRename");
        filenameDialogue.Show(null, OnRename);
    }

    void OnContextMenuDelete()
    {
        string message = "Are you sure you want to delete this recipe!?";
        messageDialogue.Show(message, cancelText: "Wait, no.", confirmText: "Delete!", confirmAction: DeleteRecipe);
    }

    void DeleteRecipe() => File.Delete(recipe.Path);

    void ApplyToNPC(NPC type)
    {
        Log.Debug($"Setting {recipe.Name} as {type} recipe");
        Settings.Plugin.customNPCs[type].recipe.Value = recipe.Name;
        var npc = NPCManager.NPCs[type];
        if (!npc.outfitManager.pelvis) return;

        Log.Debug($"{type} is currently active in scene, applying recipe.");
        RecipeApplier.ActivateRecipe(npc.outfitManager, recipe.Descriptor);
    }

    void OnContextMenuListMissing()
    {
        string message = string.Empty;
        if (recipe.Error == RecipeFile.Status.Incomplete)
        {
            var missingSources = RecipeApplier.GetMissingSources(recipe.Descriptor);
            foreach (var source in missingSources) { message += source + Environment.NewLine; }
            foreach (var acc in recipe.MissingAccessories) { message += acc + Environment.NewLine; }
        }
        if (recipe.Error == RecipeFile.Status.SlowSource)
        {
            var mats = RecipeApplier.GetWorldMats(recipe.Descriptor);
            var unloadedScenes = SceneResourceProvider.CheckMaterialsReady(mats);
            foreach (var scene in unloadedScenes) { message += scene + Environment.NewLine; }
        }
        messageDialogue.Show(message, cancelText: "Done");
    }

    void OnRename(RecipeDescriptor _, string newName)
    {
        if (newName.Trim() == "") return;
        foreach (var character in Path.GetInvalidFileNameChars()) { if (newName.Contains(character)) return; }

        if (!newName.ToLower().EndsWith(recipe.Extension)) newName += recipe.Extension;
        var newPath = RecipeSaver.RecipeFilenameToPath(newName);

        File.Move(recipe.Path, newPath);
    }

    void ConvertToPNG()
    {
        RecipeApplier
            .ActivateRecipe(
                PlayerInstances.DefaultPlayer.outfitManager, 
                recipe.Descriptor);
        var newPath = Path.Combine(
            Path.GetDirectoryName(recipe.Path),
            Path.GetFileNameWithoutExtension(recipe.Path)
                + Constants.PngFileExtension); 
        RecipeSaver.SavePNG(
            new RecipeDescriptor(PlayerInstances.DefaultPlayer.outfitManager),
            newPath);
        File.Delete(recipe.Path);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == Settings.HotKeys.ContextMenu) contextMenu.Show(this);
    }

    void ShowInExplorer()
    {
        try
        {
            string argument = "/select, \"" + recipe.Path + "\"";
            Process.Start("explorer.exe", argument);
        }
        catch (Win32Exception e) { Log.Error(e.Message); }
    }

    public List<(string, UnityAction)> GetContextMenuItems()
    {
        var output = new List<(string, UnityAction)>();
        if (recipe.Error == RecipeFile.Status.NoError)
        {
            PlayerInstances.ValidPlayers
                .ForEach(player =>
                    output.Add(
                        ($"Load on P{player.playerIndex+1}"
                        ,()=> OnContextMenuLoad(player))));
            if (PlayerInstances.ValidPlayers.Count() == 0)
            {
                output.Add(("Load", () => OnContextMenuLoad(PlayerInstances.DefaultPlayer)));
            }
            if (recipe.Name.Contains(Constants.AutoSave, StringComparison.CurrentCultureIgnoreCase))
                return output;
        }
        if (recipe.Error != RecipeFile.Status.FileError)
        {
            output.Add(("Overwrite", OnContextMenuOverwrite));
            output.Add(("Delete", OnContextMenuDelete));
            output.Add(("Rename", OnContextMenuRename));
            NPCManager.ValidNPCs().ForEach(x => output.Add(($"Set as {x}", () => ApplyToNPC(x))));
        }
        if (recipe.Error == RecipeFile.Status.Incomplete || recipe.Error == RecipeFile.Status.SlowSource)
        {
            output.Add(("Load*", OnContextMenuWarningLoad));
            output.Add(("Show Missing", OnContextMenuListMissing));
        }
        if (recipe.Extension == Constants.JsonFileExtension
            && recipe.Error == RecipeFile.Status.NoError)
        {
            output.Add(("Update to .png", ConvertToPNG));
        }
        if (recipe.Extension == Constants.PngFileExtension
            && recipe.Error != RecipeFile.Status.FileError)
        {
            output.Add(("Show in Explorer", ShowInExplorer));
        }
        if (recipe.Error == RecipeFile.Status.FileError)
        {
            output.Add(("Ignore", () => Destroy(this.gameObject)));
        }
        return output;
    }
}
