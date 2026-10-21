using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiSellWindow : MonoBehaviour
{
    public static MultiSellWindow Instance { get; private set; }
    [SerializeField] private Transform _entryContainer;
    [SerializeField] private MultiSellEntryUI _entryTemplate;
    [SerializeField] private ItemVisualDatabase _itemVisualDatabase;
    [SerializeField] private PurchaseDetailsWindow _purchaseDetailsWindow;
    [SerializeField] private ScrollRect _scroll;
    [SerializeField] private TMP_Text _offerCount;
    [SerializeField,Min(1)] private int columns=6;
    [SerializeField,Min(1)] private int minimumSlots=30;
    private readonly List<MultiSellEntryUI> slots=new List<MultiSellEntryUI>();
    private int _npcObjectId,_multiSellId;
    public int NpcObjectId=>_npcObjectId;
    public int MultiSellId=>_multiSellId;
    public MerchantOffer SelectedOffer { get; private set; }
    public IReadOnlyList<MultiSellEntryUI> Slots=>slots;
    private void Awake(){Instance=this;if(_entryTemplate)_entryTemplate.gameObject.SetActive(false);gameObject.SetActive(false);}
    private void OnDisable(){ClearSelection();}
    public void ClearSelection(){SelectedOffer=null;foreach(var slot in slots)if(slot)slot.SetSelected(false);if(_purchaseDetailsWindow)_purchaseDetailsWindow.Clear();}
    private void ClearEntries(){foreach(var slot in slots)if(slot){slot.gameObject.SetActive(false);Destroy(slot.gameObject);}slots.Clear();}
    public void Select(MultiSellEntryUI slot)
    {
        if(!slot||slot.Offer==null)return;
        InventoryUI.InterruptTooltips();SelectedOffer=slot.Offer;
        foreach(var item in slots)item.SetSelected(item==slot);
        _purchaseDetailsWindow.ShowOffer(SelectedOffer);
    }
    public void Show(int npcObjectId,int multiSellId,string entriesData)
    {
        _npcObjectId=npcObjectId;_multiSellId=multiSellId;
        NpcHtmlWindow.Instance?.Hide();InventoryUI.InterruptTooltips();ClearSelection();ClearEntries();
        var offers=MerchantOffer.Parse(entriesData,_itemVisualDatabase);
        int count=Mathf.Max(minimumSlots,Mathf.CeilToInt(offers.Count/(float)columns)*columns);
        for(int i=0;i<count;i++)
        {
            var slot=Instantiate(_entryTemplate,_entryContainer);slot.Bind(i<offers.Count?offers[i]:null,this);slot.gameObject.SetActive(true);slots.Add(slot);
        }
        if(_offerCount)_offerCount.text=offers.Count+" offers   ·   Select an item to inspect";
        gameObject.SetActive(true);transform.SetAsLastSibling();if(_purchaseDetailsWindow)_purchaseDetailsWindow.Clear();
        Canvas.ForceUpdateCanvases();if(_scroll){_scroll.StopMovement();_scroll.verticalNormalizedPosition=1;}
    }
    public void Hide(){ClearSelection();gameObject.SetActive(false);}
    public void BuyEntry(int entryId)
    {
        if(!isActiveAndEnabled||SelectedOffer==null||SelectedOffer.EntryId!=entryId||_npcObjectId<=0||_multiSellId<=0)return;
        InventoryUI.InterruptTooltips();GameServerConnection.Instance?.SendMultiSellBuyRequest(_npcObjectId,_multiSellId,entryId,1);
    }
}
