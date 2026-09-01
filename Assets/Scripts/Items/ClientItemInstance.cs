public class ClientItemInstance
{
    public int ObjectId { get; }

    public int ItemId { get; }

    public int Quantity { get; private set; }

    public string EquippedSlot { get; private set; }

    public bool IsEquipped =>
        EquippedSlot != "None";

    public ClientItemInstance(
        int objectId,
        int itemId,
        int quantity,
        string equippedSlot)
    {
        ObjectId = objectId;
        ItemId = itemId;
        Quantity = quantity;
        EquippedSlot = equippedSlot;
    }

    public void SetQuantity(
        int quantity)
    {
        Quantity = quantity;
    }

    public void SetEquippedSlot(
        string equippedSlot)
    {
        EquippedSlot = equippedSlot;
    }
}