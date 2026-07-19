public enum ConsumableType
{
    Seed,
}


public class ItemData_Consumable : ItemData
{
    public ConsumableType consumableType;
    public float effectValue;
}