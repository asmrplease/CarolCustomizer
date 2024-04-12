﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace CarolCustomizer.Contracts;
public interface IContextMenuActions
{
    public List<(string, UnityAction)> GetContextMenuItems();
}
