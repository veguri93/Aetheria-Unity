using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    private PlayerMovement _movement;
    private PlayerCombat _combat;

    private SkillShortcutBarUI _shortcutBar;
    [SerializeField]
    private SkillDatabase skillDatabase;

    private WorldItemDrop pendingPickup;

    private const float PickupRange = 2f;

    private void Awake()
    {
        _movement =
            GetComponent<PlayerMovement>();

        _combat =
            GetComponent<PlayerCombat>();

        _shortcutBar =
            FindAnyObjectByType<SkillShortcutBarUI>();
    }

    private void Update()
    {
        HandleShortcutHotkeys();

        if (pendingPickup == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                pendingPickup.transform.position);

        if (distance > PickupRange)
            return;

        TryPickup(
            pendingPickup);
    }

    private void HandleShortcutHotkeys()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
            ActivateShortcut(1);
        else if (Keyboard.current.f2Key.wasPressedThisFrame)
            ActivateShortcut(2);
        else if (Keyboard.current.f3Key.wasPressedThisFrame)
            ActivateShortcut(3);
        else if (Keyboard.current.f4Key.wasPressedThisFrame)
            ActivateShortcut(4);
        else if (Keyboard.current.f5Key.wasPressedThisFrame)
            ActivateShortcut(5);
        else if (Keyboard.current.f6Key.wasPressedThisFrame)
            ActivateShortcut(6);
        else if (Keyboard.current.f7Key.wasPressedThisFrame)
            ActivateShortcut(7);
        else if (Keyboard.current.f8Key.wasPressedThisFrame)
            ActivateShortcut(8);
        else if (Keyboard.current.f9Key.wasPressedThisFrame)
            ActivateShortcut(9);
        else if (Keyboard.current.f10Key.wasPressedThisFrame)
            ActivateShortcut(10);
        else if (Keyboard.current.f11Key.wasPressedThisFrame)
            ActivateShortcut(11);
        else if (Keyboard.current.f12Key.wasPressedThisFrame)
            ActivateShortcut(12);
    }

    public void ActivateShortcut(
        int slotNumber)
    {
        if (_shortcutBar == null)
            return;

        if (!_shortcutBar.TryGetShortcut(
                slotNumber,
                out ShortcutType shortcutType,
                out int referenceId))
        {
            return;
        }

        switch (shortcutType)
        {
            case ShortcutType.Skill:
                ActivateSkillShortcut(
                    slotNumber,
                    referenceId);
                break;

            case ShortcutType.Item:
                ActivateItemShortcut(
                    slotNumber,
                    referenceId);
                break;
        }
    }

    private void ActivateSkillShortcut(
        int slotNumber,
        int skillId)
    {
        if (skillDatabase == null)
        {
            Debug.LogWarning(
                "[SKILL] SkillDatabase is not assigned.");

            return;
        }

        if (!skillDatabase.TryGetSkill(
                skillId,
                out SkillClientData skill))
        {
            Debug.LogWarning(
                $"[SHORTCUT] F{slotNumber} skill failed: " +
                $"SkillId={skillId} was not found.");

            return;
        }

        switch (skill.TargetType)
        {
            case SkillClientTargetType.SELF:
                {
                    GameServerConnection.Instance?
                        .SendSkillUseRequest(
                            skillId,
                            "Self",
                            0,
                            false);

                    break;
                }

            case SkillClientTargetType.ONE:
                {
                    if (TargetManager.Instance == null)
                        return;

                    Monster target =
                        TargetManager.Instance.CurrentTarget;

                    if (target == null)
                    {
                        Debug.LogWarning(
                            $"[SHORTCUT] F{slotNumber} skill failed: " +
                            $"no monster targeted.");

                        return;
                    }

                    GameServerConnection.Instance?
                        .SendSkillUseRequest(
                            skillId,
                            "Monster",
                            target.ObjectId,
                            false);

                    break;
                }

            default:
                {
                    Debug.LogWarning(
                        $"[SHORTCUT] F{slotNumber} target type " +
                        $"{skill.TargetType} is not implemented yet.");

                    break;
                }
        }
    }
    public void ActivateSkillFromWindow(
    int skillId)
    {
        ActivateSkillShortcut(
            0,
            skillId);
    }
    private void ActivateItemShortcut(
      int slotNumber,
      int itemId)
    {
        if (!ClientInventory.TryGetItemByItemId(
                itemId,
                out ClientItemInstance item))
        {
            Debug.LogWarning(
                $"[SHORTCUT] F{slotNumber} item failed: " +
                $"ItemId={itemId} is not in inventory.");

            return;
        }

        if (item.IsEquipped)
        {
            GameServerConnection.Instance?
                .SendUnequipItemRequest(
                    item.ObjectId);

            return;
        }

        GameServerConnection.Instance?
            .SendItemActionRequest(
                item.ObjectId);
    }

    public void OnGroundClicked(
        Vector3 position)
    {
        pendingPickup = null;

        if (_combat.IsAttackLocked)
        {
            _combat.StopAttack(
                position);

            return;
        }

        _combat.StopAttack();

        _movement.MoveTo(
            position);
    }

    public void OnMonsterClicked(
        Monster monster)
    {
        pendingPickup = null;

        if (TargetManager.Instance.CurrentTarget !=
            monster)
        {
            TargetManager.Instance.SetTarget(
                monster);

            return;
        }

        _combat.StartAttack(
            monster);
    }

    public void OnWorldItemClicked(
        WorldItemDrop worldItem)
    {
        if (worldItem == null)
            return;

        pendingPickup =
            worldItem;

        float distance =
            Vector3.Distance(
                transform.position,
                worldItem.transform.position);



        if (distance <= PickupRange)
        {
            TryPickup(
                worldItem);

            return;
        }


        _combat.StopAttack();

        _movement.MoveTo(
            worldItem.transform.position);
    }

    private void TryPickup(
        WorldItemDrop worldItem)
    {
        if (worldItem == null)
        {
            pendingPickup = null;
            return;
        }

        int objectId =
            worldItem.ObjectId;

        pendingPickup = null;

        _movement.Stop();



        GameServerConnection.Instance?
            .SendPickupItemRequest(
                objectId);
    }
}