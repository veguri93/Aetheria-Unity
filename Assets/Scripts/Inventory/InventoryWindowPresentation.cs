using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Tab appearance and window close only. Existing button callbacks keep ownership of filtering.</summary>
public sealed class InventoryWindowPresentation : MonoBehaviour
{
    [SerializeField] private Button itemsTab;
    [SerializeField] private Button questTab;
    [SerializeField] private Image itemsUnderline;
    [SerializeField] private Image questUnderline;
    [SerializeField] private TMP_Text itemsLabel;
    [SerializeField] private TMP_Text questLabel;
    [SerializeField] private Color activeText=new Color(.86f,.72f,.46f,1);
    [SerializeField] private Color inactiveText=new Color(.59f,.61f,.64f,1);
    private bool questActive;
    private void Awake(){itemsTab.onClick.AddListener(Items);questTab.onClick.AddListener(Quests);Paint();}
    private void OnEnable()=>Paint();
    private void Items(){questActive=false;Paint();}
    private void Quests(){questActive=true;Paint();}
    private void Paint()
    {
        if(itemsUnderline)itemsUnderline.gameObject.SetActive(!questActive);
        if(questUnderline)questUnderline.gameObject.SetActive(questActive);
        if(itemsLabel)itemsLabel.color=questActive?inactiveText:activeText;
        if(questLabel)questLabel.color=questActive?activeText:inactiveText;
    }
    public void Close()=>gameObject.SetActive(false);
    private void OnDestroy(){if(itemsTab)itemsTab.onClick.RemoveListener(Items);if(questTab)questTab.onClick.RemoveListener(Quests);}
}
