using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CarolCustomizer.Utils;
using CarolCustomizer.Models.Recipes;

namespace CarolCustomizer.UI;
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
        textBox = this.GetComponentInChildren<InputField>(true);
        rect = this.GetComponent<RectTransform>();

        confirm = this.transform.Find(confirmAddress).GetComponent<Button>();
        confirm.onClick.AddListener(OnConfirm);

        cancel = this.transform.Find(cancelAddress).GetComponent<Button>();
        cancel.onClick.AddListener(OnCancel);

        this.gameObject.SetActive(false);
        Log.Debug("end constructor");
    }

    public void Show(RecipeDescriptor20 recipe, UnityAction<RecipeDescriptor20, string> onConfirm)
    {
        Log.Debug("FilenameDialogue.Show()");
        this.recipe = recipe;
        this.onConfirm = onConfirm;

        this.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) gameObject.SetActive(false);
    }

    private void OnConfirm()
    {
        if (textBox.text != "") onConfirm?.Invoke(recipe, textBox.text);
        this.gameObject.SetActive(false);
    }

    private void OnCancel()
    {
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        textBox.text = "";
    }
}
