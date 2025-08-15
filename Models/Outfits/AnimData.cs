using System.Linq;
using UnityEngine;

namespace CarolCustomizer.Models.Outfits;
public class AnimData : MonoBehaviour
{
    [SerializeField]
    RuntimeAnimatorController controller;
    public RuntimeAnimatorController Controller => controller;

    [SerializeField]
    Animator animator;
    public Animator Animator => animator;

    bool disableAnimator;

    public AnimData Constructor()
    {
        animator = GetComponentsInParent<Animator>(true)?.FirstOrDefault(x => x.runtimeAnimatorController);

        controller ??= animator?.runtimeAnimatorController;
        if (animator) animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        return this;
    }


    public void SetAnimator(RuntimeAnimatorController rac)
    {
        Animator.runtimeAnimatorController = rac;
        Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (disableAnimator) { DisableAnimator(); }
    }

    public void DisableAnimator()
    {
        if (!Animator) return;

        disableAnimator = true;
        Animator.enabled = false;
        Animator.Rebind();
        var boneData = this.GetComponent<BoneData>();
        if (boneData) boneData.MoveToRestPosition();
    }

    void LateUpdate()
    {
        if (!disableAnimator) return;
        if (!Animator) return;

        Animator.enabled = false;
    }

    public void EnableAnimator()
    {
        if (!Animator) return;

        disableAnimator = false;
        Animator.enabled = true;
        Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }
}
