using System;
using UnityEngine;

[Serializable]
public class ItemVisualDefinition
{
    [SerializeField] private int itemId;
    [SerializeField] private string itemName;

    [SerializeField] private string type;

    [TextArea(2, 5)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject prefab;

    public int ItemId => itemId;
    public string ItemName => itemName;
    public string Type => type;
    public string Description => description;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
}