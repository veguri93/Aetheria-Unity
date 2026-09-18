using System;
using System.Collections.Generic;

public static class ClientSkillBook
{
    private static readonly HashSet<int> _ownedSkillIds =
        new();

    public static event Action Changed;

    public static bool HasSkill(
        int skillId)
    {
        return _ownedSkillIds.Contains(
            skillId);
    }

    public static IReadOnlyCollection<int> GetOwnedSkillIds()
    {
        return _ownedSkillIds;
    }

    public static void SetSkills(
        IEnumerable<int> skillIds)
    {
        _ownedSkillIds.Clear();

        foreach (int skillId in skillIds)
        {
            if (skillId > 0)
            {
                _ownedSkillIds.Add(
                    skillId);
            }
        }

        Changed?.Invoke();
    }

    public static void Clear()
    {
        _ownedSkillIds.Clear();

        Changed?.Invoke();
    }
}