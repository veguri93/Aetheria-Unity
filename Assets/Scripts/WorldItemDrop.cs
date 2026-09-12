using UnityEngine;

public class WorldItemDrop : MonoBehaviour
{
    public int ObjectId { get; private set; }
    public int ItemId { get; private set; }
    public int Quantity { get; private set; }

    public void Initialize(
        int objectId,
        int itemId,
        int quantity)
    {
        ObjectId = objectId;
        ItemId = itemId;
        Quantity = quantity;
    }
}