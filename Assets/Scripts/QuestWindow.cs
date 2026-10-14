using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestWindow : MonoBehaviour
{
    public static QuestWindow Instance { get; private set; }

    [Header("Quest List")]
    [SerializeField]
    private Transform questListContent;

    [SerializeField]
    private Button questRowTemplate;

    [Header("Quest Details")]
    [SerializeField]
    private TMP_Text questNameText;

    [SerializeField]
    private TMP_Text descriptionText;

    [SerializeField]
    private TMP_Text objectiveText;

    [SerializeField]
    private Button abandonQuestButton;

    private readonly Dictionary<int, QuestViewData>
        _quests =
            new();

    private readonly Dictionary<int, QuestRowUI>
        _rows =
            new();

    private int _selectedQuestId;

    private class QuestViewData
    {
        public int QuestId { get; }

        public string Name { get; }

        public string Description { get; }

        public string Objective { get; }

        public bool CanAbandon { get; }

        public QuestViewData(
            int questId,
            string name,
            string description,
            string objective,
            bool canAbandon)
        {
            QuestId =
                questId;

            Name =
                name;

            Description =
                description;

            Objective =
                objective;

            CanAbandon =
                canAbandon;
        }
    }

    private void Awake()
    {
        Instance =
            this;

        questRowTemplate.gameObject.SetActive(
            false);

        ClearDetails();

        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(
            true);
    }

    public void Hide()
    {
        gameObject.SetActive(
            false);
    }

    public void AddOrUpdateQuest(
        int questId,
        string questName,
        string description,
        string objective,
        bool canAbandon)
    {
        QuestViewData quest =
            new(
                questId,
                questName,
                description,
                objective,
                canAbandon);

        _quests[questId] =
            quest;

        if (_rows.TryGetValue(
                questId,
                out QuestRowUI existingRow))
        {
            existingRow.SetData(
                questId,
                questName,
                this);
        }
        else
        {
            Button rowButton =
                Instantiate(
                    questRowTemplate,
                    questListContent);

            rowButton.gameObject.SetActive(
                true);

            QuestRowUI row =
                rowButton.GetComponent<QuestRowUI>();

            if (row == null)
            {
                Debug.LogError(
                    "QuestRowTemplate is missing QuestRowUI.");

                Destroy(
                    rowButton.gameObject);

                return;
            }

            row.SetData(
                questId,
                questName,
                this);

            _rows.Add(
                questId,
                row);
        }

        if (_selectedQuestId ==
            questId)
        {
            SelectQuest(
                questId);
        }
    }
    public void AbandonSelectedQuest()
    {
        if (_selectedQuestId <= 0)
            return;

        int questId =
            _selectedQuestId;

        ConfirmationWindow.Instance?.Show(
            () =>
            {
                GameServerConnection.Instance?
                    .SendQuestAbandonRequest(
                        questId);
            });
    }
    public void RemoveQuest(
        int questId)
    {
        _quests.Remove(
            questId);

        if (_rows.TryGetValue(
                questId,
                out QuestRowUI row))
        {
            Destroy(
                row.gameObject);

            _rows.Remove(
                questId);
        }

        if (_selectedQuestId ==
            questId)
        {
            ClearDetails();
        }
    }

    public void ClearQuests()
    {
        foreach (QuestRowUI row
            in _rows.Values)
        {
            if (row != null)
            {
                Destroy(
                    row.gameObject);
            }
        }

        _rows.Clear();
        _quests.Clear();

        ClearDetails();
    }

    public void SelectQuest(
        int questId)
    {
        if (!_quests.TryGetValue(
                questId,
                out QuestViewData quest))
        {
            return;
        }

        _selectedQuestId =
            questId;

        questNameText.text =
            quest.Name;

        descriptionText.text =
            quest.Description;

        objectiveText.text =
            "Current Objective:\n\n" +
            quest.Objective;

        abandonQuestButton.interactable =
            quest.CanAbandon;
    }

    public void ClearDetails()
    {
        _selectedQuestId =
            0;

        questNameText.text =
            "Select a Quest";

        descriptionText.text =
            string.Empty;

        objectiveText.text =
            "Current Objective:\n\n" +
            "No objective selected.";

        abandonQuestButton.interactable =
            false;
    }
}