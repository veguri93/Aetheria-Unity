using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PurchaseDetailsWindow : MonoBehaviour
{
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemName,_descriptionText,_itemType;
    [SerializeField] private Transform _costContainer;
    [SerializeField] private PurchaseCostRowUI _costRowTemplate;
    [SerializeField] private Button _buyButton;
    [SerializeField] private GameObject _weaponStats,_iconFallback;
    [SerializeField] private TMP_Text _pAtk,_mAtk,_atkSpd,_castSpd;
    [SerializeField] private ScrollRect _infoScroll;
    private int _entryId;
    private MerchantOffer current;
    private void OnEnable(){ClientInventory.Changed+=RefreshAvailability;RefreshAvailability();}
    private void OnDisable(){ClientInventory.Changed-=RefreshAvailability;}
    public void RefreshAvailability()
    {
        bool available=current!=null;
        if(current!=null)
        {
            var requirements=new System.Collections.Generic.Dictionary<int,long>();
            foreach(var cost in current.Costs){requirements.TryGetValue(cost.ItemId,out long required);requirements[cost.ItemId]=required+cost.Amount;}
            foreach(var cost in requirements){long owned=0;foreach(var item in ClientInventory.Items)if(item.ItemId==cost.Key&&!item.IsEquipped)owned+=item.Quantity;if(owned<cost.Value){available=false;break;}}
        }
        if(_buyButton)_buyButton.interactable=available;
    }
    private void Awake(){_buyButton.onClick.AddListener(Buy);_costRowTemplate.gameObject.SetActive(false);Clear();}
    public void Clear()
    {
        current=null;_entryId=0;ClearCosts();_buyButton.interactable=false;_itemIcon.enabled=false;if(_iconFallback)_iconFallback.SetActive(false);
        _itemName.text="Inspect an item";_itemType.text="MERCHANT'S COLLECTION";_descriptionText.text="Select an item from the collection to view its attributes, description and purchase requirements.";_weaponStats.SetActive(false);
        if(_infoScroll)_infoScroll.verticalNormalizedPosition=1;
    }
    private void ClearCosts(){for(int i=_costContainer.childCount-1;i>=0;i--){var child=_costContainer.GetChild(i);if(child!=_costRowTemplate.transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}}}
    private static string Stat(string value)=>string.IsNullOrEmpty(value)?"—":value;
    public void ShowOffer(MerchantOffer offer)
    {
        current=offer;_entryId=offer.EntryId;_itemIcon.sprite=offer.Icon;_itemIcon.enabled=offer.Icon;_itemIcon.preserveAspect=true;if(_iconFallback)_iconFallback.SetActive(!offer.Icon);
        _itemName.text=offer.Name;_itemType.text=offer.Metadata.type+(string.IsNullOrEmpty(offer.Metadata.tier)||offer.Metadata.tier=="None"?"":"  ·  "+offer.Metadata.tier);
        _weaponStats.SetActive(offer.Metadata.weapon);_pAtk.text=Stat(offer.Metadata.pAtk);_mAtk.text=Stat(offer.Metadata.mAtk);_atkSpd.text=Stat(offer.Metadata.atkSpd);_castSpd.text=Stat(offer.Metadata.castSpd);
        _descriptionText.text=(string.IsNullOrWhiteSpace(offer.Description)?"No description available.":offer.Description)+(string.IsNullOrEmpty(offer.AdditionalProducts)?"":"\n\nALSO INCLUDED"+offer.AdditionalProducts);
        ClearCosts();foreach(var cost in offer.Costs){var row=Instantiate(_costRowTemplate,_costContainer);row.SetData(cost);row.gameObject.SetActive(true);}
        gameObject.SetActive(true);RefreshAvailability();Canvas.ForceUpdateCanvases();if(_infoScroll){_infoScroll.StopMovement();_infoScroll.verticalNormalizedPosition=1;}
    }
    public void Hide(){Clear();}
    private void Buy(){RefreshAvailability();if(_entryId>0&&_buyButton.interactable)MultiSellWindow.Instance?.BuyEntry(_entryId);}
}
