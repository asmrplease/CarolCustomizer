using System.Collections.Generic;
using CarolCustomizer.Utils;
using UnityEngine;
using UnityEngine.UI;
using CarolCustomizer.Assets;
using CarolCustomizer.Models;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Behaviors.Recipes;

namespace CarolCustomizer.UI.Recipes;
public class RecipeListUI : MonoBehaviour
{
    private static readonly string listRootAddress = "Scroll View/Viewport/Content";
    private static readonly string newRecipeAddress = "New Recipe";

    private TabbedUIAssetLoader loader;
    private OutfitManager outfitManager;
    private DynamicContextMenu contextMenu;
    private FilenameDialogue fileDialogue;
    private MessageDialogue messageDialogue;
    private RecipesManager recipesManager;

    private Transform listRoot;
    private GameObject filenameDialogueGO;
    private Button newRecipeButton;

    private Dictionary<string, RecipeUI> recipeUIs = new();

    public void Constructor(
        TabbedUIAssetLoader loader,
        RecipesManager recipesManager,
        OutfitManager outfitManager,
        DynamicContextMenu contextMenu,
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

        this.recipesManager.RefreshAll();
    }

    private void OnDestroy()
    {
        Log.Debug("ReceiptListUI.OnDestroy()");
        recipesManager.OnRecipeCreated -= OnRecipeCreated;
        recipesManager.OnRecipeDeleted -= OnRecipeDeleted;
    }

    public void OnRecipeCreated(Recipe newRecipe)
    {
        Log.Debug("RecipeListUI.OnRecipeCreated");
        //instantiate a outfit ui object
        var uiInstance = Instantiate(loader.OutfitListElement, listRoot);
        if (!uiInstance) { Log.Error("Failed to instantiate outfit UI prefab for a recipeUI."); return; }

        //add a RecipeUI component
        var recipeUI = uiInstance.AddComponent<RecipeUI>();
        if (!recipeUI) { Log.Warning("failed to instantiate recipeUI component"); return; }
        recipeUI.Constructor(newRecipe, this, outfitManager, contextMenu, fileDialogue, messageDialogue);

        //add to list of recipeUIs
        recipeUIs.Add(newRecipe.Path, recipeUI);
    }

    public void OnRecipeDeleted(Recipe removedRecipe)
    {
        Destroy(recipeUIs[removedRecipe.Path].gameObject);
        recipeUIs.Remove(removedRecipe.Path);
    }

    private void OnNewSave()
    {
        fileDialogue.Show(new RecipeDescriptor20(outfitManager), RecipeSaver.Save);
    }
}
