using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Contracts;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System;
using System.Collections;
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

    Recipe recipe;
    Main.ContextMenu contextMenu;
    FilenameDialogue filenameDialogue;
    MessageDialogue messageDialogue;

    Image background;
    Image displayImage;
    Text displayName;
    Text pickupLocation;

    public void Constructor(
        Recipe recipe,
        UIAssetLoader loader,
        Main.ContextMenu contextMenu,
        FilenameDialogue filenameDialogue,
        MessageDialogue messageDialogue)
    {
        this.recipe = recipe;
        this.contextMenu = contextMenu;
        this.filenameDialogue = filenameDialogue;
        this.messageDialogue = messageDialogue;
        name = this.recipe.Name;

        displayImage = transform.Find(displayImageAddress)?.GetComponent<Image>();
        displayImage.sprite = loader.PirateIcon;
        displayImage.enabled = false;

        if (recipe.Extension == Constants.PngFileExtension) 
            CCPlugin.CoroutineRunner.StartCoroutine(LoadThumbnail());

        displayName = transform.Find(outfitNameAddress)?.GetComponentInChildren<Text>();
        displayName.text = this.recipe.Name;

        background = transform.GetChild(0).gameObject.GetComponent<Image>();
        background.color = GetUIColor();

        pickupLocation = transform.Find(pickupLocationAddress)?.GetComponent<Text>();
        pickupLocation.text = GetStatusMessage();
    }


    IEnumerator LoadThumbnail()
    {
        yield return new WaitForEndOfFrame();

        var bytes = File.ReadAllBytes(recipe.Path);
        var sprite = displayImage.sprite;
        Texture2D thumbnail = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(thumbnail, bytes)) { Log.Warning($"failed to load png for {recipe.Name}"); yield break; }

        displayImage.sprite = Sprite
            .Create(
                thumbnail,
                new Rect (0,0,thumbnail.width,thumbnail.height),
                new Vector2(0.5f, 0.5f));
        displayImage.sprite.name = recipe.Name ;
        displayImage.enabled = true;
        yield break;
    }

    void OnShezaraChanged(object sender, EventArgs e)
    {
        if (!displayImage) return; 
        
        displayImage.enabled = (e.AsConfigEntry<string>().Value == recipe.Name);
    }

    string GetStatusMessage()
    {
        switch (recipe.Error)
        {
            case Recipe.Status.MissingSource:   return $"Missing {RecipeApplier.GetMissingSources(recipe.Descriptor).Count()} sources";
            case Recipe.Status.InvalidJson:     return "Recipe data invalid";
            case Recipe.Status.FileError:       return "Error loading file";
            default: return "";
        }
    }

    Color GetUIColor()
    {
        switch (recipe.Error)
        {
            case Recipe.Status.MissingSource: return Constants.DefaultColor;
            case Recipe.Status.InvalidJson: return Color.gray;
            case Recipe.Status.FileError: return Color.gray;
            default: return Constants.DefaultColor;
        }
    }

    void OnContextMenuOverwrite()
    {
        string message = "Are you sure you'd like to overwrite this recipe?";
        messageDialogue.Show(message, confirmText: "Yes!", confirmAction: Overwrite);
    }

    void Overwrite() => RecipeSaver.SavePNG(new RecipeDescriptor24(PlayerInstances.DefaultPlayer.outfitManager), recipe.Path);

    void OnContextMenuLoad(PlayerCarolInstance player) => 
        RecipeApplier.ActivateRecipe(player.outfitManager, recipe.Descriptor);

    void OnContextMenuWarningLoad()
    {
        string message = "Some of the resources for this recipe aren't available: " + Environment.NewLine; ;
        var missingSources = RecipeApplier.GetMissingSources(recipe.Descriptor);
        foreach (var source in missingSources) { message += source + Environment.NewLine; }
        messageDialogue.Show(message, cancelText: "Nevermind", confirmText: "Load Anyway", confirmAction: () => OnContextMenuLoad(PlayerInstances.DefaultPlayer));
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

    void SetAsShezara()
    {
        Log.Debug($"Setting {recipe.Name} as shezara recipe");
        Settings.Plugin.shezaraRecipe.Value = recipe.Name;
    }

    void OnContextMenuListMissing()
    {
        var missingSources = RecipeApplier.GetMissingSources(recipe.Descriptor);
        string message = string.Empty;
        foreach (var source in missingSources) { message += source + Environment.NewLine; }
        messageDialogue.Show(message, cancelText: "Done");
    }

    void OnRename(RecipeDescriptor24 _, string newName)
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
            new RecipeDescriptor24(PlayerInstances.DefaultPlayer.outfitManager),
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
        if (recipe.Error == Recipe.Status.NoError)
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
        if (recipe.Error != Recipe.Status.FileError)
        {
            output.Add(("Overwrite", OnContextMenuOverwrite));
            output.Add(("Delete", OnContextMenuDelete));
            output.Add(("Rename", OnContextMenuRename));
            output.Add(("Set Shezara", SetAsShezara));  
            
        }
        if (recipe.Error == Recipe.Status.MissingSource)
        {
            output.Add(("Load*", OnContextMenuWarningLoad));
            output.Add(("Show Missing", OnContextMenuListMissing));
        }
        if (recipe.Extension == Constants.JsonFileExtension
            && recipe.Error == Recipe.Status.NoError)
        {
            output.Add(("Update to .png", ConvertToPNG));
        }
        if (recipe.Extension == Constants.PngFileExtension
            && recipe.Error != Recipe.Status.FileError)
        {
            output.Add(("Show in Explorer", ShowInExplorer));
        }
        if (recipe.Error == Recipe.Status.FileError)
        {
            output.Add(("Ignore", () => Destroy(this.gameObject)));
        }
        return output;
    }
}
