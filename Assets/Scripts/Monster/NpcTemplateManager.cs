using System.Collections.Generic;
using UnityEngine;

public class NpcTemplateManager : MonoBehaviour
{
    public static NpcTemplateManager Instance { get; private set; }

    [SerializeField]
    private List<NpcTemplate> _templates = new();

    private Dictionary<int, NpcTemplate> _templateMap = new();

    private void Awake()
    {
        Instance = this;

        foreach (NpcTemplate template in _templates)
        {
            _templateMap[template.Id] = template;
        }
    }

    public bool TryGetTemplate(
        int id,
        out NpcTemplate template)
    {
        return _templateMap.TryGetValue(
            id,
            out template);
    }
}