using System.Collections.Generic;
using UnityEngine.Events;

namespace CarolCustomizer.Contracts;
public interface IContextMenuActions
{
    public List<(string, UnityAction)> GetContextMenuItems();
}
