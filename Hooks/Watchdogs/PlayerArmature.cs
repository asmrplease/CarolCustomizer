using UnityEngine;
using CarolCustomizer.Utils;
using System.Collections;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Behaviors.Settings;
using CarolCustomizer.Behaviors.Carol;
using CarolCustomizer.Contracts;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PlayerArmature : MonoBehaviour, ICarolType
{
    private const float LockSpeed = 1.00f;
    private const float UnlockSpeed = 1.75f;

    CarolController carolController;
    ModelData playerModelData;
    public PelvisWatchdog watchdog { get; private set; }
    public Entity carolEntity { get; private set; }

    public bool Busy { get; private set; } = false;
    public bool Locked { get; private set; } = false;
    public bool isGrounded => carolController.onGround;
    public bool isSwimming => carolController.swim.swimming;
    public bool controllerDisabled => !carolController.enabled;
    public bool inDialogue => carolEntity.hud.dialogue.dialogue.activeSelf;

    public ICarolType Constructor(PelvisWatchdog watchdog)
    {
        this.watchdog ??= watchdog;
        carolController = this.gameObject.GetComponentInParent<CarolController>();
        playerModelData = this.gameObject.GetComponentInParent<ModelData>();
        if (!carolController) Log.Error("Failed to find CarolController in PlayerModBehavior.Constructor()");
        if (!playerModelData) Log.Error("Failed to find ModelData on player armature");
        if (playerModelData) { playerModelData.hairIsVisible = false; }
        return this;
    }

    void OnEnable()
    {
        this.watchdog = GetComponent<PelvisWatchdog>();
        this.watchdog.Behavior = this;
        carolController = this.gameObject.GetComponentInParent<CarolController>();
        watchdog.CompData.SetBaseVisibility(false);
        if (!carolEntity) carolEntity = GetComponentInParent<Entity>();
        if (!carolEntity)
        {
            Log.Error("Failed to find Entity during PlayerWatchdog.OnEnable()!");
            PlayerInstances.DefaultPlayer.NotifySpawned(watchdog);
            return;
        }
        PlayerInstances.OnPlayerSpawn(this);
        if (Locked) LockPlayer(0.01f);
    }

    public void LockPlayer(float initialDelay = 0) => StartCoroutine(LockRoutine(initialDelay));

    public void UnlockPlayer() => StartCoroutine(UnlockRoutine());

    internal void SetAnimation(string animationName)
    {
        StartCoroutine(carolEntity.anim.SetTriggerForOneFrame(animationName));
    }

    public void SetBaseOutfit(Outfit outfit)
    {
        Log.Debug("Changing outfit via Entity.Swapmodel");
        //TODO: Fix 1.0
        //carolEntity.SwapModel(outfit.storedAsset.gameObject);
    }

    IEnumerator LockRoutine(float initialDelay = 0f)
    {
        float speed = LockSpeed * Settings.Plugin.MenuSpeed;
        Log.Debug($"Locking player, speed: {speed}");
        Busy = true;
        Locked = true;
        if (initialDelay > 0) yield return new WaitForSeconds(initialDelay);

        var inventory = carolEntity.GetComponent<Inventory>();
        carolEntity.rigidbody.linearVelocity = Vector3.zero;
        carolEntity.LockMove(float.PositiveInfinity);
        carolEntity.LockAttack(float.PositiveInfinity);
        carolEntity.AddInvulnerableTime(float.PositiveInfinity);
        carolEntity.inventory.DrawWeapon(null);
        carolEntity.inventory.HideHeldWeapon(false);
        carolEntity.anim.SetFloat("SpeedFront", 0f);
        carolEntity.anim.SetFloat("SpeedSides", 0f);
        inventory.phone.SetActive(true);
        carolEntity.anim.speed = speed;
        SetAnimation("PhoneOut");
        carolEntity.enabled = false;
        yield return new WaitForSeconds(Constants.PhoneHideTime / speed);

        carolEntity.anim.speed = 1;
        Busy = false;
        Log.Debug("Locked.");
        yield break;
    }

    internal IEnumerator UnlockRoutine()
    {
        float speed = UnlockSpeed * Settings.Plugin.MenuSpeed;
        Log.Debug("Unlocking player");
        Busy = true;
        Locked = false;
        SetAnimation("PhoneBack");
        carolEntity.anim.speed = speed;
        yield return new WaitForSeconds(Constants.PhoneHideTime / speed);

        var inventory = carolEntity.GetComponent<Inventory>();
        inventory.phone.SetActive(false);
        carolEntity.anim.speed = 1;
        carolEntity.enabled = true;
        carolEntity.UnlockAttack();
        carolEntity.UnlockMove();
        carolEntity.MakeVulnerable();
        Busy = false;
        Log.Debug("Unlocked.");
        yield break;
    }

    public bool ManagesPlayer(Entity playerEntity) => carolEntity == playerEntity;

    public void SetAnimator(Outfit outfit)
    {
        if (watchdog.AnimData?.Animator is null) { Log.Warning($"null animator when trying to set animator on {this}"); return; }
        Log.Debug($"Setting animator from {outfit.DisplayName}");
        var rac = outfit?.RuntimeAnimator;
        if (!rac) { Log.Warning("failed to get animator from outfit"); return; }
        
        watchdog.AnimData.SetAnimator(rac);
    }

    public void SetHeightOffset(float height)
    {
        Log.Debug("SetHeightOffset");
        var prefabRoot = transform.parent;
        if (!prefabRoot) { Log.Warning("prefabRoot was null in SetHeightOffset"); return; }

        Log.Debug($"SetHeightOffset() {prefabRoot.name}");
        prefabRoot.localPosition = Vector3.up * height;
    }

    public bool CanOpenMenu()
    {
        if (Busy)               return false;
        if (!carolController)   return false;
        if (!isGrounded)        return false;
        if (isSwimming)         return false;
        if (controllerDisabled) return false;
        if (inDialogue)         return false;
        return true;
    }

    void OnDisable()
    {
        Log.Debug($"{this}.OnDisable()");
    }

    public void SetBaseVisibility(bool visibility) 
    {
        watchdog.CompData.SetBaseVisibility(visibility);
        playerModelData.hairIsVisible = visibility;
        var inventory = GetComponentInParent<Inventory>();
        if (!inventory) return;

        inventory.currentHairstyle.GetComponent<Hairstyle>().Hide(visibility);
    } 

    public void Dispose()
    {
        //Destroy(this);
    }
}