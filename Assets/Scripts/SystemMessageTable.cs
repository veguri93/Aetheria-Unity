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