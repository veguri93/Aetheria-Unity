
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEquipmentVisual : MonoBehaviour
{
    [Serializable]
    private class WeaponVisual
    {
        public int ItemId;
        public GameObject Model;
    }

    [SerializeField]
    private List<WeaponVisual> _weapons = new();

    public void Refresh()
    {
        // Hide all weapon models first.
        foreach (WeaponVisual weapon in _weapons)
        {
            if (weapon.Model != null)
                weapon.Model.SetActive(false);
        }

        // Find the currently equipped weapon.
        foreach (ClientItemInstance item in ClientInventory.Items)
        {
            if (!item.IsEquipped)
                continue;

            foreach (WeaponVisual weapon in _weapons)
            {
                if (weapon.ItemId != item.ItemId)
                    continue;

                if (weapon.Model == null)
                    continue;

                weapon.Model.SetActive(true);
                return;
            }
        }
    }
}
