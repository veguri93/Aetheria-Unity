
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemVisualDatabase",
    menuName = "Aetheria/Items/Item Visual Database")]
public class ItemVisualDatabase : ScriptableObject
{
    [SerializeField]
    private List<ItemVisualAsset> itemAssets = new();

    public bool TryGet(
        int itemId,
        out ItemVisualDefinition definition)
    {
        foreach (ItemVisualAsset asset in itemAssets)
        {
            if (asset == null ||
                asset.Definition == null)
                continue;

            if (asset.Definition.ItemId != itemId)
                continue;

            definition = asset.Definition;
            return true;
        }

        definition = null;
        return false;
    }
}
