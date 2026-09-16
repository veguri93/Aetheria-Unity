using System.Collections.Generic;
using UnityEngine;

public class SkillShortcutBarUI : MonoBehaviour
{
    [SerializeField]
    private SkillDatabase skillDatabase;

    [SerializeField]
    private ItemVisualDatabase itemVisualDatabase;

    [SerializeField]
    private SkillShortcutSlotUI slotPrefab;

    [SerializeField]
    private Transform slotContainer;

    private readonly List<SkillShortcutSlotUI> slots =
        new();

    private const int SlotCount = 12;

    private void Start()
    {
        EnsureSlotsCreated();
    }

    private void EnsureSlotsCreated()
    {
        if (slots.Count > 0)
            return;

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

    public void ClearAllShortcuts()
    {
        EnsureSlotsCreated();

        foreach (SkillShortcutSlotUI slot in slots)
        {
            slot.ClearShortcut();
        }
    }

    public void LoadShortcut(
        int slotNumber,
        ShortcutType shortcutType,
        int referenceId)
    {
        EnsureSlotsCreated();

        if (slotNumber < 1 ||
            slotNumber > slots.Count)
        {
            return;
        }

        switch (shortcutType)
        {
            case ShortcutType.Skill:
                SetSkill(
                    slotNumber,
                    referenceId);
                break;

            case ShortcutType.Item:
                SetItem(
                    slotNumber,
                    referenceId);
                break;
        }
    }

    public void SetSkill(
        int slotNumber,
        int skillId)
    {
        EnsureSlotsCreated();

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

    public void SetItem(
        int slotNumber,
        int itemId)
    {
        EnsureSlotsCreated();

        if (slotNumber < 1 ||
            slotNumber > slots.Count)
        {
            return;
        }

        if (!itemVisualDatabase.TryGet(
                itemId,
                out ItemVisualDefinition definition))
        {
            Debug.LogWarning(
                $"Item {itemId} was not found in ItemVisualDatabase.");

            return;
        }

        slots[slotNumber - 1].SetItem(
            itemId,
            definition.Icon);
    }

    public bool TryGetShortcut(
        int slotNumber,
        out ShortcutType shortcutType,
        out int referenceId)
    {
        EnsureSlotsCreated();

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