using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CarolCustomizer.Behaviors;
public class HotKeyConfig
{
    public KeyCode MenuToggleMouseKey { get; private set; }
    public KeyCode MenuToggleKeyboardKey { get; private set; }
    public PointerEventData.InputButton ContextMenuMouseButton { get; private set; }

    public HotKeyConfig(KeyCode menuToggleMouse, KeyCode menuToggleKeyboard, PointerEventData.InputButton contextMenu )
    {
        this.MenuToggleMouseKey = menuToggleMouse;
        this.MenuToggleKeyboardKey = menuToggleKeyboard;
        this.ContextMenuMouseButton = contextMenu;
    }
}
