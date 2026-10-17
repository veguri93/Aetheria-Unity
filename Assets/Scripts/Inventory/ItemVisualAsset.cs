
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewItemVisual",
    menuName = "Aetheria/Items/Item Visual")]
public class ItemVisualAsset : ScriptableObject
{
    [SerializeField]
    private ItemVisualDefinition definition = new();

    public ItemVisualDefinition Definition => definition;
}
