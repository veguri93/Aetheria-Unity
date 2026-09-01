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
    private Sprite icon;

    [SerializeField]
    private GameObject prefab;

    public int ItemId =>
        itemId;

    public string ItemName =>
        itemName;

    public Sprite Icon =>
        icon;

    public GameObject Prefab =>
        prefab;
}