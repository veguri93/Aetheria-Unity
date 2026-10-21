using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryItemContextMenu : MonoBehaviour
{
    public static bool IsOpen => Instance != null && Instance.isActiveAndEnabled;
    public static InventoryItemContextMenu Instance { get; private set; }

    [SerializeField]
    private Button useButton;

    [SerializeField]
    private Button dropButton;




    private void Awake()
    {
        Instance = this;



        gameObject.SetActive(false);
    }

    // Retained only for serialized scene compatibility; inventory no longer opens this menu.
    public void Show(ClientItemInstance item, RectTransform itemRect) => Hide();
    public void Hide() { gameObject.SetActive(false); }
    private void OnDropClicked() => Hide();
}

