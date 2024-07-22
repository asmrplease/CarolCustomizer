using CarolCustomizer.Models.Accessories;

namespace CarolCustomizer.Models.Materials;

public record AccMatSlot
{
    //TODO: there may be an issue here with some StoredAccessories not actually being unique enough on their own
    public StoredAccessory accessory;
    public int index;
    public AccMatSlot(StoredAccessory accessory, int index)
    {
        this.accessory = accessory;
        this.index = index;
    }
}
