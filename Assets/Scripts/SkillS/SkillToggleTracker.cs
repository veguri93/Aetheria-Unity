using System.Collections.Generic;

public static class SkillToggleTracker
{
    private static readonly HashSet<int>
        ActiveSkillIds =
            new();

    public static void SetActive(
        int skillId,
        bool isActive)
    {
        if (skillId <= 0)
            return;

        if (isActive)
        {
            ActiveSkillIds.Add(
                skillId);
        }
        else
        {
            ActiveSkillIds.Remove(
                skillId);
        }
    }

    public static bool IsActive(
        int skillId)
    {
        return ActiveSkillIds.Contains(
            skillId);
    }

    public static void Clear()
    {
        ActiveSkillIds.Clear();
    }
}