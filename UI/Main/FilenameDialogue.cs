using CarolCustomizer.Assets;
using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.UI.Outfits;
using CarolCustomizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CarolCustomizer.UI.Main;
public class FilenameDialogue : MonoBehaviour
{
    static readonly string confirmAddress = "Buttons/Confirm";
    static readonly string cancelAddress = "Buttons/Cancel";
    static readonly string dropdownAddress = "Pose Select";

    InputField textBox;
    Dropdown dropdown;
    Button confirm;
    Button cancel;
    RectTransform rect;

    RecipeDescriptor recipe;
    UnityAction<RecipeDescriptor, string> onConfirm;

    public FilenameDialogue Constructor()
    {
        Log.Debug("FilenameDialogue.Constructor");
        textBox = GetComponentInChildren<InputField>(true);
        rect = GetComponent<RectTransform>();

        confirm = transform.Find(confirmAddress).GetComponent<Button>();
        confirm.onClick.AddListener(OnConfirm);
        cancel = transform.Find(cancelAddress).GetComponent<Button>();
        cancel.onClick.AddListener(OnCancel);

        dropdown = transform.Find(dropdownAddress).GetComponent<Dropdown>();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        gameObject.SetActive(false);
        return this;
    }

    void DropdownSetup()
    {
        var list = Enumerable.Range(0, 11)
            .Select(x => x.ToString())
            .ToList();
        dropdown.ClearOptions();
        dropdown.AddOptions(list);
        //var animator = OutfitListUI.TargetOutfit.pelvis.CompData.Animator;
        //StartCoroutine(animator.SetTriggerForOneFrame("PhoneBack"));
        //Log.Debug("Getting animations from pyjamas:");
        //var animator = OutfitListUI.TargetOutfit.pelvis.CompData.Animator;
        ////how do we get a list of animations from the animator
        //List<string> animationNames = animator.parameters
        //    .Where(x => x.type == AnimatorControllerParameterType.Bool)
        //    .Select(x => x.name)
        //    .ToList();
        //animationNames.ForEach(Log.Debug);
        //dropdown.ClearOptions();
        //dropdown.AddOptions(animationNames);
    }

    public void Show(RecipeDescriptor recipe, UnityAction<RecipeDescriptor, string> onConfirm)
    {
        this.recipe = recipe;
        this.onConfirm = onConfirm;
        DropdownSetup();
        CCPlugin.CoroutineRunner.StartCoroutine(idk());
    }

    IEnumerator idk()
    {
        if (OutfitListUI.TargetOutfit.pelvis is PlayerWatchdog player)
        {
            //player.UnlockPlayer();
            yield return player.UnlockRoutine();
            var carolEntity = player.carolEntity;
            carolEntity.LockMove(float.PositiveInfinity);
            carolEntity.LockAttack(float.PositiveInfinity);
            carolEntity.AddInvulnerableTime(float.PositiveInfinity);
            carolEntity.inventory.DrawWeapon(null);
            carolEntity.inventory.HideHeldWeapon(false);
            carolEntity.enabled = false;
        }
        else Log.Warning("Carol was not a player when showing filename dialogue.");

        gameObject.SetActive(true);
    }

    void OnDropdownChanged(int index)
    {
        //get animation name
        //var selection = dropdown.options[index];
        //get animator
        var animator = OutfitListUI.TargetOutfit.pelvis.CompData.Animator;
        //animator.SetBool(selection.text, true);
        StartCoroutine(Dialogue.controller.SetExpressionValue(animator, "Expression", index));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) gameObject.SetActive(false);
    }

    void OnConfirm()
    {
        if (textBox.text != "") onConfirm?.Invoke(recipe, textBox.text);
        var animator = OutfitListUI.TargetOutfit.pelvis.CompData.Animator;
        StartCoroutine(Dialogue.controller.SetExpressionValue(animator, "Expression", 0));
        if (OutfitListUI.TargetOutfit.pelvis is PlayerWatchdog player) player.LockPlayer();
        gameObject.SetActive(false);
    }

    void OnCancel()
    {
        var animator = OutfitListUI.TargetOutfit.pelvis.CompData.Animator;
        StartCoroutine(Dialogue.controller.SetExpressionValue(animator, "Expression", 0));
        if (OutfitListUI.TargetOutfit.pelvis is PlayerWatchdog player) player.LockPlayer();
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        textBox.text = "";
        var animator = OutfitListUI.TargetOutfit.pelvis.CompData.Animator;
        StartCoroutine(Dialogue.controller.SetExpressionValue(animator, "Expression", 0));
    }
}
