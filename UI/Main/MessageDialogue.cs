using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CarolCustomizer.Utils;

namespace CarolCustomizer.UI.Main;
public class MessageDialogue : MonoBehaviour
{
    const string defaultCancelMessage = "Cancel";

    private static readonly string messageAddress = "Text";
    private static readonly string confirmButtonAddress = "Buttons/Confirm";
    private static readonly string confirmTextAddress = "Buttons/Confirm/Text";
    private static readonly string cancelButtonAddress = "Buttons/Cancel";
    private static readonly string cancelTextAddress = "Buttons/Cancel/Text";

    Text message;
    Button confirmButton;
    Text confirmText;
    Button cancelButton;
    Text cancelText;

    public void Constructor()
    {
        message = transform.Find(messageAddress).GetComponent<Text>();

        confirmButton = transform.Find(confirmButtonAddress).GetComponent<Button>();
        confirmText = transform.Find(confirmTextAddress).GetComponent<Text>();

        cancelButton = transform.Find(cancelButtonAddress).GetComponent<Button>();
        cancelText = transform.Find(cancelTextAddress).GetComponent<Text>();

        confirmButton.onClick.AddListener(Close);
        cancelButton.onClick.AddListener(Close);
    }

    public void Show
    (
        string message,
        string confirmText = "", UnityAction confirmAction = null,
        string cancelText = defaultCancelMessage, UnityAction cancelAction = null
    )
    {
        this.message.text = message;

        this.cancelText.text = cancelText;
        if (cancelAction is not null) cancelButton.onClick.AddListener(cancelAction);

        if (confirmText != "" && confirmAction != null)
        {
            this.confirmText.text = confirmText;
            confirmButton.onClick.AddListener(confirmAction);
            confirmButton.gameObject.SetActive(true);
        }
        else { confirmButton.gameObject.SetActive(false); }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);

        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(Close);
        cancelButton.onClick.AddListener(Close);
    }
}
