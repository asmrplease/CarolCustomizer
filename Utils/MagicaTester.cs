using CarolCustomizer.Hooks.Watchdogs;
using CarolCustomizer.Models.Accessories;
using MagicaCloth2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Utils;
internal static class MagicaTester
{
    internal static void Test(MagicaCloth cloth, LiveAccessory acc, PelvisWatchdog armatureRoot)
    {
        Log.Info("Magica Cloth Test");
        //basic null checks
        if (acc is null) { Log.Error("LiveAccessory is null"); return; }
        SkinnedMeshRenderer liveSMR = null;//acc.DEBUG_GET_SMR();
        var storedSMR = acc.storedAcc.referenceSMR;
        if (!cloth) { Log.Error("Cloth null"); return; }
        if (!liveSMR) { Log.Error("SMR null"); return; }
        if (!armatureRoot) { Log.Error("Armature Root null"); return; }

        //status checks
        var sdata = cloth.SerializeData;
        var process = cloth.Process;
        Log.Info($"sdata valid: {sdata.IsValid()}");
        Log.Info($"process valid: {process.Result.Result}");

        Log.Info($"SMR name: {liveSMR.name}");
        string pelvisAwake = armatureRoot.gameObject.activeInHierarchy ? "enabled" : "disabled";
        Log.Info($"Pelvis {armatureRoot} {pelvisAwake}");

        //check smr bones
        var smrBones = liveSMR.bones;
        var validSMRBones = liveSMR.bones.Where(x => x);
        var invalidSMRBones = liveSMR.bones.Where(x => !x);
        Log.Debug($"smrbones.Length = {smrBones.Length}");
        if (invalidSMRBones.Any()) Log.Error($"Invalid SMR bone count: {invalidSMRBones.Count()}");
        var SMRnonChildBones = validSMRBones.Where(x => !x.IsChildOf(armatureRoot.transform));
        Log.Info($"SMR Bones assigned to wrong armature: {SMRnonChildBones.Count()}");

        //check magica references
        if (!sdata.sourceRenderers.Contains(liveSMR)) Log.Error("cloth is not pointed at smr");
        if (sdata.sourceRenderers.Any(x => x != liveSMR)) Log.Warning("magica was pointed at another smr");
        var usedTransforms = new HashSet<Transform>();
        sdata.GetUsedTransform(usedTransforms);
        Log.Debug($"UsedTransforms.Count = {usedTransforms.Count()}");
        usedTransforms.ForEach(x => Log.Debug(x.name));
        var magicaNonChildBones = usedTransforms.Where(x => !x.IsChildOf(armatureRoot.transform));
        Log.Info($"Magica Bones assigned to wrong armature: {magicaNonChildBones.Count()}");
        magicaNonChildBones.ForEach(x => Log.Debug(x.name));

        Log.Info("Test Complete.");
    }
}
