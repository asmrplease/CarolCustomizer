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

    //Dictionary<string, RecipeUI> recipeUIs = new();
    SortedList<string, RecipeUI> recipeUIsSorted = new();

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

        recipeUIsSorted.Add(newRecipe.Path, recipeUI);
        recipeUI.transform.SetSiblingIndex(recipeUIsSorted.IndexOfKey(newRecipe.Path));
    }

    public void OnRecipeDeleted(Recipe removedRecipe)
    {
        GameObject.Destroy(recipeUIsSorted[removedRecipe.Path].gameObject);
        recipeUIsSorted.Remove(removedRecipe.Path);
    }

    void OnNewSave()
    {
        fileDialogue.Show(new RecipeDescriptor23(outfitManager), RecipeSaver.SavePNG);
    }
}
