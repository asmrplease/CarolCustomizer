global using ContextButton = (string label, System.Action action);//UnityEngine.Events.UnityAction action);
using CarolCustomizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;


namespace CarolCustomizer.Contracts;
public interface IContextMenuActions
{
    public List<ContextButton> GetContextMenuItems();
}

public static class ContextMenuHelper
{
    public static List<ContextButton> AutoMenuItems(this IContextMenuActions target)
    {
        Log.Debug("IContextMenuActions.Auto()");
        var idk = target
            .GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Select(m => (method: m, attr: m.GetCustomAttribute<MenuItemAttribute>()))
            .Where(tup => tup.attr is not null);
        //idk.ForEach(tup => Log.Debug(tup.attr.Label));

        return idk
            .Select(tup => (tup.attr.Label, (Action) Delegate.CreateDelegate(typeof(Action), target, tup.method.Name)))
            .ToList();
    }

    public static void Test(this IContextMenuActions target)
    {
        Log.Info("IContextMenuActions.Test()");
        var menu = target.GetContextMenuItems();
        Log.Debug(menu.Count().ToString());
        foreach (var tup in menu)
        {
            Log.Debug(tup.Item1);
            tup.action.Invoke();
        }
            
    }
}


[AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class MenuItemAttribute : Attribute
{
    public string Label { get;}
    public MenuItemAttribute(string label)
    {
        this.Label = label;
    }
}