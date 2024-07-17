using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Recipes;
public class RecipeListUI : MonoBehaviour
{
    static readonly string listRootAddress = "Scroll View/Viewport/Content";
    static readonly string newRecipeAddress = "New Recipe";

    UIAssetLoader loader;
    OutfitManager outfitManager;
    Main.ContextMenu contextMenu;
    FilenameDialogue fileDialogue;
    MessageDialogue messageDialogue;
    RecipeFileWatcher recipesManager;

    Transform listRoot;
    GameObject filenameDialogueGO;
    Button newRecipeButton;

    SortedList<string, RecipeUI> recipeUIs = new();

    public RecipeListUI Constructor(
        UIAssetLoader loader,
        RecipeFileWatcher recipesManager,
        OutfitManager outfitManager,
        Main.ContextMenu contextMenu,
        MessageDialogue messageDialogue)
    {
        this.loader = loader;
        this.outfitManager = outfitManager;
        this.recipesManager = recipesManager;
        this.contextMenu = contextMenu;
        this.messageDialogue = messageDialogue;

        listRoot = transform.Find(listRootAddress);

        filenameDialogueGO = Instantiate(loader.FilenameDialogue, transform.parent.parent);
        fileDialogue = filenameDialogueGO.AddComponent<FilenameDialogue>();
        fileDialogue.Constructor();
        filenameDialogueGO.SetActive(false);

        var newRecipeButtonGO = transform.Find(newRecipeAddress);
        newRecipeButton = newRecipeButtonGO.GetComponent<Button>();
        newRecipeButton.onClick.AddListener(OnNewSave);

        this.recipesManager.OnRecipeCreated += OnRecipeCreated;
        this.recipesManager.OnRecipeDeleted += OnRecipeDeleted;

        gameObject.SetActive(false);

        return this;
    }

    void OnDestroy()
    {
        Log.Debug("ReceiptListUI.OnDestroy()");
        recipesManager.OnRecipeCreated -= OnRecipeCreated;
        recipesManager.OnRecipeDeleted -= OnRecipeDeleted;
    }

    public void OnRecipeCreated(Recipe newRecipe)
    {
        Log.Debug("RecipeListUI.OnRecipeCreated");

        var uiInstance = Instantiate(loader.OutfitListElement, listRoot);
        if (!uiInstance) { Log.Error("Failed to instantiate outfit UI prefab for a recipeUI."); return; }

        var recipeUI = uiInstance.AddComponent<RecipeUI>();
        if (!recipeUI) { Log.Warning("failed to instantiate recipeUI component"); return; }
        recipeUI.Constructor(newRecipe, loader, outfitManager, contextMenu, fileDialogue, messageDialogue);

        recipeUIs.Add(newRecipe.Path, recipeUI);
        Log.Debug($"Sibling index: {recipeUIs.IndexOfKey(newRecipe.Path)}");
        recipeUI.transform.SetSiblingIndex(recipeUIs.IndexOfKey(newRecipe.Path));
    }

    public void OnRecipeDeleted(Recipe removedRecipe)
    {
        if (!recipeUIs.TryGetValue(removedRecipe.Path, out var ui)) { Log.Warning("tried to remove non-existant recipe ui element."); return; }

        GameObject.Destroy(ui.gameObject);
        recipeUIs.Remove(removedRecipe.Path);
    }

    void OnNewSave()
    {
        fileDialogue.Show(new RecipeDescriptor23(outfitManager), RecipeSaver.SavePNG);
    }
}
