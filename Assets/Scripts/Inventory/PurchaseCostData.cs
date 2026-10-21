using UnityEngine;
public class PurchaseCostData
{
    public Sprite Icon { get; }
    public int Amount { get; }
    public int ItemId { get; }
    public string Name { get; }
    public PurchaseCostData(Sprite icon,int amount,int itemId=0,string name=null){Icon=icon;Amount=amount;ItemId=itemId;Name=name;}
}
