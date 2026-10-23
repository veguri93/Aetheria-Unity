using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class NpcVisualTemplate
{
    public int Id;
    public GameObject Prefab;
}

public class NpcTemplateManager : MonoBehaviour
{
    public static NpcTemplateManager Instance { get; private set; }
    [SerializeField] private List<NpcTemplate> _templates = new();
    [Tooltip("Optional model-only registry. Existing NPC prefab IDs also remain valid visual IDs.")]
    [SerializeField] private List<NpcVisualTemplate> _visualTemplates = new();
    readonly Dictionary<int, NpcTemplate> _templateMap = new();
    readonly Dictionary<int, GameObject> _visualMap = new();
    readonly Dictionary<GameObject, GameObject> _appearanceCache = new();
    GameObject _inactiveRoot;

    void Awake() { Instance = this; InitializeRegistry(); }
    public void InitializeRegistry()
    {
        _templateMap.Clear(); _visualMap.Clear();
        foreach (var template in _templates)
        {
            if (template == null) continue;
            _templateMap[template.Id] = template;
            if (template.Prefab) _visualMap[template.Id] = template.Prefab;
        }
        foreach (var visual in _visualTemplates)
            if (visual != null && visual.Id > 0 && visual.Prefab) _visualMap[visual.Id] = visual.Prefab;
    }
    public bool TryGetTemplate(int id, out NpcTemplate template) => _templateMap.TryGetValue(id, out template);
    public bool TryGetVisual(int id, out GameObject prefab) => _visualMap.TryGetValue(id, out prefab);
    public bool TryGetFriendlyVisual(NpcType type, out NpcTemplate template)
    {
        template = _templates.Find(t => t != null && t.Type == type && t.Prefab && t.Prefab.GetComponent<Npc>())
            ?? _templates.Find(t => t != null && t.Type == NpcType.Citizen && t.Prefab && t.Prefab.GetComponent<Npc>())
            ?? _templates.Find(t => t != null && t.Type == NpcType.Merchant && t.Prefab && t.Prefab.GetComponent<Npc>());
        return template != null && !IsHostile(type);
    }
    public static bool IsHostile(NpcType type) => type == NpcType.Monster || type == NpcType.RaidBoss || type == NpcType.GrandBoss;

    // Returns an inactive entity so handlers can validate its components before activation.
    public GameObject CreateEntity(int templateId, int visualId, NpcType type, Vector3 position, Quaternion rotation)
    {
        bool hostile = IsHostile(type);
        _templateMap.TryGetValue(templateId, out var identity);
        if (!TryGetVisual(visualId, out var visual))
        {
            Debug.LogWarning($"Visual template {visualId} is not registered for NPC {templateId}; trying its existing fallback.");
            if (identity != null && identity.Prefab) visual = identity.Prefab;
            else if (!hostile && TryGetFriendlyVisual(type, out var fallback)) visual = fallback.Prefab;
            else return null;
        }
        if (!_inactiveRoot)
        {
            _inactiveRoot = new GameObject("NPC appearance cache");
            _inactiveRoot.SetActive(false);
            _inactiveRoot.transform.SetParent(transform, false);
        }
        // Keep the original prefab exactly as-is for ordinary existing entities of the matching behavior.
        bool original = identity != null && visual == identity.Prefab &&
            (hostile ? visual.GetComponent<Monster>() != null && visual.GetComponent<Npc>() == null
                     : visual.GetComponent<Npc>() != null && visual.GetComponentInChildren<Monster>(true) == null);
        GameObject entity;
        if (original) entity = Instantiate(visual, _inactiveRoot.transform, false);
        else
        {
            if (!_appearanceCache.TryGetValue(visual, out var model))
            {
                model = NpcAppearance.Build(visual, _inactiveRoot.transform);
                _appearanceCache.Add(visual, model);
            }
            entity = Instantiate(model, _inactiveRoot.transform, false);
            NpcAppearance.AddInteractionShape(entity, visual);
            // Presentation and behavior come from an existing shell of the correct category, not the model.
            GameObject shell = null;
            if (hostile)
            {
                shell = identity?.Prefab && identity.Prefab.GetComponent<Monster>() ? identity.Prefab
                    : visual.GetComponent<Monster>() ? visual
                    : _templates.FirstOrDefault(t => t != null && t.Prefab && t.Prefab.GetComponent<Monster>())?.Prefab;
            }
            else if (TryGetFriendlyVisual(type, out var friendly)) shell = friendly.Prefab;
            if (!shell)
            {
                Debug.LogWarning($"No registered {(hostile ? "monster" : "friendly NPC")} behavior shell for {templateId}.");
                Destroy(entity); return null;
            }
            if (hostile)
            {
                entity.AddComponent<Monster>().CopySpawnSettings(shell.GetComponent<Monster>());
                var ai = shell.GetComponent<MonsterAI>();
                if (ai) entity.AddComponent<MonsterAI>().CopySpawnSettings(ai);
            }
            else
            {
                entity.AddComponent<Npc>();
                // Static NPC position is server-owned; a reused walking clip must not move its root.
                foreach (var animator in entity.GetComponentsInChildren<Animator>(true)) animator.applyRootMotion = false;
            }
            var plate = shell.GetComponent<WorldNameplate>();
            if (plate) WorldNameplate.AddPresentation(entity, plate);
            entity.layer = shell.layer;
        }
        entity.SetActive(false);
        entity.transform.SetParent(null, false);
        entity.transform.localScale = visual.transform.localScale;
        entity.transform.SetPositionAndRotation(position, rotation);
        return entity;
    }
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        _appearanceCache.Clear();
        // Cache objects are children and are destroyed with this manager.
    }
}
