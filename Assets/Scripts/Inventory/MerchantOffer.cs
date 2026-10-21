using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Read-only presentation of the existing multisell wire data. No purchase authority.
[Serializable] public sealed class MerchantItemMetadata
{
    public string name, type, tier, pAtk, mAtk, atkSpd, castSpd;
    public bool weapon;
}
public sealed class MerchantOffer
{
    public int EntryId, ItemId, Count;
    public Sprite Icon;
    public string Name, Description, AdditionalProducts;
    public MerchantItemMetadata Metadata = new MerchantItemMetadata();
    public readonly List<PurchaseCostData> Costs = new List<PurchaseCostData>();
    public static List<MerchantOffer> Parse(string wire, ItemVisualDatabase database)
    {
        var offers=new List<MerchantOffer>();
        foreach(var entry in (wire??string.Empty).Split(';'))
        {
            var p=entry.Split('~');
            if((p.Length!=3&&p.Length!=4)||!int.TryParse(p[0],out int id)||id<=0)continue;
            var products=p[2].Split(',');var first=products[0].Split(':');
            if(first.Length!=2||!int.TryParse(first[0],out int item)||!int.TryParse(first[1],out int count)||item<=0||count<=0)continue;
            var offer=new MerchantOffer{EntryId=id,ItemId=item,Count=count};
            if(database&&database.TryGet(item,out var visual)){offer.Icon=visual.Icon;offer.Name=visual.ItemName;offer.Description=visual.Description;offer.Metadata.type=visual.Type;}
            if(p.Length==4&&!string.IsNullOrEmpty(p[3]))
            {
                try{offer.Metadata=JsonUtility.FromJson<MerchantItemMetadata>(Encoding.UTF8.GetString(Convert.FromBase64String(p[3])))??offer.Metadata;}
                catch(Exception ex) when(ex is FormatException||ex is ArgumentException){Debug.LogWarning("Invalid merchant display metadata; using available item visuals.");}
            }
            if(!string.IsNullOrEmpty(offer.Metadata.name))offer.Name=offer.Metadata.name;
            if(string.IsNullOrEmpty(offer.Name))offer.Name="Item #"+item;
            if(count>1)offer.Name+=" ×"+count;
            bool valid=true;
            foreach(var ingredient in p[1].Split(','))
            {
                var cost=ingredient.Split(':');
                if(cost.Length!=2||!int.TryParse(cost[0],out int costId)||!int.TryParse(cost[1],out int amount)||costId<=0||amount<=0){valid=false;break;}
                Sprite icon=null;string name="Item #"+costId;
                if(database&&database.TryGet(costId,out var v)){icon=v.Icon;name=v.ItemName;}
                if(costId==item&&!string.IsNullOrEmpty(offer.Metadata.name))name=offer.Metadata.name;
                offer.Costs.Add(new PurchaseCostData(icon,amount,costId,name));
            }
            for(int i=1;i<products.Length;i++)
            {
                var extra=products[i].Split(':');if(extra.Length!=2||!int.TryParse(extra[0],out int extraId)||!int.TryParse(extra[1],out int extraCount)||extraId<=0||extraCount<=0){valid=false;break;}
                string name=database&&database.TryGet(extraId,out var v)?v.ItemName:"Item #"+extraId;
                offer.AdditionalProducts+="\n"+name+" ×"+extraCount;
            }
            if(valid&&offer.Costs.Count>0)offers.Add(offer);
        }
        return offers;
    }
}
