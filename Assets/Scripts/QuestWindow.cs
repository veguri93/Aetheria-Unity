using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Quest journal presentation. Quest data and abandon authority remain on the server.</summary>
public class QuestWindow : MonoBehaviour
{
    public static QuestWindow Instance { get; private set; }
    [Header("Quest List")]
    [SerializeField] private Transform questListContent;
    [SerializeField] private Button questRowTemplate;
    [SerializeField] private TMP_Text emptyStateText;
    [SerializeField] private TMP_Text questCountText;
    [Header("Quest Details")]
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private Button abandonQuestButton;
    [SerializeField] private ScrollRect detailsScroll;
    [SerializeField] private GameObject descriptionHeading;
    [SerializeField] private GameObject objectiveHeading;
    [SerializeField] private GameObject objectiveDivider;
    private readonly Dictionary<int, QuestViewData> _quests = new();
    private readonly Dictionary<int, QuestRowUI> _rows = new();
    private int _selectedQuestId;
    public int SelectedQuestId => _selectedQuestId;
    private sealed class QuestViewData
    {
        public string Name, Description, Objective;
        public bool CanAbandon;
    }
    private void Awake()
    {
        Instance=this;
        questRowTemplate.gameObject.SetActive(false);
        ClearDetails(); UpdateListState(); Hide();
    }
    public void Show(){gameObject.SetActive(true);transform.SetAsLastSibling();}
    public void Hide(){gameObject.SetActive(false);}
    public void AddOrUpdateQuest(int questId,string questName,string description,string objective,bool canAbandon)
    {
        _quests[questId]=new QuestViewData{Name=questName,Description=description,Objective=objective,CanAbandon=canAbandon};
        if(!_rows.TryGetValue(questId,out var row))
        {
            var rowButton=Instantiate(questRowTemplate,questListContent);
            row=rowButton.GetComponent<QuestRowUI>();
            if(!row){Debug.LogError("QuestRowTemplate is missing QuestRowUI.");Destroy(rowButton.gameObject);return;}
            _rows.Add(questId,row);rowButton.gameObject.SetActive(true);
        }
        row.SetData(questId,questName,this);row.SetSelected(questId==_selectedQuestId);
        UpdateListState();
        if(_selectedQuestId==questId)Present(_quests[questId],false);
    }
    public void AbandonSelectedQuest()
    {
        int id=_selectedQuestId;
        if(!_quests.TryGetValue(id,out var quest)||!quest.CanAbandon)return;
        var confirmation=ConfirmationWindow.Instance;
        if(!confirmation)return;
        confirmation.Show(()=>
        {
            // Recheck after confirmation: a snapshot may have removed or changed this quest.
            if(_quests.TryGetValue(id,out var current)&&current.CanAbandon)
                GameServerConnection.Instance?.SendQuestAbandonRequest(id);
        });
        confirmation.transform.SetAsLastSibling();
    }
    public void RemoveQuest(int questId)
    {
        _quests.Remove(questId);
        if(_rows.Remove(questId,out var row)&&row){row.gameObject.SetActive(false);Destroy(row.gameObject);}
        if(_selectedQuestId==questId)ClearDetails();
        UpdateListState();
    }
    public void ClearQuests()
    {
        foreach(var row in _rows.Values)if(row){row.gameObject.SetActive(false);Destroy(row.gameObject);}
        _rows.Clear();_quests.Clear();ClearDetails();UpdateListState();
    }
    public void SelectQuest(int questId)
    {
        if(!_quests.TryGetValue(questId,out var quest))return;
        bool changed=_selectedQuestId!=questId;_selectedQuestId=questId;
        foreach(var pair in _rows)pair.Value.SetSelected(pair.Key==questId);
        Present(quest,changed);
    }
    private void Present(QuestViewData quest,bool resetScroll)
    {
        questNameText.text=quest.Name??string.Empty;
        descriptionText.text=quest.Description??string.Empty;
        objectiveText.text=quest.Objective??string.Empty;
        bool description=!string.IsNullOrWhiteSpace(quest.Description),objective=!string.IsNullOrWhiteSpace(quest.Objective);
        descriptionText.gameObject.SetActive(description);objectiveText.gameObject.SetActive(objective);
        if(descriptionHeading)descriptionHeading.SetActive(description);
        if(objectiveHeading)objectiveHeading.SetActive(objective);
        if(objectiveDivider)objectiveDivider.SetActive(description&&objective);
        abandonQuestButton.interactable=quest.CanAbandon;
        if(detailsScroll){LayoutRebuilder.MarkLayoutForRebuild(detailsScroll.content);if(resetScroll){detailsScroll.StopMovement();detailsScroll.verticalNormalizedPosition=1;}}
    }
    public void ClearDetails()
    {
        _selectedQuestId=0;
        foreach(var row in _rows.Values)if(row)row.SetSelected(false);
        questNameText.text="Select a quest";descriptionText.text="Choose a quest from the journal to view its story and current objective.";descriptionText.gameObject.SetActive(true);
        objectiveText.text=string.Empty;objectiveText.gameObject.SetActive(false);
        if(descriptionHeading)descriptionHeading.SetActive(false);
        if(objectiveHeading)objectiveHeading.SetActive(false);
        if(objectiveDivider)objectiveDivider.SetActive(false);
        abandonQuestButton.interactable=false;
        if(detailsScroll){detailsScroll.StopMovement();detailsScroll.verticalNormalizedPosition=1;}
    }
    private void UpdateListState()
    {
        if(emptyStateText)emptyStateText.gameObject.SetActive(_quests.Count==0);
        if(questCountText)questCountText.SetText(_quests.Count==1 ? "1 active quest" : "{0} active quests",_quests.Count);
    }
    private void OnDestroy(){if(Instance==this)Instance=null;}
}
