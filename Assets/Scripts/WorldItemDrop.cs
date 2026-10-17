
using UnityEngine;

public class WorldItemDrop : MonoBehaviour
{
    [SerializeField]
    private ItemVisualDatabase _itemVisualDatabase;

    [SerializeField]
    private Transform _visualRoot;

    private MeshRenderer _defaultRenderer;
    private GameObject _spawnedVisual;

    public int ObjectId { get; private set; }
    public int ItemId { get; private set; }
    public int Quantity { get; private set; }

    private void Awake()
    {
        _defaultRenderer =
            GetComponent<MeshRenderer>();
    }

    public void Initialize(
        int objectId,
        int itemId,
        int quantity)
    {
        ObjectId = objectId;
        ItemId = itemId;
        Quantity = quantity;

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_defaultRenderer != null)
            _defaultRenderer.enabled = true;

        if (_visualRoot == null ||
            _itemVisualDatabase == null)
        {
            return;
        }

        if (!_itemVisualDatabase.TryGet(
                ItemId,
                out ItemVisualDefinition definition))
        {
            return;
        }

        if (definition.Prefab == null)
            return;

        GameObject prefab = definition.Prefab;

        _spawnedVisual = Instantiate(
            prefab,
            _visualRoot.position,
            prefab.transform.rotation);

        // Preserve the model's world scale.
        _spawnedVisual.transform.SetParent(
            _visualRoot,
            true);

        // Apply item-specific ground positioning.
        _spawnedVisual.transform.localPosition =
            definition.GroundPosition;

        // Rotate relative to the prefab's original rotation.
        _spawnedVisual.transform.localRotation =
            Quaternion.Euler(definition.GroundRotation) *
            _spawnedVisual.transform.localRotation;

        // Scale relative to the prefab's original size.
        _spawnedVisual.transform.localScale =
            Vector3.Scale(
                _spawnedVisual.transform.localScale,
                definition.GroundScale);

        // Keep the existing root pickup collider.
        // Disable any colliders on the visual model.
        foreach (Collider collider in
                 _spawnedVisual.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        // Hide the generic sphere.
        if (_defaultRenderer != null)
            _defaultRenderer.enabled = false;
    }
}
