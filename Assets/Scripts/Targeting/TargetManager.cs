using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance { get; private set; }

    public Monster CurrentTarget { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetTarget(Monster monster)
    {
        if (CurrentTarget == monster)
            return;

        if (CurrentTarget != null)
            CurrentTarget.Deselect();

        CurrentTarget = monster;

        if (CurrentTarget != null)
            CurrentTarget.Select();

      
    }

    public void ClearTarget()
    {
        if (CurrentTarget != null)
            CurrentTarget.Deselect();

        CurrentTarget = null;

    }
}