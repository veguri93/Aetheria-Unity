
using UnityEngine;

public class Npc : MonoBehaviour, ITargetable
{

    private int _objectId;
    private int _templateId;
    private string _name = string.Empty;
    private string _title = string.Empty;
    private NpcType _type;

    public int ObjectId =>
        _objectId;

    public int TemplateId =>
        _templateId;

    public string Name =>
        _name;

    public string Title =>
        _title;

    public NpcType Type =>
        _type;

    private void Awake()
    {
        
    }

    public void Initialize(
        int objectId,
        int templateId,
        string npcName,
        NpcType npcType,
        string npcTitle = "")
    {
        _objectId = objectId;
        _templateId = templateId;
        _name = npcName;
        _title = npcTitle ?? string.Empty;
        _type = npcType;

        if (TryGetComponent<WorldNameplate>(out var nameplate))
        {
            nameplate.SetInfo(_name, _title);
        }
    }

    public void Select()
    {
        OverheadTargetIndicator.SetFor(this, TargetIndicatorState.Selected);
    }

    public void Deselect()
    {
        OverheadTargetIndicator.SetFor(this, TargetIndicatorState.None);
    }
}
