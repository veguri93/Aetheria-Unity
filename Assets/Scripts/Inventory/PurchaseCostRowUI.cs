using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PurchaseCostRowUI : MonoBehaviour
{
    [SerializeField] private Image _costIcon;
    [SerializeField] private TMP_Text _costAmount;
    [SerializeField] private TMP_Text _costName;
    [SerializeField] private GameObject _fallback;
    public void SetData(Sprite icon,long amount){_costIcon.sprite=icon;_costIcon.enabled=icon;_costIcon.preserveAspect=true;_costAmount.text=amount.ToString("N0");if(_fallback)_fallback.SetActive(!icon);}
    public void SetData(PurchaseCostData cost, int quantity=1){SetData(cost.Icon,(long)cost.Amount*quantity);if(_costName)_costName.text=cost.Name??"Required item";}
}
