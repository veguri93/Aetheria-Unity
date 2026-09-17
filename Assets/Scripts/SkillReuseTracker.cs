using System.Collections.Generic;
using UnityEngine;

public static class SkillReuseTracker
{
    private class ReuseState
    {
        public float EndsAt;
        public float Duration;
    }

    private static readonly Dictionary<int, ReuseState>
        reuseStates = new();

    public static void StartReuse(
        int skillId,
        int durationMilliseconds)
    {
        if (skillId <= 0 ||
            durationMilliseconds <= 0)
        {
            return;
        }

        float duration =
            durationMilliseconds / 1000f;

        reuseStates[skillId] =
            new ReuseState
            {
                EndsAt =
                    Time.unscaledTime + duration,

                Duration =
                    duration
            };
    }

    public static bool TryGetReuse(
        int skillId,
        out float remaining,
        out float duration)
    {
        remaining = 0f;
        duration = 0f;

        if (!reuseStates.TryGetValue(
                skillId,
                out ReuseState state))
        {
            return false;
        }

        remaining =
            state.EndsAt -
            Time.unscaledTime;

        duration =
            state.Duration;

        if (remaining <= 0f)
        {
            reuseStates.Remove(
                skillId);

            remaining = 0f;

            return false;
        }

        return true;
    }

    public static float GetNormalizedRemaining(
        int skillId)
    {
        if (!TryGetReuse(
                skillId,
                out float remaining,
                out float duration))
        {
            return 0f;
        }

        if (duration <= 0f)
            return 0f;

        return Mathf.Clamp01(
            remaining / duration);
    }

    public static void Clear()
    {
        reuseStates.Clear();
    }
}