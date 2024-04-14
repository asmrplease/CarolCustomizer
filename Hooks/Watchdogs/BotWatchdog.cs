using CarolCustomizer.Behaviors;
using CarolCustomizer.Utils;
using System.Linq;

namespace CarolCustomizer.Hooks.Watchdogs;
public class BotWatchdog : PelvisWatchdog
{
    public override void Awake()
    {
        base.Awake();
        NPCManager.OnBotSpawn(this);
    }
    public virtual void SetBotName(string botName) { }

    public override void SetBaseVisibility(bool visible)
    {
        foreach (var mesh in MeshData?.baseMeshes.Where(x=>x.name != Constants.RobotHead)) 
            { mesh.gameObject.SetActive(visible); }
    }

    void OnDestroy() => NPCManager.OnBotDespawn(this);
}
