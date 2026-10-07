using System.Collections.Generic;
using UnityEngine;

public class MultiSellWindow : MonoBehaviour
{
    public static MultiSellWindow Instance { get; private set; }

    [SerializeField]
    private Transform _entryContainer;

    [SerializeField]
    private MultiSellEntryUI _entryTemplate;

    [SerializeField]
    private ItemVisualDatabase _itemVisualDatabase;

    [SerializeField]
    private PurchaseDetailsWindow _purchaseDetailsWindow;

    private int _npcObjectId;
    private int _multiSellId;

    public int NpcObjectId =>
        _npcObjectId;

    public int MultiSellId =>
        _multiSellId;

    private void Awake()
    {
        Instance =
            this;

        if (_entryTemplate != null)
        {
            _entryTemplate.gameObject.SetActive(
                false);
        }

        gameObject.SetActive(
            false);
    }

    private void ClearEntries()
    {
        if (_entryContainer == null ||
            _entryTemplate == null)
        {
            return;
        }

        for (int i =
                 _entryContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                _entryContainer.GetChild(
                    i);

            if (child.gameObject ==
                _entryTemplate.gameObject)
            {
                continue;
            }

            Destroy(
                child.gameObject);
        }
    }

    private void CreateEntryRows(
        string entriesData)
    {
        if (_entryContainer == null ||
            _entryTemplate == null ||
            _itemVisualDatabase == null ||
            string.IsNullOrWhiteSpace(
                entriesData))
        {
            return;
        }

        string[] entries =
            entriesData.Split(
                ';',
                System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string entryData
            in entries)
        {
            string[] parts =
                entryData.Split('~');

            if (parts.Length != 3)
                continue;

            if (!int.TryParse(
                    parts[0],
                    out int entryId))
            {
                continue;
            }

            // -----------------------------------------
            // INGREDIENTS
            // Example:
            // 2003:500,2010:25,2020:3
            // -----------------------------------------

            string[] ingredientParts =
                parts[1].Split(
                    ',',
                    System.StringSplitOptions.RemoveEmptyEntries);

            if (ingredientParts.Length == 0)
                continue;

            List<PurchaseCostData> costs =
                new();


            bool ingredientsValid =
                true;

            foreach (string ingredientPart
                in ingredientParts)
            {
                string[] ingredientData =
                    ingredientPart.Split(':');

                if (ingredientData.Length != 2 ||
                    !int.TryParse(
                        ingredientData[0],
                        out int ingredientItemId) ||
                    !int.TryParse(
                        ingredientData[1],
                        out int ingredientCount))
                {
                    ingredientsValid =
                        false;

                    break;
                }

                Sprite costIcon =
                    null;

                if (_itemVisualDatabase.TryGet(
                        ingredientItemId,
                        out ItemVisualDefinition costDefinition))
                {
                    costIcon =
                        costDefinition.Icon;
                }
                else
                {
                    Debug.LogWarning(
                        $"Multisell ingredient {ingredientItemId} " +
                        $"was not found in ItemVisualDatabase.");
                }

                costs.Add(
                    new PurchaseCostData(
                        costIcon,
                        ingredientCount));

            }

            if (!ingredientsValid ||
                costs.Count == 0)
            {
                continue;
            }

            // -----------------------------------------
            // PRODUCT
            // Example: 1001:1
            // -----------------------------------------

            string[] productParts =
                parts[2].Split(
                    ',',
                    System.StringSplitOptions.RemoveEmptyEntries);

            if (productParts.Length == 0)
                continue;

            string[] productData =
                productParts[0].Split(':');

            if (productData.Length != 2 ||
                !int.TryParse(
                    productData[0],
                    out int productItemId) ||
                !int.TryParse(
                    productData[1],
                    out int productCount))
            {
                continue;
            }

            if (!_itemVisualDatabase.TryGet(
                    productItemId,
                    out ItemVisualDefinition productDefinition))
            {
                Debug.LogWarning(
                    $"Multisell product {productItemId} " +
                    $"was not found in ItemVisualDatabase.");

                continue;
            }

            // -----------------------------------------
            // CREATE SHOP ROW
            // -----------------------------------------

            MultiSellEntryUI entry =
                Instantiate(
                    _entryTemplate,
                    _entryContainer);

            entry.gameObject.SetActive(
                true);

            string displayName =
                productDefinition.ItemName;

            if (productCount > 1)
            {
                displayName +=
                    $" x{productCount}";
            }

            entry.SetEntryId(
                entryId);

            entry.SetData(
                productDefinition.Icon,
                displayName);

            entry.SetPurchaseDetails(
                productDefinition.Description,
                costs);
        }
    }

    public void ShowEntryDetails(
        int entryId,
        Sprite itemIcon,
        string itemName,
        string description,
        IReadOnlyList<PurchaseCostData> costs)
    {
        if (_purchaseDetailsWindow == null)
            return;

        _purchaseDetailsWindow.Show(
            entryId,
            itemIcon,
            itemName,
            description,
            costs);
    }

    public void BuyEntry(
        int entryId)
    {
        if (entryId <= 0)
            return;

        GameServerConnection.Instance?
            .SendMultiSellBuyRequest(
                _npcObjectId,
                _multiSellId,
                entryId,
                1);
    }

    public void Show(
        int npcObjectId,
        int multiSellId,
        string entriesData)
    {
        _npcObjectId =
            npcObjectId;

        _multiSellId =
            multiSellId;

        NpcHtmlWindow.Instance?
            .Hide();

        _purchaseDetailsWindow?
            .Hide();

        ClearEntries();

        CreateEntryRows(
            entriesData);

        gameObject.SetActive(
            true);
    }

    public void Hide()
    {
        _purchaseDetailsWindow?
            .Hide();

        gameObject.SetActive(
            false);
    }
}