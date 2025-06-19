using CarolCustomizer.Models.Recipes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Main;
public class FilenameDialogue : MonoBehaviour
{
    static readonly string confirmAddress = "Buttons/Confirm";
    static readonly string cancelAddress = "Buttons/Cancel";

    InputField textBox;
    Button confirm;
    Button cancel;
    RectTransform rect;

    LatestDescriptor recipe;
    UnityAction<LatestDescriptor, string> onConfirm;

    public FilenameDialogue Constructor()
    {
        textBox = GetComponentInChildren<InputField>(true);
        rect = GetComponent<RectTransform>();

        confirm = transform.Find(confirmAddress).GetComponent<Button>();
        confirm.onClick.AddListener(OnConfirm);

        cancel = transform.Find(cancelAddress).GetComponent<Button>();
        cancel.onClick.AddListener(OnCancel);

        gameObject.SetActive(false);
        return this;
    }

    public void Show(LatestDescriptor recipe, UnityAction<LatestDescriptor, string> onConfirm)
    {
        this.recipe = recipe;
        this.onConfirm = onConfirm;

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) gameObject.SetActive(false);
    }

    void OnConfirm()
    {
        if (textBox.text != "") onConfirm?.Invoke(recipe, textBox.text);
        gameObject.SetActive(false);
    }

    void OnCancel()
    {
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        textBox.text = "";
    }
}
