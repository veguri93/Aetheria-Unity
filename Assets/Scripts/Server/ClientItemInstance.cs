public class ClientItemInstance
{
    public int ObjectId { get; }

    public int ItemId { get; }

    public int Quantity { get; private set; }

    public string EquippedSlot { get; private set; }

    public bool IsQuestItem { get; }

    public bool IsEquipped =>
        EquippedSlot != "None";

    public ClientItemInstance(
        int objectId,
        int itemId,
        int quantity,
        string equippedSlot,
        bool isQuestItem = false)
    {
        ObjectId =
            objectId;

        ItemId =
            itemId;

        Quantity =
            quantity;

        EquippedSlot =
            equippedSlot;

        IsQuestItem =
            isQuestItem;
    }

    public void SetQuantity(
        int quantity)
    {
        Quantity =
            quantity;
    }

    public void SetEquippedSlot(
        string equippedSlot)
    {
        EquippedSlot =
            equippedSlot;
    }
}