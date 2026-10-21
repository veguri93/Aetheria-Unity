using System.Collections.Generic;

public static class ClientInventory
{
    public static event System.Action Changed;
    public static void NotifyChanged() => Changed?.Invoke();
    private static readonly Dictionary<int, ClientItemInstance> _items =
        new();

    public static IReadOnlyCollection<ClientItemInstance> Items =>
        _items.Values;

    public static void Clear()
    {
        _items.Clear(); NotifyChanged();
    }

    public static void Add(
        ClientItemInstance item)
    {
        _items[item.ObjectId] =
            item; NotifyChanged();
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
        bool removed = _items.Remove(objectId); if (removed) NotifyChanged(); return removed;
    }
}
