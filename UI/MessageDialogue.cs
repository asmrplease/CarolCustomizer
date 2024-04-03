using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CarolCustomizer.Utils;

namespace CarolCustomizer.UI;
public class MessageDialogue : MonoBehaviour
{
    const string defaultCancelMessage = "Cancel";

    private static readonly string messageAddress = "Text";
    private static readonly string confirmButtonAddress = "Buttons/Confirm";
    private static readonly string confirmTextAddress= "Buttons/Confirm/Text";
    private static readonly string cancelButtonAddress = "Buttons/Cancel";
    private static readonly string cancelTextAddress= "Buttons/Cancel/Text";

    Text message;
    Button confirmButton;
    Text confirmText;
    Button cancelButton;
    Text cancelText;

    public void Constructor()
    {
        this.message = this.transform.Find(messageAddress).GetComponent<Text>();
        
        this.confirmButton = this.transform.Find(confirmButtonAddress).GetComponent<Button>();
        this.confirmText = this.transform.Find(confirmTextAddress).GetComponent<Text>();

        this.cancelButton = this.transform.Find(cancelButtonAddress).GetComponent<Button>();
        this.cancelText = this.transform.Find(cancelTextAddress).GetComponent<Text>();

        this.confirmButton.onClick.AddListener(Close);
        this.cancelButton.onClick.AddListener(Close);
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
        if (cancelAction is not null) this.cancelButton.onClick.AddListener(cancelAction);

        if (confirmText != "" && confirmAction != null)
        {
            this.confirmText.text = confirmText;
            this.confirmButton.onClick.AddListener(confirmAction);
            this.confirmButton.gameObject.SetActive(true);
        } 
        else { confirmButton.gameObject.SetActive(false); }
        
        this.gameObject.SetActive(true);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);

        this.confirmButton.onClick.RemoveAllListeners();
        this.cancelButton.onClick.RemoveAllListeners();

        this.confirmButton.onClick.AddListener(Close);
        this.cancelButton.onClick.AddListener(Close);
    }
}
