using UnityEngine;

public class InventoryWindowToggle : MonoBehaviour
{
    [SerializeField]
    private GameObject inventoryWindow;

    public void ToggleInventory()
    {
        inventoryWindow.SetActive(
            !inventoryWindow.activeSelf);
    }
}