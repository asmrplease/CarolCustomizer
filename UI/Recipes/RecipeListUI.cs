using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.Models.Recipes;
using CarolCustomizer.UI.Main;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Recipes;
public class RecipeListUI : MonoBehaviour
{
    static readonly string listRootAddress = "Scroll View/Viewport/Content";
    static readonly string searchBoxAddress = "Search Box";
    static readonly string newRecipeAddress = "New Recipe";

    UIAssetLoader loader;
    Main.ContextMenu contextMenu;
    FilenameDialogue fileDialogue;
    MessageDialogue messageDialogue;
    RecipeFileWatcher recipesManager;

    Transform listRoot;
    GameObject filenameDialogueGO;
    Button newRecipeButton;
    InputField searchBox;
    Text searchBoxHint;

    SortedList<string, RecipeUI> recipeUIs = new();

    public RecipeListUI Constructor(
        UIAssetLoader loader,
        RecipeFileWatcher recipesManager,
        Main.ContextMenu contextMenu,
        MessageDialogue messageDialogue)
    {
        this.loader = loader;
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

        searchBox = transform
            .Find(searchBoxAddress)
            .GetComponent<InputField>();
        searchBoxHint = searchBox.transform
            .GetChild(1)
            .GetComponent<Text>();
        searchBox.onEndEdit.AddListener(OnSearchBoxChanged);
        searchBoxHint.text = "Search Recipes";
        gameObject.SetActive(false);

        return this;
    }

    void OnDestroy()
    {
        Log.Debug("ReceiptListUI.OnDestroy()");
        recipesManager.OnRecipeCreated -= OnRecipeCreated;
        recipesManager.OnRecipeDeleted -= OnRecipeDeleted;
    }

    void OnSearchBoxChanged(string text)
    {
        string search = text.Trim();
        bool empty = search == string.Empty;
        recipeUIs.ToList()
            .ForEach(kvp => 
                kvp.Value.gameObject.SetActive(
                    empty || 
                    kvp.Key.Contains(search, System.StringComparison.OrdinalIgnoreCase)));
    }

    public void OnRecipeCreated(Recipe newRecipe)
    {
        Log.Debug("RecipeListUI.OnRecipeCreated");

        var uiInstance = Instantiate(loader.OutfitListElement, listRoot);
        if (!uiInstance) { Log.Error("Failed to instantiate outfit UI prefab for a recipeUI."); return; }

        var recipeUI = uiInstance.AddComponent<RecipeUI>();
        if (!recipeUI) { Log.Warning("failed to instantiate recipeUI component"); return; }
        recipeUI.Constructor(newRecipe, loader, contextMenu, fileDialogue, messageDialogue);

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
        fileDialogue.Show(new RecipeDescriptor23(PlayerInstances.DefaultPlayer.outfitManager), RecipeSaver.SavePNG);
    }
}
