using BepInEx.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace CarolCustomizer.Utils;

/// <summary>
/// Global instance for logging to make message creation simple.
/// </summary>
internal class Log
{
    #region Static Instance
    private static ManualLogSource i;
    private static Dictionary<string, float> Timers = new();
    public Log(ManualLogSource manualLogSource) { i = manualLogSource; }
    #endregion

    #region Static Logger Methods
    public static void Debug(string message) => i.LogDebug(message);
    public static void Info(string message) => i.LogInfo(message);
    public static void Message(string message) => i.LogMessage(message);
    public static void Warning(string message) => i.LogWarning(message);
    public static void Error(string message) => i.LogError(message);    
    public static void Fatal(string message) => i.LogFatal(message);
    #endregion

    #region Time Logging
    public static void TimerStart(string timerName, Action<string> logType = null)
    {
        Timers[timerName] = Time.realtimeSinceStartup;
        logType?.Invoke($"Started timer {timerName}");
    }

    public static float TimerEnd(string timerName, string message = "", Action<string> logType = null)
    {
        if (!Timers.ContainsKey(timerName)) {Log.Warning($"Timer {timerName} was ended before it was started."); return -1; }
        float result = Time.time - Timers[timerName];
        logType ??= Log.Debug;
        logType?.Invoke($"Timer {timerName} {message} completed in {result} seconds.");
        return result;
    }
    #endregion
}
