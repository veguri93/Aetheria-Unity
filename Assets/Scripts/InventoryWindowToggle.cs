using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryWindowToggle : MonoBehaviour
{
    [SerializeField]
    private GameObject inventoryWindow;

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        inventoryWindow.SetActive(
            !inventoryWindow.activeSelf);
    }
}