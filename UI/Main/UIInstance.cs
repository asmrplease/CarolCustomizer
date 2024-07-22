using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.UI.Config;
using CarolCustomizer.UI.Materials;
using CarolCustomizer.UI.Outfits;
using CarolCustomizer.UI.Recipes;
using CarolCustomizer.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Main;

public class UIInstance : MonoBehaviour
{
    #region Static Addresses
    private static readonly string viewRootAddress = "Accessory Panel";
    private static readonly string versionAddress = "Accessory Panel/TitleBar/Version";
    private static readonly string outfitsButtonAddress = "Accessory Panel/Tabs/Outfits";
    private static readonly string recipesButtonAddress = "Accessory Panel/Tabs/Recipes";
    private static readonly string materialsButtonAddress = "Accessory Panel/Tabs/Materials";
    private static readonly string settingsButtonAddress = "Accessory Panel/Tabs/Settings";
    #endregion

    #region Dependencies
    public MaterialManager materialManager { get; private set; }
    #endregion

    Button outfitsButton;
    Button recipesButton;
    Button materialsButton;
    Button configButton;

    #region Common Component References
    Canvas canvas;
    List<Component> Views;
    List<Button> Buttons;
    #endregion

    #region Public Interface
    public UIInstance Constructor(UIAssetLoader loader, RecipeFileWatcher recipeFileWatcher)
    {
        materialManager = new();

        var mainTransform = Instantiate(loader.UIContainer, transform).transform;
        mainTransform
            .Find(versionAddress)
            .GetComponent<Text>()
            .text = PluginInfo.PLUGIN_VERSION.ToString();

        canvas = mainTransform.GetComponent<Canvas>();
        canvas.enabled = true;

        var viewRoot = mainTransform.Find(viewRootAddress);

        var contextMenu = Instantiate(loader.ContextMenu, mainTransform)
            .AddComponent<ContextMenu>()
            .Constructor(loader.ContextMenuButton);
        var messageDialogue = Instantiate(loader.MessageDialogue, mainTransform)
            .AddComponent<MessageDialogue>()
            .Constructor();

        var outfitView = Instantiate(loader.OutfitView, viewRoot)
            .AddComponent<OutfitListUI>()
            .Constructor(loader, materialManager, contextMenu);
        var recipesView = Instantiate(loader.RecipesView, viewRoot)
            .AddComponent<RecipeListUI>()
            .Constructor(loader, recipeFileWatcher, contextMenu, messageDialogue);
        var materialsView = Instantiate(loader.MaterialsView, viewRoot)
            .AddComponent<MaterialsListUI>()
            .Constructor(loader, materialManager, contextMenu);
        var configView = Instantiate(loader.SettingsView, viewRoot)
            .AddComponent<ConfigUI>()
            .Constructor(messageDialogue);

        outfitsButton =   mainTransform.SetupButton(outfitsButtonAddress, 
            () => ChangeTab(outfitsButton, outfitView.gameObject));
        recipesButton =   mainTransform.SetupButton(recipesButtonAddress, 
            () => ChangeTab(recipesButton, recipesView.gameObject ));
        materialsButton = mainTransform.SetupButton(materialsButtonAddress,
            () => ChangeTab(materialsButton, materialsView.gameObject));
        configButton =  mainTransform.SetupButton(settingsButtonAddress,
            () => ChangeTab(configButton, configView.gameObject));

        Views = new List<Component>() { outfitView, recipesView, materialsView, configView };
        Buttons = new List<Button> { outfitsButton, recipesButton, materialsButton, configButton };
        
        ChangeTab(outfitsButton, outfitView.gameObject);
        gameObject
            .AddComponent<MenuToggle>()
            .Constructor(this);
        MenuToggle.OnMenuToggle += HandleMenuToggle;
        return this;
    }

    void HandleMenuToggle(bool visible)
    {
        Log.Debug("UIInstance.HandleMenuToggle()");
        canvas.enabled = visible;
        if (!visible) EventSystem.current.SetSelectedGameObject(null);
    }
    #endregion

    void ChangeTab(Button button, GameObject view)
    {
        Log.Debug($"{button.name}");
        Views.ForEach(x => x.gameObject.SetActive(false));
        Buttons.ForEach(x => x.GetComponent<Image>().color = Constants.DefaultColor);
        view.SetActive(true);
        button.GetComponent<Image>().color = Constants.Highlight;
    }
}
