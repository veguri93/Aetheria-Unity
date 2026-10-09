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
},
{
    14,
    new SystemMessage(
        14,
        "YOU_HAVE_ACQUIRED_$1",
        "You have acquired $1.")
},

{
    15,
    new SystemMessage(
        15,
        "PURCHASE_FAILED_NOT_ENOUGH_RESOURCES",
        "Purchase failed. You do not have enough required items.")
},
{
    16,
    new SystemMessage(
        16,
        "CURRENT_LOCATION_X_$1_Y_$2_Z_$3",
        "Location: X=$1 Y=$2 Z=$3.")
},
{
    17,
    new SystemMessage(
        17,
        "YOU_DO_NOT_HAVE_ADMIN_ACCESS",
        "You do not have permission to use admin commands.")
},
{
    18,
    new SystemMessage(
        18,
        "YOU_HAVE_RECEIVED_$1_HP",
        "You have received $1 HP.")
},
{
    19,
    new SystemMessage(
        19,
        "$1_HIT_YOU_FOR_$2_DAMAGE",
        "$1 hit you for $2 damage.")
},

{
    20,
    new SystemMessage(
        20,
        "$1_IS_NOW_AFFECTED_BY_$2",
        "$1 is now affected by $2.")
},
{
    21,
    new SystemMessage(
        21,
        "YOU_ARE_NOW_$1",
        "You are now a $1. May your strength guide your path.")
},
{
    22,
    new SystemMessage(
        22,
        "YOU_HAVE_LEARNED_$1",
        "You have learned $1.")
},
{
    23,
    new SystemMessage(
        23,
        "YOU_HAVE_CONSUMED_$1",
        "You have consumed $1.")
},
{
    24,
    new SystemMessage(
        24,
        "YOU_HAVE_CREATED_GUILD_$1",
        "You have created the guild $1.")
},
{
    25,
    new SystemMessage(
        25,
        "YOU_ARE_ALREADY_IN_A_GUILD",
        "You are already a member of a guild.")
},
{
    26,
    new SystemMessage(
        26,
        "YOU_DO_NOT_MEET_GUILD_CREATION_REQUIREMENTS",
        "You do not meet the requirements to create a guild.")
},
{
    27,
    new SystemMessage(
        27,
        "GUILD_NAME_$1_IS_ALREADY_TAKEN",
        "The guild name $1 is already taken.")
},
{
    28,
    new SystemMessage(
        28,
        "YOU_HAVE_INVITED_$1_TO_THE_GUILD",
        "You have invited $1 to the guild.")
},
{
    29,
    new SystemMessage(
        29,
        "YOU_HAVE_RECEIVED_A_GUILD_INVITATION_FROM_$1",
        "You have received an invitation from the guild $1.")
},
{
    30,
    new SystemMessage(
        30,
        "$1_DID_NOT_ACCEPT_YOUR_GUILD_INVITATION",
        "$1 did not accept your guild invitation.")
},
{
    31,
    new SystemMessage(
        31,
        "YOU_HAVE_REJECTED_THE_GUILD_INVITATION",
        "You have rejected the guild invitation.")
},
{
    32,
    new SystemMessage(
        32,
        "YOU_HAVE_JOINED_THE_GUILD_$1",
        "You have joined the guild $1.")
},
{
    33,
    new SystemMessage(
        33,
        "$1_HAS_JOINED_THE_GUILD",
        "$1 has joined the guild.")
},
{
    34,
    new SystemMessage(
        34,
        "YOU_HAVE_BEEN_PROMOTED_TO_$1",
        "You have been promoted to $1.")
},
{
    35,
    new SystemMessage(
        35,
        "YOU_HAVE_PROMOTED_$1_TO_$2",
        "You have promoted $1 to $2.")
},
{
    36,
    new SystemMessage(
        36,
        "$1_IS_NOW_$2",
        "$1 is now $2.")
},
{
    37,
    new SystemMessage(
        37,
        "$1_IS_NEW_CLAN_LEADER",
        "$1 is the new clan leader!")
},
{
    38,
    new SystemMessage(
        38,
        "YOU_HAVE_LEFT_THE_CLAN",
        "You have left the clan.")
},
{
    39,
    new SystemMessage(
        39,
        "$1_HAS_LEFT_THE_CLAN",
        "$1 has left the clan.")
},
{
    40,
    new SystemMessage(
        40,
        "YOU_HAVE_BEEN_DISMISSED_FROM_THE_CLAN",
        "You have been dismissed from the clan.")
},
{
    41,
    new SystemMessage(
        41,
        "YOU_HAVE_DISMISSED_$1_FROM_THE_CLAN",
        "You have dismissed $1 from the clan.")
},
{
    42,
    new SystemMessage(
        42,
        "$1_HAS_BEEN_DISMISSED_FROM_THE_CLAN",
        "$1 has been dismissed from the clan.")
},
{
    43,
    new SystemMessage(
        43,
        "$1_REJECTED_PARTY_INVITATION",
        "$1 rejected the party invitation.")
},
{
    44,
    new SystemMessage(
        44,
        "$1_JOINED_THE_PARTY",
        "$1 joined the party.")
},
{
    45,
    new SystemMessage(
        45,
        "$1_LEFT_THE_PARTY",
        "$1 left the party.")
},
{
    46,
    new SystemMessage(
        46,
        "YOU_HAVE_LEFT_THE_PARTY",
        "You have left the party.")
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