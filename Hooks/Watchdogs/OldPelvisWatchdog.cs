using CarolCustomizer.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CarolCustomizer.Models;
using CarolCustomizer.Utils;

namespace CarolCustomizer.Hooks.Watchdogs;

public class OldPelvisWatchdog : MonoBehaviour
{
    /*
    #region Watchdog Data
    private readonly Guid Guid = Guid.NewGuid();
    
    [SerializeField]
    private Component typeComponent;
    public Component TypeComponent => typeComponent;

    [SerializeField]
    private InstanceType instanceType;
    public InstanceType Type => instanceType;
    #endregion



    #region Name Data
    private string rootName => this.transform.root.name;
    private string grandparentName => this.transform.parent.parent.name;
    #endregion


    public override string ToString()
    {
        return $"{rootName}->{grandparentName}({this.Guid.ToString().TakeLast(5)})";
    }

    private void Awake()
    {
        Log.Debug($"{this}'s PelvisWatchdog.Awake()");
        if (rootName == "BepInEx_Manager") { Log.Debug("Skipping..."); return; }

        var coopToggles = this.transform.parent.GetComponentsInChildren<CoopModelToggle>(true);
        foreach (var toggle in coopToggles) { toggle.enabled = false; }

        var type = SetType();
        if (type == InstanceType.DedicatedActress)  { CCPlugin.cutscenePlayer?.NotifySpawned(this); return; }
        if (type == InstanceType.CampaignBot)       { NPCManager.OnBotSpawn(this); return; }
        
    }

    private void Start()
    {
        Log.Debug($"Watchdog {this} Start()");
        if (rootName == "BepInEx_Manager") { Log.Debug("Skipping..."); return; }
        if (this.instanceType == InstanceType.MainMenu) { base.StartCoroutine(MainMenuFix()); }
    }

    private void OnEnable()
    {
        Log.Debug($"{this}'s PelvisWatchdog.OnEnable()");
        if (rootName == "BepInEx_Manager") { Log.Debug("Skipping..."); return; }
        SetType();
        if (instanceType == InstanceType.Player) { NotifyPlayer((Entity)this.typeComponent); return; }
        if (instanceType == InstanceType.MultiplayerBot) { NPCManager.OnBotSpawn(this); return; };
        if (instanceType == InstanceType.ShezaraActress) { NPCManager.OnShezaraAwake(this); return; }
    }
    //this appears to be where we're most likely to figure out what type we've been instantiated as.
    private void OnTransformParentChanged()
    {
        Log.Debug($"{this}'s PelvisWatchdog.OnTransformParentChanged()");
        if (rootName == "BepInEx_Manager") { Log.Debug("Skipping..."); return; }

        var type = SetType();
        if (type == InstanceType.Player) { Log.Info("Watchdog->Player"); NotifyPlayer((Entity)this.typeComponent); }
        if (type == InstanceType.MultiplayerBot) { NPCManager.OnBotSpawn(this); return; };
        if (type == InstanceType.Actress) { Log.Info("Watchdog->Actress"); CCPlugin.cutscenePlayer?.NotifySpawned(this); }
        if (type == InstanceType.MainMenu) { Log.Info("Watchdog->MainMenu"); CCPlugin.cutscenePlayer?.NotifySpawned(this); }
    }

    private void OnDisable()
    {
        if (rootName == "BepInEx_Manager") return;
        Log.Debug($"{this}'s PelvisWatchdog.OnDisable()");
        if (instanceType == InstanceType.DedicatedActress) { CCPlugin.cutscenePlayer?.RestorePrevious(); }
        if (instanceType == InstanceType.ShezaraActress) { NPCManager.shezaraInstance?.RestorePrevious(); }
    }

    private void OnDestroy()
    {
        Log.Debug($"{this}'s PelvisWatchdog.OnDestroy()");
        if (rootName == "BepInEx_Manager") { Log.Debug("Skipping..."); return; }
        var coopToggles = this.transform.parent.GetComponentsInChildren<CoopModelToggle>(true);
        foreach (var coopToggle in coopToggles) { coopToggle.enabled = true; }

        if (this.instanceType == InstanceType.CampaignBot) { NPCManager.OnBotDespawn(this); }
        if (this.instanceType == InstanceType.MultiplayerBot) { NPCManager.OnBotDespawn(this); }
    }

    private void NotifyPlayer(Entity entity)
    {
        int playerIndex = Entity.players.IndexOf(entity);
        if (playerIndex == -1) { Log.Error("Tried to notify player but no matching player entity was found"); return; }

        var playerCount = CCPlugin.playerManagers.Count;
        if (playerIndex > playerCount) { Log.Warning($"player {playerIndex} spawned but we only have {playerCount} playerManagers."); return; }

        Log.Debug($"Notifying player {playerIndex} of spawn.");
        //CCPlugin.playerManagers[playerIndex].NotifySpawned(this);
    }


    /// <summary>
    /// Tries to change type of this watchdog.
    /// </summary>
    /// <param name="inputType">The desired type, autodetect when blank.</param>
    /// <returns>The type this was changed to, or unknown if not changed.</returns>
    public InstanceType SetType(InstanceType inputType = InstanceType.Unknown) 
    {
        //if a type is not specified, detect it.
        if (inputType == InstanceType.Unknown ) { inputType = DetectType(); }
        
        //if types have not changed.
        if (this.instanceType == inputType) return InstanceType.Unknown;

        //if type has changed to unknown
        if (inputType == InstanceType.Unknown) return InstanceType.Unknown;

        //if type has changed to a new known type
        this.instanceType = inputType;
        return instanceType;
    }

    private InstanceType DetectType()
    {
        Log.Debug($"Detecting type...");

        var virtualCarol = this.transform.GetComponentInParent<VirtualCarol>();
        if (virtualCarol) 
        {
            Log.Debug("I think this is a multiplayer bot");
            this.typeComponent = virtualCarol;
            return InstanceType.MultiplayerBot;
        }

        var entity = this.transform.GetComponentInParent<Entity>(true);
        if (entity) 
        {
            this.typeComponent = entity;
            if (this.rootName != "CAROL(Clone)")
            {
                Log.Debug("I think this is a bot");
                return InstanceType.CampaignBot;
            }
            
            Log.Debug("I think this is a player");
            return InstanceType.Player;
        }

        var cutsceneActor = this.transform.GetComponentInParent<CutsceneActor>(true);
        if (cutsceneActor) 
        {
            this.typeComponent = cutsceneActor;
            Log.Debug("I think this is an standard actress"); 
            return InstanceType.Actress; 
        }

        var slateActor = this.transform.GetComponentInParent<Slate.Character>(true);
        if (slateActor) 
        { 
            this.typeComponent = slateActor;

            if (this.transform.parent.name == "Carol_Pirate")
            {
                Log.Debug("i think this is a shezara actress");
                return InstanceType.ShezaraActress;
            }

            Log.Debug("i think this is a dedicated actress"); 
            return InstanceType.DedicatedActress; 
        }

        var menuSwitchOutfit = this.transform.GetComponentInParent<MenuSwitchOutfit>(true);
        if (menuSwitchOutfit) 
        {
            //menuSwitchOutfit.enabled = false;
            this.typeComponent = menuSwitchOutfit;
            Log.Debug("I think this is the main menu actress."); 
            return InstanceType.MainMenu; 
        }
        
        if (this.transform.parent.name == "Carol_Pirate")
        {
            Log.Debug("i think this is a shezara actress");
            return InstanceType.ShezaraActress;
        }

        if (this.transform.root.name == "CUTSCENES")
        {
            Log.Debug("i think this is an actress with no component");
            return InstanceType.DedicatedActress;
        }

        return InstanceType.Unknown;
    }

    public enum InstanceType
    {
        Unknown,
        Reference,
        Player,
        Actress,
        MainMenu,
        DedicatedActress,
        ShezaraActress,
        CampaignBot,
        MultiplayerBot
    }
    */
}
