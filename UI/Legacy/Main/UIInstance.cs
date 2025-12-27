using CarolCustomizer.Assets;
using CarolCustomizer.Behaviors;
using CarolCustomizer.Behaviors.Recipes;
using CarolCustomizer.UI.Legacy.Config;
using CarolCustomizer.UI.Legacy.Materials;
using CarolCustomizer.UI.Legacy.Outfits;
using CarolCustomizer.UI.Legacy.Recipes;
using CarolCustomizer.Utils;
using FuseBox.Lugh;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Legacy.Main;

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

    Button outfitsButton;
    Button recipesButton;
    Button materialsButton;
    Button configButton;

    public LughPanel lughPanel;

    #region Common Component References
    Canvas canvas;
    List<Component> Views;
    List<Button> Buttons;
    internal LoadingIndicator loadingIndicator;
    #endregion

    #region Public Interface
    public UIInstance Constructor(UIAssetLoader loader, RecipeFileWatcher recipeFileWatcher)
    {
        var mainTransform = Instantiate(loader.UIContainer, transform).transform;
        mainTransform
            .Find(versionAddress)
            .GetComponent<Text>()
            .text = PluginInfo.PLUGIN_VERSION.ToString();
        lughPanel = mainTransform.gameObject.AddComponent<LughPanel>();

        canvas = mainTransform.GetComponent<Canvas>();
        canvas.enabled = true;
        UIElementFactory factory = new(loader);

        var viewRoot = mainTransform.Find(viewRootAddress);

        var contextMenu = Instantiate(loader.ContextMenu, mainTransform)
            .AddComponent<ContextMenu>()
            .Constructor(loader.ContextMenuButton);
        var messageDialogue = Instantiate(loader.MessageDialogue, mainTransform)
            .AddComponent<MessageDialogue>()
            .Constructor();
        var outfitView = Instantiate(loader.OutfitView, viewRoot)
            .AddComponent<NewOutfitListUI>()
            .Constructor(factory, recipeFileWatcher, contextMenu);
        var recipesView = Instantiate(loader.RecipesView, viewRoot)
            .AddComponent<RecipeListUI>()
            .Constructor(loader, recipeFileWatcher, contextMenu, messageDialogue);
        var materialsView = Instantiate(loader.MaterialsView, viewRoot)
            .AddComponent<MaterialsListUI>()
            .Constructor(loader, contextMenu);
        var configView = Instantiate(loader.SettingsView, viewRoot)
            .AddComponent<ConfigUI>()
            .Constructor(messageDialogue);

        loadingIndicator = viewRoot.gameObject
            .AddComponent<LoadingIndicator>()
            .Constructor(viewRoot);
        outfitsButton =   mainTransform.SetupButton(outfitsButtonAddress, 
            () => ChangeTab(outfitsButton, outfitView.gameObject));
        recipesButton =   mainTransform.SetupButton(recipesButtonAddress, 
            () => ChangeTab(recipesButton, recipesView.gameObject ));
        materialsButton = mainTransform.SetupButton(materialsButtonAddress,
            () => ChangeTab(materialsButton, materialsView.gameObject));
        configButton =  mainTransform.SetupButton(settingsButtonAddress,
            () => ChangeTab(configButton, configView.gameObject));

        Views = [ outfitView, recipesView, materialsView, configView];
        Buttons = [outfitsButton, recipesButton, materialsButton, configButton ];
        
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
