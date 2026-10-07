using System.Text;

public static class PlayerTitleChangedPacketHandler
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

        if (!int.TryParse(
                parts[0],
                out int playerId))
        {
            return;
        }

        string title =
            parts[1];

        LocalPlayer localPlayer =
            LocalPlayer.Instance;

        if (localPlayer != null &&
            localPlayer.PlayerId == playerId)
        {
            localPlayer.SetTitle(
                title);

            return;
        }

        PlayerManager.Instance?
            .UpdatePlayerTitle(
                playerId,
                title);
    }
}