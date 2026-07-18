using CarolCustomizer.Models.Outfits;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CarolCustomizer.Utils;
public static class Constants
{
    public static readonly List<string> freeOutfits =
    [
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
    ];
    public static readonly Color DefaultColor = new(0.1882353f, 0.0f, 0.3647059f, 1.0f);
    public static readonly Color Highlight = new(0.6f, 0.2039f, 0.7373f, 0.728f);
    public static readonly Color Vanilla = new(0, 0.1294f, 0.3647f, 1f);
    public static readonly Color Teal = new(0, 0.1961f, 0.1373f, 1f);
    public static readonly Dictionary<string, float> MenuSpeeds = new()
    {
        {"Slow", 0.66f},
        {"Normal", 1.0f},
        {"Fast", 2.0f}
    };

    public const string MenuSceneName = "SYS - Main Menu";
    public const string LoadingSceneName = "SYS - Loading Zone";
    public const string NetworkSceneName = "SYS - Network Manager";
    public const string IntroCutsceneName = "ADV - Intro Cutscene";
    public const string Pyjamas = "CAROL_PyjamaBasic";
    public const string RobotHead = "Robocarolhead";
    public const string Shezara = "Shezara";
    public const string HeadBone = "Bn_CarolHead";
    public const string Pelvis = "CarolPelvis";
    public const float CarolDefaultMass = 0.01f;
    public const KeyCode DefaultReload = KeyCode.F8;
    public const int SMRLayer = 8;

    public static readonly Version v250 = new("2.5.0");
    public static readonly Version v240 = new("2.4.0");
    public static readonly Version v230 = new("2.3.0");
    public static readonly Version v220 = new("2.2.0");
    public static readonly Version v210 = new("2.1.0");
    public static readonly Version v200 = new("2.0.0");
    public static readonly Version v100 = new("1.0.0");
    
    public const string JsonFileExtension = ".json";
    public const string PngFileExtension = ".png";
    public const string RecipeFolderName = "Recipes";
    public const string BepInExFolder = "BepInEx";
    public const string PluginsFolder = "plugins";
    public const string UIAssetName = "tabui.ui";
    public const string LogFileName = "LogOutput.log";
    public const string AssetFolderName = "CarolCustomizer";
    public const string Preferences = "Preferences";
    public const string AutoSave = "AutoSave";
    public const string PNGChunkKeyword = "RecipeData";
    public const string HairstyleSourceName = "Hairstyles";
    public const string HairDyeSourceName = "HairDye";

    public static readonly string ApplicationPath = Directory.GetParent(Application.dataPath).FullName;
    public static readonly string RecipeFolderPath = Path.Combine(ApplicationPath, RecipeFolderName);
    public static readonly string BepInExFolderPath = Path.Combine(ApplicationPath, BepInExFolder);
    public static readonly string PluginsFolderPath = Path.Combine(BepInExFolderPath, PluginsFolder);
    public static readonly string AssetFolderPath = Path.Combine(PluginsFolderPath, AssetFolderName);
    public static readonly string UIAssetPath = Path.Combine(AssetFolderPath, UIAssetName);
    public static readonly string LogFilePath = Path.Combine(BepInExFolderPath, LogFileName);
    
    public static readonly Vector3 OutOfTheWay = new(-10000, -10000, -10000);
    public static readonly SourceDescriptor PyjamaDescriptor = new(Constants.Pyjamas, Models.SourceType.Outfit);
    public const float PhoneHideTime = 1.75f;
    public const int MaxCoopPlayers = 4;
    public const int ThumbnailSize = 1024;

    public static IReadOnlyDictionary<string, string> LegacySceneNames = new Dictionary<string, string> 
    {
        {"Carnival_of_shadows", "TAL - Carnival of shadows" }, 
        {"Caverns", "ADV - Caverns" },
        {"Christmas_2019_Event", "TAL - Cruise of light" },
        {"Christmas_2022_Event", "TAL - A Christmas Carol" },
        {"ChristmasArena", "ADV - ChristmasArena" },
        {"Corpopolis", "ADV - Corpopolis" },
        {"Corpotower", "ADV - Corpotower" },
        {"Corpozombies_Shoreofnacre", "TAL - Shores of Nacre" },
        {"forestbonus1", "BNS - Chroma falls Bonus 1" },
        {"forestbonus2", "BNS - Chroma falls Bonus 2" },
        {"forestbonus3", "BNS - Chroma falls Bonus 3" },
        {"Greenhouse", "ADV - Greenhouse" },
        {"Halloween_2019", "TAL - Catastrophe of the Moon" },
        {"Halloween_2021", "TAL - Middle school of the dead" },
        {"Halloween_2022", "TAL - Academy of Evil" },
        {"Halloween_2023", "TAL - High Tension" },
        {"Halloween_Area_15", "TAL - Area 15" },
        {"INTRO_CUTSCENES", "ADV - Intro Cutscene" },
        {"LoadingZone", LoadingSceneName },
        {"Main_menu_new", MenuSceneName },
        {"Marshmalotts_Celestialforge", "ADV - Celestial Forge" },
        {"Marshmalotts_Synistrus", "ADV - Cursed Forest" },
        {"Marshmalotts", "ADV - Marshmalotts" },
        {"Meadows", "ADV - Meadows" },
        {"Moonbase", "ADV - Moonbase" },
        {"NetworkManager",  NetworkSceneName},
        {"NEWForest", "ADV - Chroma Falls" },
        {"Newyear_event_2021", "TAL - Hikari No Saiten" },
        {"Pinewood_Bonus_1", "BNS - Pinewood Bonus 1" },
        {"Pinewood_Bonus_2", "BNS - Pinewood Bonus 2" },
        {"Pinewood_Bonus_3", "BNS - Pinewood Bonus 3" },
        {"Pinewood_Event", "TAL - Fallen Peaks" },
        {"Popcicle_peaks_AlpineEscape", "ADV - Alpine Escape" },
        {"Popcicle_peaks_battleground", "ADV - Battlefield" },
        {"Popcicle_peaks_Factory", "ADV - Ullr Factory" },
        {"Popcicle_peaks_Glacier", "ADV - Glacier" },
        {"Popcicle_peaks_mountain", "ADV - Pinewood" },
        {"Popcicle_peaks_Polarsea", "ADV - Borealis Sea" },
        {"Rottenlake_Districtoflights", "ADV - Rottenlake Districtoflights" },
        {"Rottenlake_FishboneHarbor", "ADV - Rottenlake FishboneHarbor" },
        {"Rottenlake_OrcaAvenue", "ADV - Rottenlake OrcaAvenue" },
        {"Rottenlake_Slights", "ADV - Rottenlake Slights" },
        {"Sandykingdoms_Orienttown", "ADV - AlHibdae" },
        {"Sandykingdoms_Rustycanyon", "ADV - Rusty Canyon" },
        {"Sandykingdoms_Train", "ADV - Train" },
        {"Sandykingdoms_Valleyofthekings", "ADV - Valley of the kings" },
        {"Spacestation", "ADV - Spacestation" },
        {"Summer_slime_blast_event", "TAL - SSB1" },
        {"SummerBlast2_Event", "TAL - SSB2" },
        {"SummerBlast3_Event", "TAL - SSB3" },
        {"SummerBlast4_Event", "TAL - SSB4" },
        {"Sunshine_Resort", "ADV - Sunshine Resort" },
        {"SV_Canyon", "HNT - SV_Canyon" },
        {"SV_Compound", "HNT - SV_Compound" },
        {"SV_Glacier", "HNT - SV_Glacier" },
        {"SV_Greenhouse", "HNT - SV_Greenhouse" },
        {"SV_Marina", "HNT - SV_Marina" },
        {"SV_RooftopNewyear", "HNT - SV_RooftopNewyear" },
        {"SV_Sewers", "HNT - SV_Sewers" },
        {"SV_Shangrila", "HNT - SV_Shangrila" },
        {"SV_SUNSETTEMPLE", "HNT - SV_SUNSETTEMPLE" },
        {"Sv_Tutorialhunt", "HNT - Sv_Tutorialhunt" },
        {"SV_Volcano", "HNT - SV_Volcano" },
        {"Swamps_Bonus_1", "BNS - Swamps Bonus 1" },
        {"Swamps_Bonus_2", "BNS - Swamps Bonus 2" },
        {"Swamps_RaceTrack", "BNS - Swamps RaceTrack" },
        {"Swamps", "ADV - Swamps" },
        {"Tales_Tutorial", "TAL - Tales_Tutorial" },
        {"Treetops_bonus_1", "BNS - Treetops Bonus 1" },
        {"Treetops_bonus_2", "BNS - Treetops Bonus 2" },
        {"Treetops", "ADV - Treetops" },
        {"volcano", "ADV - Volcano" },
        {"volcanobonus1", "BNS - Volcano Bonus 1" },
        {"volcanobonus2", "BNS - Volcano Bonus 2" },
        {"VS_Atlantis", "PLG - VS_Atlantis" },
        {"VS_Ballpit", "PLG - VS_Ballpit" },
        {"VS_Cathedral", "PLG - VS_Cathedral" },
        {"VS_FacingSpires", "PLG - VS_FacingSpires" },
        {"VS_Forge", "PLG - VS_Forge" },
        {"VS_Gulch", "PLG - VS_Gulch" },
        {"VS_Harbor", "PLG - VS_Harbor" },
        {"VS_Hyperspace", "PLG - VS_Hyperspace" },
        {"VS_Mines", "PLG - VS_Mines" },
        {"VS_Nightclub", "PLG - VS_Nightclub" },
        {"VS_Nuke", "PLG - VS_Nuke" },
        {"VS_Onsen", "PLG - VS_Onsen" },
        {"VS_Rooftops", "PLG - VS_Rooftops" },
        {"VS_School", "PLG - VS_School" },
        {"VS_Sectors", "PLG - VS_Sectors" },
        {"VS_Shibuya", "PLG - VS_Shibuya" },
        {"VS_Skyrail", "PLG - VS_Skyrail" },
        {"VS_Spaceport", "PLG - VS_Spaceport" },
        {"VS_Tomb", "PLG - VS_Tomb" },
        {"VS_Waterpark", "PLG - VS_Waterpark" },
        {"VS_Woods", "PLG - VS_Woods" },
    };
}