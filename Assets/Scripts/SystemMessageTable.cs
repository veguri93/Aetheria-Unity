using System.Collections.Generic;

public static class SystemMessageTable
{
    private static readonly Dictionary<int, SystemMessage> Messages =
        new()
        {
            {
                1,
                new SystemMessage(
                    1,
                    "YOU_HAVE_EQUIPPED_$1",
                    "You have equipped $1.")
            },
            {
                2,
                new SystemMessage(
                    2,
                    "WELCOME_TO_AETHERIA",
                    "Welcome to Aetheria!")
            },
            {
                 3,
                 new SystemMessage(
                  3,
                   "YOU_DEALT_$1_DAMAGE_TO_$2",
                   "You dealt $1 damage to $2.")
            },
            {
    4,
    new SystemMessage(
        4,
        "YOU_HAVE_UNEQUIPPED_$1",
        "You have unequipped $1.")
},
            {
    5,
    new SystemMessage(
        5,
        "YOU_HAVE_GAINED_$1_EXPERIENCE",
        "You have gained $1 experience.")
},
{
    6,
    new SystemMessage(
        6,
        "YOUR_LEVEL_HAS_INCREASED_TO_$1",
        "Your level has increased to $1.")
},
{
    7,
    new SystemMessage(
        7,
        "YOU_ARE_NOW_AFFECTED_BY_$1",
        "You are now affected by $1.")
},
{
    8,
    new SystemMessage(
        8,
        "YOU_ARE_NO_LONGER_AFFECTED_BY_$1",
        "You are no longer affected by $1.")
            },

            {
                9,
                new SystemMessage(
                    9,
                    "YOU_HAVE_PICKED_UP_$1",
                    "You have picked up $1.")
            },

            {
                10,
                new SystemMessage(
                    10,
                    "YOU_HAVE_FAILED_TO_PICK_UP_$1",
                    "You have failed to pick up $1.")
            },
            {
    11,
    new SystemMessage(
        11,
        "YOU_HAVE_USED_$1_SKILL",
        "You have used $1.")
},

{
    12,
    new SystemMessage(
        12,
        "$1_IS_ON_COOLDOWN",
        "$1 is on cooldown.")
},

{
    13,
    new SystemMessage(
        13,
        "NOT_ENOUGH_MP",
        "Not enough MP.")
}
        };

    public static SystemMessage Get(
        int messageId)
    {
        return Messages.TryGetValue(
            messageId,
            out SystemMessage message)
            ? message
            : null;
    }
}