using System.Collections.Generic;
using UnityEngine;

public class GroundItemManager : MonoBehaviour
{
    public static GroundItemManager Instance { get; private set; }

    [SerializeField]
    private WorldItemDrop worldItemDropPrefab;

    private readonly Dictionary<int, WorldItemDrop> groundItems =
        new();

    private void Awake()
    {
        Instance = this;
    }

    public void Spawn(
        int objectId,
        int itemId,
        int quantity,
        Vector3 position)
    {
        if (groundItems.ContainsKey(objectId))
            return;

        WorldItemDrop drop =
            Instantiate(
                worldItemDropPrefab,
                position,
                Quaternion.identity);

        drop.Initialize(
            objectId,
            itemId,
            quantity);

        groundItems.Add(
            objectId,
            drop);

    }

    public void Despawn(int objectId)
    {
        if (!groundItems.TryGetValue(
                objectId,
                out WorldItemDrop drop))
        {
            Debug.LogWarning(
                $"[GROUND ITEM] Cannot despawn. " +
                $"ObjectId={objectId} was not found.");

            return;
        }

        groundItems.Remove(
            objectId);

        Destroy(
            drop.gameObject);

    }
}