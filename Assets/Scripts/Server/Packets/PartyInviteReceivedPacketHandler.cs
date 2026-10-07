using System.Text;

public static class PartyInviteReceivedPacketHandler
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
                2,
                System.StringSplitOptions.None);

        if (parts.Length != 2)
            return;

        string inviterName =
            parts[0];

        if (!int.TryParse(
                parts[1],
                out int lootMode))
        {
            return;
        }

        PartyInviteReceiveWindow.Instance?
            .Show(
                inviterName,
                lootMode);
    }
}