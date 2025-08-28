using BepInEx.Logging;
using System.Runtime.CompilerServices;

namespace CarolCustomizer.Utils;

/// <summary>
/// Global instance for logging to make message creation simple.
/// </summary>
internal class Log
{
    #region Static Instance
    private static ManualLogSource i;
    public Log(ManualLogSource manualLogSource) { i = manualLogSource; }
    #endregion

    #region Static Logger Methods
    public static void Debug(string message) => i.LogDebug(message);
    public static void Info(string message) => i.LogInfo(message);
    public static void Message(string message) => i.LogMessage(message);
    public static void Warning(string message) => i.LogWarning(message);
    public static void Error(string message) => i.LogError(message);    
    public static void Fatal(string message) => i.LogFatal(message);

    public static bool WarnOnNull(object obj, [CallerMemberName] string idk = "") 
    {
        if (obj is null) { Warning($"In {idk}, {nameof(obj)} was null."); return true; }
        return false;
    }

    public static bool WarnOnCondition(bool condition, [CallerMemberName] string idk = "")
    {
        if (condition) { Warning($"In {idk}, {nameof(condition)} occurred."); return true; }
        return false;
    }
    #endregion
}
