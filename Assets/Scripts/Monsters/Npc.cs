using UnityEngine;

public class Npc : MonoBehaviour, ITargetable
{
    [SerializeField]
    private GameObject _selectionCircle;

    private int _objectId;
    private int _templateId;
    private string _name;
    private NpcType _type;

    public int ObjectId =>
        _objectId;

    public int TemplateId =>
        _templateId;

    public string Name =>
        _name;

    public NpcType Type =>
        _type;

    private void Awake()
    {
        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(
                false);
        }
    }

    public void Initialize(
        int objectId,
        int templateId,
        string npcName,
        NpcType npcType)
    {
        _objectId =
            objectId;

        _templateId =
            templateId;

        _name =
            npcName;

        _type =
            npcType;
    }

    public void Select()
    {
        if (_selectionCircle == null)
        {


            return;
        }

        _selectionCircle.SetActive(
            true);

    }

    public void Deselect()
    {


        if (_selectionCircle != null)
        {
            _selectionCircle.SetActive(
                false);
        }
    }
}