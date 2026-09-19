using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance { get; private set; }

    public ITargetable CurrentTarget { get; private set; }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetTarget(
        ITargetable target)
    {
        if (ReferenceEquals(
               CurrentTarget,
               target))
        {
            return;
        }

        if (CurrentTarget != null)
        {
            CurrentTarget.Deselect();
        }

        CurrentTarget =
            target;

        if (CurrentTarget != null)
        {
            CurrentTarget.Select();
        }
    }

    public void ClearTarget()
    {
        if (CurrentTarget != null)
        {
            CurrentTarget.Deselect();
        }

        CurrentTarget =
            null;
    }
}