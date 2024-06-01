using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CarolCustomizer.Utils;
internal class SMRWatcher : MonoBehaviour
{
    SkinnedMeshRenderer target;
    Transform[] Bones;

    void Start() 
    { 
        target = GetComponent<SkinnedMeshRenderer>();
        if (!target) { Log.Warning("SMR Watcher setup failed. "); return; }
        Bones = target.bones;
        
    }

    void Update()
    {
        if (!Enumerable.SequenceEqual(Bones, target.bones)) 
        {
            Log.Warning($"{target.name} bones changed!");
            Log.Info($"Old count: {Bones.Count()}, new count: {target.bones.Count()}");
            var list = Bones.Count() < target.bones.Count() ? Bones : target.bones;//use the smaller of the two lists
            foreach (int i in Enumerable.Range(0, list.Length))
            {
                string oldBone = Bones[i] ? Bones[i].name : "null";
                string newBone = target.bones[i] ? target.bones[i].name : "null";
                string comparison = Bones[i] == target.bones[i] ? "==" : "!=";
                Log.Debug($"{i}: old {oldBone} {comparison} new {newBone}");

            }

            Bones = target.bones;
        }
    }
}
