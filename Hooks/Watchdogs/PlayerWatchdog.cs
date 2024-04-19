using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CarolCustomizer.Utils;
using System.Collections;
using System.Net.Sockets;
using Rewired;
using CarolCustomizer.Models.Outfits;
using CarolCustomizer.Behaviors.Settings;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PlayerWatchdog : PelvisWatchdog
{
    private const float LockSpeed = 1.00f;
    private const float UnlockSpeed = 1.75f;

    Entity carolEntity;
    CarolController carolController;

    public bool Busy { get; private set; } = false;
    public bool Locked { get; private set; } = false;
    public bool isGrounded => carolController.onGround;
    public bool isSwimming => carolController.swim.swimming;
    public bool controllerDisabled => !carolController.enabled;
    public bool inDialogue => carolEntity.hud.dialogue.dialogue.activeSelf;

    public override PelvisWatchdog BuildFromExisting(PelvisWatchdog watchdog, Component typeComponent)
    {
        carolEntity = typeComponent as Entity;
        carolController = gameObject.GetComponentInParent<CarolController>();
        if (!carolController) { Log.Error($"Failed to find {this}'s CarolController"); }
        return base.BuildFromExisting(watchdog, typeComponent);
    }

    void OnEnable()
    {
        CCPlugin.cutscenePlayer.NotifySpawned(this);
        if (Locked) LockPlayer(0.01f);
    }

    public void LockPlayer(float initialDelay = 0) => StartCoroutine(LockRoutine(initialDelay));

    public void UnlockPlayer() => StartCoroutine(UnlockRoutine());

    public override void SetBaseOutfit(Outfit outfit)
    {
        Log.Debug("Changing outfit via Entity.Swapmodel");
        carolEntity.SwapModel(outfit.storedAsset.gameObject);
    }

    private IEnumerator LockRoutine(float initialDelay = 0f)
    {
        float speed = LockSpeed * Settings.Plugin.MenuSpeed;
        Log.Debug($"Locking player, speed: {speed}");
        Busy = true;
        Locked = true;
        if (initialDelay > 0) yield return new WaitForSeconds(initialDelay);

        var inventory = carolEntity.GetComponent<Inventory>();
        carolEntity.rigidbody.velocity = Vector3.zero;
        carolEntity.LockMove(float.PositiveInfinity);
        carolEntity.LockAttack(float.PositiveInfinity);
        carolEntity.AddInvulnerableTime(float.PositiveInfinity);
        carolEntity.inventory.DrawWeapon(null);
        carolEntity.inventory.HideHeldWeapon(false);
        carolEntity.anim.SetFloat("SpeedFront", 0f);
        carolEntity.anim.SetFloat("SpeedSides", 0f);
        inventory.phone.SetActive(true);
        carolEntity.anim.speed = speed;
        carolEntity.StartCoroutine(carolEntity.anim.SetTriggerForOneFrame("PhoneOut"));
        carolEntity.enabled = false;
        yield return new WaitForSeconds(Constants.PhoneHideTime / speed);

        carolEntity.anim.speed = 1;
        Busy = false;
        Log.Debug("Locked.");
        yield break;
    }

    private IEnumerator UnlockRoutine()
    {
        float speed = UnlockSpeed * Settings.Plugin.MenuSpeed;
        Log.Debug("Unlocking player");
        Busy = true;
        Locked = false;
        carolEntity.StartCoroutine(carolEntity.anim.SetTriggerForOneFrame("PhoneBack"));
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

    public bool CanOpenMenu()
    {
        if (!carolController)   return false;
        if (!isGrounded)        return false;
        if (isSwimming)         return false;
        if (controllerDisabled) return false;
        if (inDialogue)         return false;
        return true;
    }
}
