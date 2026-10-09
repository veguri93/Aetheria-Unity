using System.Text;

public static class PartySnapshotPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split(
                '|',
                System.StringSplitOptions.None);

        if (parts.Length < 3)
            return;

        if (!int.TryParse(
                parts[0],
                out int lootMode))
        {
            return;
        }

        if (!int.TryParse(
                parts[1],
                out int leaderSessionId))
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
            3 + memberCount * 2;

        if (parts.Length !=
            expectedParts)
        {
            return;
        }

        PartyWindow partyWindow =
            PartyWindow.Instance;

        if (partyWindow == null)
            return;

        partyWindow.ClearMemberRows();

        if (memberCount <= 1)
        {
            partyWindow.Hide();
            return;
        }

        int index =
            3;

        for (int i = 0;
            i < memberCount;
            i++)
        {
            if (!int.TryParse(
                    parts[index],
                    out int playerId))
            {
                return;
            }

            string playerName =
                parts[index + 1];

            LocalPlayer localPlayer =
                LocalPlayer.Instance;

            if (localPlayer == null ||
                playerId != localPlayer.PlayerId)
            {
                partyWindow.AddMemberRow(
                    playerId,
                    playerName);
            }

            index +=
                2;
        }

        partyWindow.Show();
    }
}