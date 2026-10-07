using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffBarUI : MonoBehaviour
{
    [SerializeField]
    private SkillDatabase _skillDatabase;

    [SerializeField]
    private BuffSlotUI _buffSlotPrefab;

    [SerializeField]
    private Transform _buffContainer;

    [SerializeField]
    private Transform _negativeContainer;

    private readonly Dictionary<int, BuffSlotUI>
        _activeBuffSlots =
            new();

    private readonly Dictionary<int, BuffSlotUI>
        _activeNegativeSlots =
            new();

    private LayoutElement _buffLayoutElement;
    private LayoutElement _negativeLayoutElement;

    private const int SlotsPerRow =
        10;

    private const float SlotSize =
        32f;

    private const float RowSpacing =
        3f;

    private void Awake()
    {
        if (_buffContainer != null)
        {
            _buffLayoutElement =
                _buffContainer.GetComponent<LayoutElement>();
        }

        if (_negativeContainer != null)
        {
            _negativeLayoutElement =
                _negativeContainer.GetComponent<LayoutElement>();
        }

        UpdateLayoutHeights();
    }

    public void AddOrRefreshBuff(
        int skillId,
        int durationMilliseconds)
    {
        if (_activeBuffSlots.TryGetValue(
                skillId,
                out BuffSlotUI existingSlot))
        {
            existingSlot.Refresh(
                durationMilliseconds);

            return;
        }

        BuffSlotUI newSlot =
            CreateSlot(
                skillId,
                durationMilliseconds,
                _buffContainer);

        if (newSlot == null)
            return;

        _activeBuffSlots.Add(
            skillId,
            newSlot);

        UpdateLayoutHeights();
    }

    public void RemoveBuff(
        int skillId)
    {
        if (!_activeBuffSlots.TryGetValue(
                skillId,
                out BuffSlotUI slot))
        {
            return;
        }

        _activeBuffSlots.Remove(
            skillId);

        Destroy(
            slot.gameObject);

        UpdateLayoutHeights();
    }

    public void AddOrRefreshNegativeEffect(
        int skillId,
        int durationMilliseconds)
    {
        if (_activeNegativeSlots.TryGetValue(
                skillId,
                out BuffSlotUI existingSlot))
        {
            existingSlot.Refresh(
                durationMilliseconds);

            return;
        }

        BuffSlotUI newSlot =
            CreateSlot(
                skillId,
                durationMilliseconds,
                _negativeContainer);

        if (newSlot == null)
            return;

        _activeNegativeSlots.Add(
            skillId,
            newSlot);

        UpdateLayoutHeights();
    }

    public void RemoveNegativeEffect(
        int skillId)
    {
        if (!_activeNegativeSlots.TryGetValue(
                skillId,
                out BuffSlotUI slot))
        {
            return;
        }

        _activeNegativeSlots.Remove(
            skillId);

        Destroy(
            slot.gameObject);

        UpdateLayoutHeights();
    }

    private BuffSlotUI CreateSlot(
        int skillId,
        int durationMilliseconds,
        Transform container)
    {
        if (_skillDatabase == null ||
            _buffSlotPrefab == null ||
            container == null)
        {
            return null;
        }

        if (!_skillDatabase.TryGetSkill(
                skillId,
                out SkillClientData skill))
        {
            Debug.LogWarning(
                $"BuffBarUI could not find skill {skillId}.");

            return null;
        }

        BuffSlotUI newSlot =
            Instantiate(
                _buffSlotPrefab,
                container);

        newSlot.Initialize(
            skillId,
            skill.Icon,
            durationMilliseconds);

        return newSlot;
    }

    private void UpdateLayoutHeights()
    {
        UpdateLayoutHeight(
            _buffLayoutElement,
            _activeBuffSlots.Count);

        UpdateLayoutHeight(
            _negativeLayoutElement,
            _activeNegativeSlots.Count);
    }

    private static void UpdateLayoutHeight(
        LayoutElement layoutElement,
        int slotCount)
    {
        if (layoutElement == null)
            return;

        if (slotCount <= 0)
        {
            layoutElement.preferredHeight =
                0f;

            return;
        }

        int rows =
            Mathf.CeilToInt(
                slotCount /
                (float)SlotsPerRow);

        layoutElement.preferredHeight =
            (rows * SlotSize) +
            ((rows - 1) * RowSpacing);
    }
}