using System.Collections.Generic;
using UnityEngine;

public class SkillsWindowUI : MonoBehaviour
{
    [SerializeField]
    private SkillDatabase skillDatabase;

    [SerializeField]
    private Transform activeContent;

    [SerializeField]
    private Transform passiveContent;

    [SerializeField]
    private SkillEntryUI skillEntryPrefab;

    [SerializeField]
    private Transform activeScrollView;

    [SerializeField]
    private Transform passiveScrollView;

    private const int ActiveSlotCount = 50;
    private const int PassiveSlotCount = 50;

    private void OnEnable()
    {
        ClientSkillBook.Changed +=
            PopulateSkills;
    }

    private void OnDisable()
    {
        ClientSkillBook.Changed -=
            PopulateSkills;
    }

    private void Start()
    {
        PopulateSkills();
        ShowActiveSkills();
    }

    private void PopulateSkills()
    {
        List<SkillClientData> activeSkills =
            new();

        List<SkillClientData> passiveSkills =
            new();

        List<int> ownedSkillIds =
            new(
                ClientSkillBook.GetOwnedSkillIds());

        ownedSkillIds.Sort();

        foreach (int skillId in ownedSkillIds)
        {
            if (!skillDatabase.TryGetSkill(
                    skillId,
                    out SkillClientData skill))
            {
                Debug.LogWarning(
                    $"Skill {skillId} was not found in SkillDatabase.");

                continue;
            }

            if (skill.OperateType ==
                SkillClientOperateType.P)
            {
                passiveSkills.Add(
                    skill);
            }
            else
            {
                activeSkills.Add(
                    skill);
            }
        }

        PopulateTable(
            activeContent,
            activeSkills,
            ActiveSlotCount);

        PopulateTable(
            passiveContent,
            passiveSkills,
            PassiveSlotCount);
    }

    public void ShowActiveSkills()
    {
        activeScrollView.gameObject.SetActive(
            true);

        passiveScrollView.gameObject.SetActive(
            false);
    }

    public void ShowPassiveSkills()
    {
        activeScrollView.gameObject.SetActive(
            false);

        passiveScrollView.gameObject.SetActive(
            true);
    }

    private void PopulateTable(
        Transform targetContent,
        List<SkillClientData> skills,
        int slotCount)
    {
        if (targetContent == null)
            return;

        for (int i = targetContent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                targetContent.GetChild(i).gameObject);
        }

        for (int i = 0;
             i < slotCount;
             i++)
        {
            SkillEntryUI entry =
                Instantiate(
                    skillEntryPrefab,
                    targetContent);

            entry.ClearSkill();

            if (i >= skills.Count)
                continue;

            entry.SetSkill(
                skills[i]);
        }
    }
}