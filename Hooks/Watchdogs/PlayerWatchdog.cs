using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CarolCustomizer.Utils;
using System.Collections;
using System.Net.Sockets;
using CarolCustomizer.Models;
using CarolCustomizer.Behaviors;
using Rewired;

namespace CarolCustomizer.Hooks.Watchdogs;
public class PlayerWatchdog : PelvisWatchdog
{
    Entity carolEntity;
    CarolController carolController;

    public bool Busy { get; private set; }
    public bool Locked { get; private set; }

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

    override public void Awake()
    {
        Log.Info("PlayerWatchdog Awake()");

        base.Awake();
        Locked = false;
        Busy = false;
    }

    void OnEnable()
    {
        //if (DetectType()) return;
        CCPlugin.cutscenePlayer.NotifySpawned(this);
        if (Locked) LockPlayer(0.01f);
    }

    protected override void OnTransformParentChanged()
    {
        Log.Info("PlayerWatchdog OnTransformParentChanged");
        base.OnTransformParentChanged();
    }

    public void LockPlayer(float initialDelay = 0) => StartCoroutine(LockRoutine(1, initialDelay));

    public void UnlockPlayer() => StartCoroutine(UnlockRoutine(1.75f));

    public override void SetBaseOutfit(Outfit outfit)
    {
        Log.Debug("Changing outfit via Entity.Swapmodel");
        carolEntity.SwapModel(outfit.storedAsset.gameObject);
    }

    private IEnumerator LockRoutine(float speed = 1, float initialDelay = 0f)
    {
        Log.Debug("Locking player");
        Busy = true;
        Locked = true;
        Log.Debug($"Waiting for {initialDelay} seconds");
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

    private IEnumerator UnlockRoutine(float speed = 1)
    {
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

    public bool ManagesPlayer(Entity playerEntity)
    {
        return carolEntity == playerEntity;
    }

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
