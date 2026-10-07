using UnityEngine;

public class PurchaseCostData
{
    public Sprite Icon { get; }

    public int Amount { get; }

    public PurchaseCostData(
        Sprite icon,
        int amount)
    {
        Icon =
            icon;

        Amount =
            amount;
    }
}