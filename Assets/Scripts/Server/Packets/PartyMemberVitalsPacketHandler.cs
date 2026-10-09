using System.Text;

public static class PartyMemberVitalsPacketHandler
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

        if (parts.Length != 5)
            return;

        if (!int.TryParse(
                parts[0],
                out int playerId))
        {
            return;
        }

        if (!int.TryParse(
                parts[1],
                out int currentHp))
        {
            return;
        }

        if (!int.TryParse(
                parts[2],
                out int maxHp))
        {
            return;
        }

        if (!int.TryParse(
                parts[3],
                out int currentMp))
        {
            return;
        }

        if (!int.TryParse(
                parts[4],
                out int maxMp))
        {
            return;
        }

        PartyWindow partyWindow =
            PartyWindow.Instance;

        if (partyWindow == null)
            return;

        partyWindow.UpdateMemberVitals(
            playerId,
            currentHp,
            maxHp,
            currentMp,
            maxMp);
    }
}