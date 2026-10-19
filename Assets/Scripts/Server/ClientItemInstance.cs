
public class ClientItemInstance
{
    public int ObjectId { get; }
    public int ItemId { get; }

    public int Quantity { get; private set; }

    public string EquippedSlot { get; private set; }

    public bool IsQuestItem { get; }

    public bool IsEquipped =>
        EquippedSlot != "None";

    public string Tier { get; }
    public string DisplayType { get; }

    public double? PhysicalAttack { get; }
    public double? MagicAttack { get; }
    public double? CastingSpeed { get; }
    public double? AttackSpeed { get; }

    public ClientItemInstance(
        int objectId,
        int itemId,
        int quantity,
        string equippedSlot,
        bool isQuestItem = false,
        string tier = "None",
        string displayType = "",
        double? physicalAttack = null,
        double? magicAttack = null,
        double? castingSpeed = null,
        double? attackSpeed = null)
    {
        ObjectId = objectId;
        ItemId = itemId;
        Quantity = quantity;
        EquippedSlot = equippedSlot;
        IsQuestItem = isQuestItem;

        Tier = tier;
        DisplayType = displayType;

        PhysicalAttack = physicalAttack;
        MagicAttack = magicAttack;
        CastingSpeed = castingSpeed;
        AttackSpeed = attackSpeed;
    }

    public void SetQuantity(int quantity)
    {
        Quantity = quantity;
    }

    public void SetEquippedSlot(
        string equippedSlot)
    {
        EquippedSlot = equippedSlot;
    }
}
