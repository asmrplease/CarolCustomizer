using CarolCustomizer.Models.Recipes;
using CarolCustomizer.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Main;
public class FilenameDialogue : MonoBehaviour
{
    private static readonly string confirmAddress = "Buttons/Confirm";
    private static readonly string cancelAddress = "Buttons/Cancel";

    InputField textBox;
    Button confirm;
    Button cancel;
    RectTransform rect;

    RecipeDescriptor20 recipe;
    UnityAction<RecipeDescriptor20, string> onConfirm;

    public void Constructor()
    {
        Log.Debug("FileDialogue.Constructor()");
        textBox = GetComponentInChildren<InputField>(true);
        rect = GetComponent<RectTransform>();

        confirm = transform.Find(confirmAddress).GetComponent<Button>();
        confirm.onClick.AddListener(OnConfirm);

        cancel = transform.Find(cancelAddress).GetComponent<Button>();
        cancel.onClick.AddListener(OnCancel);

        gameObject.SetActive(false);
        Log.Debug("end constructor");
    }

    public void Show(RecipeDescriptor20 recipe, UnityAction<RecipeDescriptor20, string> onConfirm)
    {
        Log.Debug("FilenameDialogue.Show()");
        this.recipe = recipe;
        this.onConfirm = onConfirm;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) gameObject.SetActive(false);
    }

    private void OnConfirm()
    {
        if (textBox.text != "") onConfirm?.Invoke(recipe, textBox.text);
        gameObject.SetActive(false);
    }

    private void OnCancel()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        textBox.text = "";
    }
}
