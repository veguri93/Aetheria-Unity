using System.Collections.Generic;

public static class ClientInventory
{
    private static readonly Dictionary<int, ClientItemInstance> _items =
        new();

    public static IReadOnlyCollection<ClientItemInstance> Items =>
        _items.Values;

    public static void Clear()
    {
        _items.Clear();
    }

    public static void Add(
        ClientItemInstance item)
    {
        _items[item.ObjectId] =
            item;
    }

    public static bool TryGetItem(
        int objectId,
        out ClientItemInstance item)
    {
        return _items.TryGetValue(
            objectId,
            out item);
    }

    public static bool TryGetItemByItemId(
        int itemId,
        out ClientItemInstance item)
    {
        foreach (ClientItemInstance inventoryItem in
                 _items.Values)
        {
            if (inventoryItem.ItemId != itemId)
                continue;

            item =
                inventoryItem;

            return true;
        }

        item =
            null;

        return false;
    }

    public static bool Remove(
        int objectId)
    {
        return _items.Remove(
            objectId);
    }
}