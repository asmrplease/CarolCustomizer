using CarolCustomizer.Behaviors.Carol;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs;
public class ArmatureMonitor : MonoBehaviour
{
    public static event Action<Transform> On;

    static Dictionary<string, CarolInstanceType> ParentNameDict = 
    [

    ];

    void UpdateStatus()
    {

    }

    public void Awake()             => UpdateStatus();
    public void OnEnable()          => UpdateStatus();
    void OnDisable()                => UpdateStatus();
    void OnTransformParentChanged() => UpdateStatus();

}
