using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Utils;
internal class DebugOnKeypress : MonoBehaviour
{
    KeyCode keyCode;
    string message;
    Action<string> LogMethod;

    public void Constructor(KeyCode keyCode, string message, Action<string> logMethod)
    {
        this.keyCode = keyCode;
        this.message = message;
        this.LogMethod = logMethod;
    }

    void Update()
    {
        if (!Input.GetKeyDown(keyCode)) return;
        LogMethod?.Invoke(message);
    }
}
