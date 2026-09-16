using System.Collections.Generic;
using UnityEngine;

public class SkillShortcutBarUI : MonoBehaviour
{
    [SerializeField]
    private SkillDatabase skillDatabase;

    [SerializeField]
    private SkillShortcutSlotUI slotPrefab;

    [SerializeField]
    private Transform slotContainer;

    private readonly List<SkillShortcutSlotUI> slots =
        new();

    private const int SlotCount = 12;

    private void Start()
    {
        CreateSlots();
    }

    private void CreateSlots()
    {
        for (int i = 1;
             i <= SlotCount;
             i++)
        {
            SkillShortcutSlotUI slot =
                Instantiate(
                    slotPrefab,
                    slotContainer);

            slot.Initialize(
                i);

            slots.Add(
                slot);
        }
    }

    public void SetSkill(
        int slotNumber,
        int skillId)
    {
        if (slotNumber < 1 ||
            slotNumber > slots.Count)
        {
            return;
        }

        if (!skillDatabase.TryGetSkill(
                skillId,
                out SkillClientData skill))
        {
            Debug.LogWarning(
                $"Skill {skillId} was not found in SkillDatabase.");

            return;
        }

        slots[slotNumber - 1].SetSkill(
            skill);
    }

    public bool TryGetShortcut(
        int slotNumber,
        out ShortcutType shortcutType,
        out int referenceId)
    {
        shortcutType =
            ShortcutType.None;

        referenceId =
            0;

        if (slotNumber < 1 ||
            slotNumber > slots.Count)
        {
            return false;
        }

        SkillShortcutSlotUI slot =
            slots[slotNumber - 1];

        if (!slot.HasShortcut)
            return false;

        shortcutType =
            slot.ShortcutType;

        referenceId =
            slot.ReferenceId;

        return true;
    }

    // Temporary compatibility with our current
    // PlayerInteraction code.
    public int GetSkillId(
        int slotNumber)
    {
        if (!TryGetShortcut(
                slotNumber,
                out ShortcutType shortcutType,
                out int referenceId))
        {
            return 0;
        }

        if (shortcutType != ShortcutType.Skill)
            return 0;

        return referenceId;
    }
}