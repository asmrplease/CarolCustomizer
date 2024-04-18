using CarolCustomizer.Behaviors;
using UnityEngine;
using UnityEngine.UI;
using CarolCustomizer.Assets;
using CarolCustomizer.Utils;

namespace CarolCustomizer.UI;

public class UIInstance : MonoBehaviour
{
    private static readonly string viewRootAddress = "Accessory Panel";
    private static readonly string versionAddress = "Accessory Panel/TitleBar/Version";
    private static readonly string outfitsButtonAddress = "Accessory Panel/Tabs/Outfits";
    private static readonly string recipesButtonAddress = "Accessory Panel/Tabs/Recipes";
    private static readonly string materialsButtonAddress = "Accessory Panel/Tabs/Materials";
    private static readonly string settingsButtonAddress = "Accessory Panel/Tabs/Settings";

    #region Dependencies
    private TabbedUIAssetLoader loader;
    public PlayerCarolInstance playerManager { get; private set; }
    public DynamicContextMenu contextMenu { get; private set; }
    public MaterialManager materialManager { get; private set; }
    #endregion

    #region Gameobjects
    private GameObject uiObject;
    private GameObject contextMenuGO;
    private GameObject outfitViewGO;
    private GameObject materialsViewGO;
    private GameObject recipesViewGO;
    private GameObject settingsViewGO;
    private GameObject messageDialogueGO;
    #endregion

    Button outfitsButton;
    Button recipesButton;
    Button materialsButton;
    Button settingsButton;

    #region Common Component References
    private Canvas canvas;
    private OutfitListUI outfitView;
    private RecipeListUI recipesView;
    private MaterialsListUI materialsView;
    private SettingsUI settingsUI;
    private MessageDialogue messageDialogue;
    private Transform viewRoot;
    #endregion

    #region Public Interface
    public void Constructor(
        TabbedUIAssetLoader loader, 
        PlayerCarolInstance player,
        RecipesManager recipesManager)
    {
        this.loader = loader;
        this.playerManager = player;
        this.materialManager = new();

        uiObject = GameObject.Instantiate(this.loader.UIContainer, this.transform);
        viewRoot = uiObject.transform.Find(viewRootAddress);

        contextMenuGO = GameObject.Instantiate(this.loader.ContextMenu, uiObject.transform);
        contextMenu = contextMenuGO.AddComponent<DynamicContextMenu>();
        contextMenu.Constructor(this.loader.ContextMenuButton);
        contextMenuGO.SetActive(false);

        messageDialogueGO = GameObject.Instantiate(this.loader.MessageDialogue, uiObject.transform);
        messageDialogue = messageDialogueGO.AddComponent<MessageDialogue>();
        messageDialogue.Constructor();
        messageDialogueGO.SetActive(false);

        outfitViewGO = GameObject.Instantiate(this.loader.OutfitView, viewRoot);
        outfitView = outfitViewGO.AddComponent<OutfitListUI>();
        outfitView.Constructor(this.loader, player, materialManager, contextMenu);

        outfitsButton = uiObject.transform.Find(outfitsButtonAddress).GetComponent<Button>();
        outfitsButton.onClick.AddListener(OnOutfitButton);

        recipesViewGO = GameObject.Instantiate(this.loader.RecipesView, viewRoot);
        recipesView = recipesViewGO.AddComponent<RecipeListUI>();
        recipesView.Constructor(this.loader, recipesManager, player.outfitManager, contextMenu, messageDialogue);

        recipesButton = uiObject.transform.Find(recipesButtonAddress).GetComponent<Button>();
        recipesButton.onClick.AddListener(OnRecipeButton);

        materialsViewGO = GameObject.Instantiate(this.loader.MaterialsView, viewRoot);
        materialsView = materialsViewGO.AddComponent<MaterialsListUI>();
        materialsView.Constructor(this.loader, materialManager, contextMenu);

        materialsButton = uiObject.transform.Find(materialsButtonAddress).GetComponent<Button>();
        materialsButton.onClick.AddListener(OnMaterialsButton);

        settingsViewGO = GameObject.Instantiate(this.loader.SettingsView, viewRoot);
        settingsUI = settingsViewGO.AddComponent<SettingsUI>();
        settingsUI.Constructor();

        settingsButton = uiObject.transform.Find(settingsButtonAddress).GetComponent<Button>();
        settingsButton.onClick.AddListener(OnSettingsButton);

        canvas = uiObject.GetComponent<Canvas>();
        canvas.enabled = true;

        OnOutfitButton();

        var versionText = uiObject.transform.Find(versionAddress).GetComponent<Text>();
        versionText.text = PluginInfo.PLUGIN_VERSION.ToString();
    }

    public void Show() => canvas.enabled = true;
    //TODO: Animate ui opening/closing

    public void Hide() => canvas.enabled = false;
    #endregion

    private void OnOutfitButton()
    {
        HideViews();
        LowlightButtons();
        outfitsButton.GetComponent<Image>().color = Constants.Highlight;
        outfitViewGO.SetActive(true);
    }

    private void OnRecipeButton()
    {
        HideViews();
        LowlightButtons();
        recipesButton.GetComponent<Image>().color = Constants.Highlight;
        recipesViewGO.SetActive(true);
    }

    private void OnMaterialsButton()
    {
        HideViews();
        LowlightButtons();
        materialsButton.GetComponent<Image>().color = Constants.Highlight;
        materialsViewGO.SetActive(true);
    }

    private void OnSettingsButton()
    {
        HideViews();
        LowlightButtons();
        settingsButton.GetComponent<Image>().color = Constants.Highlight;
        settingsViewGO.SetActive(true);
    }

    private void HideViews()
    {
        outfitViewGO.SetActive(false);
        settingsViewGO.SetActive(false);
        recipesViewGO.SetActive(false);
        materialsViewGO.SetActive(false);
    }

    private void LowlightButtons()
    {
        outfitsButton.GetComponent<Image>().color = Constants.DefaultColor;
        settingsButton.GetComponent<Image>().color = Constants.DefaultColor;
        recipesButton.GetComponent<Image>().color = Constants.DefaultColor;
        materialsButton.GetComponent<Image>().color = Constants.DefaultColor;
    }
}
