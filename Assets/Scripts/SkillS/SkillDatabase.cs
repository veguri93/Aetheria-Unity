using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillDatabase",
    menuName = "Aetheria/Skills/Skill Database")]
public class SkillDatabase : ScriptableObject
{
    [SerializeField]
    private List<SkillClientData> skills =
        new();

    public IReadOnlyList<SkillClientData> Skills =>
        skills;

    public bool TryGetSkill(
        int skillId,
        out SkillClientData skill)
    {
        skill =
            skills.Find(
                entry =>
                    entry.SkillId == skillId);

        return skill != null;
    }
}

[Serializable]
public class SkillClientData
{
    [SerializeField]
    private int skillId;

    [SerializeField]
    private string displayName;

    [TextArea(2, 5)]
    [SerializeField]
    private string description;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private SkillClientTargetType targetType;
    [SerializeField]
    private SkillClientOperateType operateType;

    public SkillClientOperateType OperateType =>
        operateType;
    public int SkillId =>
        skillId;

    public string DisplayName =>
        displayName;

    public string Description =>
        description;

    public Sprite Icon =>
        icon;

    public SkillClientTargetType TargetType =>
        targetType;
}