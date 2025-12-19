using CarolCustomizer.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarolCustomizer.Hooks.Watchdogs.UnhandledArmatures;
internal class SpoilerArmature : UnhandledArmature
{
    void OnEnable() => StartCoroutine(Hotfix());

    IEnumerator Hotfix()
    {
        //reenable hair and stuff because we're not customizing her rn
        Log.Info("SpoilerArmature.OnEnable()");
        yield return new WaitUntil(() => base.watchdog);
        yield return new WaitUntil(() => base.watchdog.CompData);
        yield return new WaitUntil(() => base.watchdog.CompData.EffectGameObjects is not null);
        Log.Info("ready to reenable components");
        base.watchdog.CompData.EffectGameObjects
            .ForEach(x => x.SetActive(true));
        Log.Debug($"{base.watchdog.CompData.EffectGameObjects.Count} objects reenabled");
    }
}
