using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRowUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text questNameText;

    [SerializeField]
    private Button button;

    public int QuestId { get; private set; }

    private QuestWindow _questWindow;

    public void SetData(
        int questId,
        string questName,
        QuestWindow questWindow)
    {
        QuestId =
            questId;

        questNameText.text =
            questName;

        _questWindow =
            questWindow;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(
            HandleClick);
    }

    private void HandleClick()
    {
        if (_questWindow == null)
            return;

        _questWindow.SelectQuest(
            QuestId);
    }
}