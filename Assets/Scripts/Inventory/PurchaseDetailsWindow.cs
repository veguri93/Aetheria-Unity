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
    private TMP_InputField quantityInput;
    private TMP_Text quantityLabel;
    private readonly System.Collections.Generic.List<PurchaseCostRowUI> rows=new System.Collections.Generic.List<PurchaseCostRowUI>();
    private bool lastCanBuy;
    public int Quantity { get; private set; } = 1;
    private void OnEnable(){ClientInventory.Changed+=RefreshAvailability;MerchantPurchaseState.Changed+=RefreshAvailability;RefreshAvailability();}
    private void OnDisable(){ClientInventory.Changed-=RefreshAvailability;MerchantPurchaseState.Changed-=RefreshAvailability;}
    private void Update(){if(lastCanBuy!=MerchantPurchaseState.CanBuy)RefreshAvailability();}
    private bool ValidQuantity()
    {
        Quantity=0;
        if(!quantityInput || !int.TryParse(quantityInput.text,System.Globalization.NumberStyles.None,System.Globalization.CultureInfo.InvariantCulture,out int q)||q<=0)return false;
        if(current!=null)
        {
            foreach(var collection in new[]{current.Costs,current.Products})
            {
                var totals=new System.Collections.Generic.Dictionary<int,long>();
                foreach(var item in collection)
                {
                    totals.TryGetValue(item.ItemId,out long total);
                    total+=(long)item.Amount*q;
                    if(item.Amount<=0||total>int.MaxValue)return false;
                    totals[item.ItemId]=total;
                }
            }
        }
        Quantity=q;return true;
    }
    private void QuantityChanged(string value)
    {
        bool valid=ValidQuantity();
        if(current!=null)
        {
            for(int i=0;i<rows.Count;i++)rows[i].SetData(current.Costs[i],valid?Quantity:0);
            _itemName.text=current.Name+(valid?" ×"+((long)current.Count*Quantity).ToString("N0"):"");
            string extra="";
            if(valid)for(int i=1;i<current.Products.Count;i++)extra+="\n"+current.Products[i].Name+" ×"+((long)current.Products[i].Amount*Quantity).ToString("N0");
            _descriptionText.text=(string.IsNullOrWhiteSpace(current.Description)?"No description available.":current.Description)+(extra.Length==0?"":"\n\nALSO INCLUDED"+extra);
        }
        RefreshAvailability();
    }
    private void CreateQuantityInput()
    {
        if(quantityInput)return;
        var buy=(RectTransform)_buyButton.transform;
        var area=new GameObject("Purchase quantity",typeof(RectTransform));
        var rect=(RectTransform)area.transform;rect.SetParent(buy.parent,false);
        rect.anchorMin=buy.anchorMin;rect.anchorMax=buy.anchorMax;rect.pivot=buy.pivot;rect.offsetMin=buy.offsetMin;rect.offsetMax=buy.offsetMax;
        // Reuse the existing footer footprint, leaving the original button styled.
        rect.anchorMax=new Vector2(rect.anchorMin.x,rect.anchorMax.y);
        rect.offsetMin=buy.offsetMin;rect.offsetMax=new Vector2(buy.offsetMin.x+150,buy.offsetMax.y);
        buy.offsetMin+=new Vector2(160,0);
        foreach(RectTransform child in buy)
        {
            if(child.name=="Label"||child.name=="Top edge"||child.name=="Bottom edge")
                child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,buy.rect.width);
            else if(child.name=="Right edge")child.anchoredPosition-=new Vector2(160,0);
        }
        var label=new GameObject("Quantity label",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(rect,false);
        quantityLabel=label.GetComponent<TextMeshProUGUI>();quantityLabel.font=_itemType.font;quantityLabel.fontSize=12;quantityLabel.color=new Color(.77f,.65f,.43f);quantityLabel.raycastTarget=false;
        var lr=(RectTransform)label.transform;lr.anchorMin=Vector2.zero;lr.anchorMax=new Vector2(.45f,1);lr.offsetMin=lr.offsetMax=Vector2.zero;
        quantityLabel.alignment=TextAlignmentOptions.MidlineLeft;
        var field=new GameObject("Quantity",typeof(RectTransform),typeof(Image),typeof(TMP_InputField));field.transform.SetParent(rect,false);
        var fr=(RectTransform)field.transform;fr.anchorMin=new Vector2(.46f,.1f);fr.anchorMax=new Vector2(1,.9f);fr.offsetMin=fr.offsetMax=Vector2.zero;
        var bg=field.GetComponent<Image>();bg.color=new Color(.10f,.10f,.11f);
        var outline=field.AddComponent<Outline>();outline.effectColor=new Color(.43f,.32f,.17f);outline.effectDistance=new Vector2(1,-1);
        var viewport=new GameObject("Viewport",typeof(RectTransform),typeof(RectMask2D));viewport.transform.SetParent(fr,false);
        var vr=(RectTransform)viewport.transform;vr.anchorMin=Vector2.zero;vr.anchorMax=Vector2.one;vr.offsetMin=new Vector2(5,2);vr.offsetMax=new Vector2(-5,-2);
        var text=new GameObject("Value",typeof(RectTransform),typeof(TextMeshProUGUI));text.transform.SetParent(vr,false);
        var tr=(RectTransform)text.transform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;
        var tmp=text.GetComponent<TextMeshProUGUI>();tmp.font=_itemName.font;tmp.fontSize=15;tmp.color=new Color(.91f,.83f,.65f);tmp.alignment=TextAlignmentOptions.Midline;tmp.raycastTarget=false;
        quantityInput=field.GetComponent<TMP_InputField>();quantityInput.textViewport=vr;quantityInput.textComponent=tmp;quantityInput.targetGraphic=bg;
        quantityInput.keyboardType=TouchScreenKeyboardType.NumberPad;quantityInput.lineType=TMP_InputField.LineType.SingleLine;
        quantityInput.SetTextWithoutNotify("1");quantityInput.onValueChanged.AddListener(QuantityChanged);
    }
    public void RefreshAvailability()
    {
        bool valid=ValidQuantity();
        lastCanBuy=MerchantPurchaseState.CanBuy;
        bool available=current!=null&&valid&&lastCanBuy;
        if(quantityLabel)quantityLabel.text=!valid?"Invalid":MerchantPurchaseState.Pending?"Buying…":"Quantity";
        if(current!=null)
        {
            var requirements=new System.Collections.Generic.Dictionary<int,long>();
            foreach(var cost in current.Costs){requirements.TryGetValue(cost.ItemId,out long required);requirements[cost.ItemId]=required+(long)cost.Amount*Quantity;}
            foreach(var cost in requirements){long owned=0;foreach(var item in ClientInventory.Items)if(item.ItemId==cost.Key&&!item.IsEquipped)owned+=item.Quantity;if(owned<cost.Value){available=false;break;}}
        }
        if(_buyButton)_buyButton.interactable=available;
    }
    private void Awake(){CreateQuantityInput();_buyButton.onClick.AddListener(Buy);_costRowTemplate.gameObject.SetActive(false);Clear();}
    public void Clear()
    {
        CreateQuantityInput();
        current=null;_entryId=0;quantityInput.SetTextWithoutNotify("1");Quantity=1;ClearCosts();rows.Clear();_buyButton.interactable=false;_itemIcon.enabled=false;if(_iconFallback)_iconFallback.SetActive(false);
        _itemName.text="Inspect an item";_itemType.text="MERCHANT'S COLLECTION";_descriptionText.text="Select an item from the collection to view its attributes, description and purchase requirements.";_weaponStats.SetActive(false);
        if(_infoScroll)_infoScroll.verticalNormalizedPosition=1;
    }
    private void ClearCosts(){for(int i=_costContainer.childCount-1;i>=0;i--){var child=_costContainer.GetChild(i);if(child!=_costRowTemplate.transform){child.gameObject.SetActive(false);Destroy(child.gameObject);}}}
    private static string Stat(string value)=>string.IsNullOrEmpty(value)?"—":value;
    public void ShowOffer(MerchantOffer offer)
    {
        CreateQuantityInput();
        if(current!=offer)quantityInput.SetTextWithoutNotify("1");
        current=offer;_entryId=offer.EntryId;_itemIcon.sprite=offer.Icon;_itemIcon.enabled=offer.Icon;_itemIcon.preserveAspect=true;if(_iconFallback)_iconFallback.SetActive(!offer.Icon);
        _itemName.text=offer.Name;_itemType.text=offer.Metadata.type+(string.IsNullOrEmpty(offer.Metadata.tier)||offer.Metadata.tier=="None"?"":"  ·  "+offer.Metadata.tier);
        _weaponStats.SetActive(offer.Metadata.weapon);_pAtk.text=Stat(offer.Metadata.pAtk);_mAtk.text=Stat(offer.Metadata.mAtk);_atkSpd.text=Stat(offer.Metadata.atkSpd);_castSpd.text=Stat(offer.Metadata.castSpd);
        _descriptionText.text=(string.IsNullOrWhiteSpace(offer.Description)?"No description available.":offer.Description)+(string.IsNullOrEmpty(offer.AdditionalProducts)?"":"\n\nALSO INCLUDED"+offer.AdditionalProducts);
        ClearCosts();rows.Clear();foreach(var cost in offer.Costs){var row=Instantiate(_costRowTemplate,_costContainer);row.SetData(cost);row.gameObject.SetActive(true);rows.Add(row);}
        QuantityChanged(quantityInput.text);
        gameObject.SetActive(true);RefreshAvailability();Canvas.ForceUpdateCanvases();if(_infoScroll){_infoScroll.StopMovement();_infoScroll.verticalNormalizedPosition=1;}
    }
    public void Hide(){Clear();}
    private void Buy(){RefreshAvailability();if(_entryId>0&&_buyButton.interactable)MultiSellWindow.Instance?.BuyEntry(_entryId,Quantity);}
}
