using CarolCustomizer.Models.Accessories;

namespace CarolCustomizer.Events;
public class AccessoryChangedEvent
{
    public readonly StoredAccessory Target;
    public readonly AccessoryDescriptor State;
    public readonly bool Visible;
    public AccessoryChangedEvent(StoredAccessory target, AccessoryDescriptor state, bool visible)
    {
        this.Target = target;
        this.State = state;
        this.Visible = visible;
    }

    public override string ToString() => $"Accessory Event: {Target} set to {Visible}";
}
