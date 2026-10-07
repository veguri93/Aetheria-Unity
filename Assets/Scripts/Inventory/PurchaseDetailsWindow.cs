using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseDetailsWindow : MonoBehaviour
{
    [SerializeField]
    private Image _itemIcon;

    [SerializeField]
    private TMP_Text _itemName;

    [SerializeField]
    private TMP_Text _descriptionText;

    [SerializeField]
    private Transform _costContainer;

    [SerializeField]
    private PurchaseCostRowUI _costRowTemplate;

    [SerializeField]
    private Button _buyButton;

    private int _entryId;

    private void Awake()
    {
        if (_buyButton != null)
        {
            _buyButton.onClick.AddListener(
                Buy);
        }

        if (_costRowTemplate != null)
        {
            _costRowTemplate.gameObject.SetActive(
                false);
        }

        Hide();
    }

    public void Show(
        int entryId,
        Sprite itemIcon,
        string itemName,
        string description,
        IReadOnlyList<PurchaseCostData> costs)
    {
        _entryId =
            entryId;

        if (_itemIcon != null)
        {
            _itemIcon.sprite =
                itemIcon;

            _itemIcon.enabled =
                itemIcon != null;
        }

        if (_itemName != null)
        {
            _itemName.text =
                itemName;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text =
                description;
        }

        ClearCosts();

        foreach (PurchaseCostData cost
            in costs)
        {
            PurchaseCostRowUI row =
                Instantiate(
                    _costRowTemplate,
                    _costContainer);

            row.SetData(
                cost.Icon,
                cost.Amount);

            row.gameObject.SetActive(
                true);
        }

        gameObject.SetActive(
            true);
    }

    private void ClearCosts()
    {
        if (_costContainer == null ||
            _costRowTemplate == null)
        {
            return;
        }

        for (int i =
                 _costContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                _costContainer.GetChild(
                    i);

            if (child.gameObject ==
                _costRowTemplate.gameObject)
            {
                continue;
            }

            Destroy(
                child.gameObject);
        }
    }

    public void Hide()
    {
        _entryId =
            0;

        ClearCosts();

        gameObject.SetActive(
            false);
    }

    private void Buy()
    {
        if (_entryId <= 0)
            return;

        MultiSellWindow.Instance?
            .BuyEntry(
                _entryId);
    }
}