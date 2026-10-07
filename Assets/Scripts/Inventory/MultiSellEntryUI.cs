using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiSellEntryUI : MonoBehaviour
{
    [SerializeField]
    private Image itemIcon;

    private Button _button;

    private int _entryId;

    private Sprite _productIcon;

    private string _productName =
        string.Empty;

    private string _description =
        string.Empty;

    private readonly List<PurchaseCostData> _costs =
        new();

    public int EntryId =>
        _entryId;

    private void Awake()
    {
        _button =
            GetComponent<Button>();

        if (_button != null)
        {
            _button.onClick.AddListener(
                HandleClick);
        }
    }

    public void SetEntryId(
        int entryId)
    {
        _entryId =
            entryId;
    }

    public void SetData(
        Sprite productIcon,
        string productName)
    {
        _productIcon =
            productIcon;

        _productName =
            productName;

        if (itemIcon != null)
        {
            itemIcon.sprite =
                productIcon;

            itemIcon.enabled =
                productIcon != null;
        }
    }

    public void SetPurchaseDetails(
        string description,
        IReadOnlyList<PurchaseCostData> costs)
    {
        _description =
            description;

        _costs.Clear();

        if (costs == null)
            return;

        foreach (PurchaseCostData cost
            in costs)
        {
            _costs.Add(
                cost);
        }
    }

    private void HandleClick()
    {
        if (_entryId <= 0)
            return;

        MultiSellWindow.Instance?
            .ShowEntryDetails(
                _entryId,
                _productIcon,
                _productName,
                _description,
                _costs);
    }
}