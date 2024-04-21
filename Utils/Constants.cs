using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CarolCustomizer.Utils;
internal static class Constants
{
    public static readonly List<string> freeOutfits = new List<string>
    {
        "CAROL_Bombsuit",
        "CAROL_Bionicle1",
        "CAROL_BombsuitShark",
        "CAROL_Heavymetal1",
        "CAROL_Heavymetal2",
        "CAROL_NinjaBlack",
        "CAROL_Ninjapink",
        "CAROL_Powersuit2",
        "CAROL_Powersuit3",
        "CAROL_Powersuit4",
        "CAROL_sweater1",
        "CAROL_sweater2",
        "CAROL_sweater3",
        "CAROL_sweater4",
        "CAROL_sweater5",
        "CAROL_sweater6",
        "CAROL_ToySoldier1",
        "CAROL_Yukata1",
        "CAROL_Xmasdress",
        "CAROL_Xmas2022",
        "CAROL_WitchStudent1",
        "CAROL_SummerBlast4",
        "CAROL_SummerBlast3",
        "CAROL_SummerBlast2",
        "CAROL_Summer1",
        "CAROL_Robot1",
        "CAROL_RetakeJacket",
        "CAROL_Hunt_League1",
        "CAROL_Ghostbuster",
        "CAROL_2021Halloween",
        "CAROL_Vampirehunter",
        "CAROL_Zombie"
    };
    public static readonly Color DefaultColor = new(0.1882353f, 0.0f, 0.3647059f, 1.0f);
    public static readonly Color Highlight = new(0.6f, 0.2039f, 0.7373f, 0.728f);//new(0.4641f, 0f, 0.4706f, 1f);
    public static readonly Color Vanilla = new(0, 0.1294f, 0.3647f, 1f);
    public static readonly Color Teal = new(0, 0.1961f, 0.1373f, 1f);
    public static readonly Dictionary<string, float> MenuSpeeds = new()
    {
        {"Slow", 0.66f},
        {"Normal", 1.0f},
        {"Fast", 2.0f}
    };

    public const string MenuSceneName = "Main_menu_new";
    public const string LoadingSceneName = "Loading_Startup";
    public const string IntroCutsceneName = "INTRO_CUTSCENES";
    public const string Pyjamas = "CAROL_PyjamaBasic";
    public const string RobotHead = "Robocarolhead";
    public const string Shezara = "Shezara";
    public const float CarolDefaultMass = 0.01f;
    public const KeyCode DefaultReload = KeyCode.Alpha8;

    public static readonly Version v210 = new("2.1.0");
    public static readonly Version v200 = new("2.0.0");
    public static readonly Version v100 = new("1.0.0");

    public const string RecipeExtension = ".json";
    public const string RecipeFolderName = "Recipes";
    public const string LogFolderRelativePath = "BepInEx";
    public const string Preferences = "Preferences";
    public static readonly string ApplicationPath = Directory.GetParent(Application.dataPath).FullName;
    public static readonly string RecipeFolderPath = Path.Combine(Constants.ApplicationPath, Constants.RecipeFolderName);
    public static readonly string BepInExFolderPath = Path.Combine(Constants.ApplicationPath, Constants.LogFolderRelativePath);
    public static readonly string LogFileName = "LogOutput.log";
    public static readonly string LogFilePath = Path.Combine(BepInExFolderPath, LogFileName);


    public static readonly Vector3 OutOfTheWay = new(-10000, -10000, -10000);
    public const float PhoneHideTime = 1.75f;
}
