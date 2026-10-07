using System.Text;

public static class PlayerLeavePacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        if (!int.TryParse(
                data,
                out int playerId))
        {
            return;
        }

        PlayerManager.Instance?
            .RemovePlayer(
                playerId);
    }
}