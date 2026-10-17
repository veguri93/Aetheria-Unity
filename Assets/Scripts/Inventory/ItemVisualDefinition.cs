
using System;
using UnityEngine;

[Serializable]
public class ItemVisualDefinition
{
    [SerializeField]
    private int itemId;

    [SerializeField]
    private string itemName;

    [SerializeField]
    private string type;

    [TextArea(2, 5)]
    [SerializeField]
    private string description;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private GameObject prefab;

    [Header("Ground Drop Visual")]

    [SerializeField]
    private Vector3 groundPosition = Vector3.zero;

    [SerializeField]
    private Vector3 groundRotation = Vector3.zero;

    [SerializeField]
    private Vector3 groundScale = Vector3.one;

    public int ItemId => itemId;
    public string ItemName => itemName;
    public string Type => type;
    public string Description => description;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;

    public Vector3 GroundPosition => groundPosition;
    public Vector3 GroundRotation => groundRotation;
    public Vector3 GroundScale => groundScale;
}
