using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseCostRowUI : MonoBehaviour
{
    [SerializeField]
    private Image _costIcon;

    [SerializeField]
    private TMP_Text _costAmount;

    public void SetData(
        Sprite icon,
        int amount)
    {
        if (_costIcon != null)
        {
            _costIcon.sprite =
                icon;

            _costIcon.enabled =
                icon != null;
        }

        if (_costAmount != null)
        {
            _costAmount.text =
                amount.ToString();
        }
    }
}