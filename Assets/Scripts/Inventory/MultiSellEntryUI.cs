using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MultiSellEntryUI : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private InventorySelectionGlow selectionGlow;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text fallback;
    private Button button;
    private MultiSellWindow owner;
    public MerchantOffer Offer { get; private set; }
    public int EntryId=>Offer?.EntryId??0;
    private void Awake(){button=GetComponent<Button>();button.onClick.AddListener(Click);}
    public void Bind(MerchantOffer offer,MultiSellWindow window)
    {
        Offer=offer;owner=window;if(!button)button=GetComponent<Button>();button.interactable=offer!=null;
        itemIcon.sprite=offer?.Icon;itemIcon.enabled=offer?.Icon!=null;itemIcon.preserveAspect=true;
        if(quantityText)quantityText.text=offer!=null&&offer.Count>1?offer.Count.ToString():string.Empty;
        if(fallback)fallback.gameObject.SetActive(offer!=null&&!offer.Icon);
        SetSelected(false);
    }
    public void SetSelected(bool selected){if(selectionGlow)selectionGlow.gameObject.SetActive(selected&&Offer!=null);}
    private void Click(){if(Offer!=null)owner.Select(this);}
    public void OnPointerEnter(PointerEventData e){if(Offer!=null&&owner.SelectedOffer!=Offer)GetComponent<Image>().color=new Color(.20f,.19f,.15f,1);}
    public void OnPointerExit(PointerEventData e){GetComponent<Image>().color=new Color(.065f,.071f,.083f,1);}
}
