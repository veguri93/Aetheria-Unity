using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private Button button;
    [SerializeField] private InventorySelectionGlow selectionGlow;
    public int QuestId { get; private set; }
    private QuestWindow _questWindow;
    public void SetData(int questId,string questName,QuestWindow questWindow)
    {
        QuestId=questId;questNameText.text=questName??string.Empty;_questWindow=questWindow;
        button.onClick.RemoveListener(HandleClick);button.onClick.AddListener(HandleClick);
    }
    public void SetSelected(bool selected){if(selectionGlow)selectionGlow.gameObject.SetActive(selected);}
    private void HandleClick(){if(_questWindow)_questWindow.SelectQuest(QuestId);}
}
