using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemVisualDatabase",
    menuName = "Aetheria/Items/Item Visual Database")]
public class ItemVisualDatabase : ScriptableObject
{
    [SerializeField]
    private List<ItemVisualDefinition> items =
        new();

    public bool TryGet(
        int itemId,
        out ItemVisualDefinition definition)
    {
        definition =
            items.Find(
                item =>
                    item.ItemId == itemId);

        return definition != null;
    }
}