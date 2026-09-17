using System.Collections.Generic;
using UnityEngine;

public class SkillsWindowUI : MonoBehaviour
{
    [SerializeField]
    private SkillDatabase skillDatabase;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private SkillEntryUI skillEntryPrefab;

    private const int SlotCount = 50;

    private readonly List<int> ownedSkillIds =
        new()
        {
            10001,
            10002,
            10003
        };

    private void Start()
    {
        PopulateSkills();
    }

    private void PopulateSkills()
    {
        for (int i = 0;
             i < SlotCount;
             i++)
        {
            SkillEntryUI entry =
                Instantiate(
                    skillEntryPrefab,
                    content);

            entry.ClearSkill();

            if (i >= ownedSkillIds.Count)
                continue;

            int skillId =
                ownedSkillIds[i];

            if (!skillDatabase.TryGetSkill(
                    skillId,
                    out SkillClientData skill))
            {
                Debug.LogWarning(
                    $"Skill {skillId} was not found in SkillDatabase.");

                continue;
            }

            entry.SetSkill(
                skill);
        }
    }
}