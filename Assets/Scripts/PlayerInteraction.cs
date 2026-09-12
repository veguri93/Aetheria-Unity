using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInteraction : MonoBehaviour
{
    private PlayerMovement _movement;
    private PlayerCombat _combat;

    private WorldItemDrop pendingPickup;

    private const float PickupRange = 2f;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
    }

    private void Update()
    {
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

    public void OnGroundClicked(Vector3 position)
    {
        pendingPickup = null;

        if (_combat.IsAttackLocked)
        {
            _combat.StopAttack(position);
            return;
        }

        _combat.StopAttack();
        _movement.MoveTo(position);
    }

    public void OnMonsterClicked(Monster monster)
    {
        pendingPickup = null;

        if (TargetManager.Instance.CurrentTarget != monster)
        {
            TargetManager.Instance.SetTarget(monster);
            return;
        }

        _combat.StartAttack(monster);
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

        Debug.Log(
            $"[PICKUP] Item clicked. " +
            $"ObjectId={worldItem.ObjectId}, " +
            $"Distance={distance:F2}");

        if (distance <= PickupRange)
        {
            TryPickup(
                worldItem);

            return;
        }

        Debug.Log(
            "[PICKUP] Item is too far. " +
            "Moving toward it.");

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

        Debug.Log(
            $"[PICKUP] Reached pickup range. " +
            $"Sending request for ObjectId={objectId}");

        GameServerConnection.Instance?
            .SendPickupItemRequest(
                objectId);
    }
}