using System.Text;

public static class GuildWindowSnapshotPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length < 3)
            return;

        string guildName =
            parts[0];

        if (!int.TryParse(
                parts[1],
                out int currentPlayerRole))
        {
            return;
        }

        if (!int.TryParse(
                parts[2],
                out int memberCount))
        {
            return;
        }

        int expectedParts =
            3 + (memberCount * 4);

        if (parts.Length < expectedParts)
            return;

        if (GuildWindow.Instance == null)
            return;

        GuildWindow.Instance.SetGuildName(
            guildName);

        GuildWindow.Instance.ClearMemberRows();

        GuildWindow.Instance.ResetLeadership();

        bool isLeader =
            currentPlayerRole == 4;

        bool isViceCaptain =
            currentPlayerRole == 3;

        GuildWindow.Instance.SetLeadershipActionsVisible(
            isLeader,
            isViceCaptain);

        int index =
            3;

        for (int i = 0;
             i < memberCount;
             i++)
        {
            if (!int.TryParse(
                    parts[index],
                    out int characterId))
            {
                return;
            }

            string memberName =
                parts[index + 1];

            if (!int.TryParse(
                    parts[index + 2],
                    out int roleValue))
            {
                return;
            }

            bool isOnline =
                parts[index + 3] == "1";

            string roleName =
                roleValue switch
                {
                    0 => "Member",
                    1 => "WarLord",
                    2 => "Diplomat",
                    3 => "Vice Captain",
                    4 => "Leader",
                    _ => "Unknown"
                };

            GuildWindow.Instance.SetLeadershipMember(
                memberName,
                roleValue);

            GuildWindow.Instance.AddMemberRow(
                characterId,
                memberName,
                roleName,
                isOnline);

            index +=
                4;
        }
    }
}