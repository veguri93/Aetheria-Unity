using System.Text;

public static class PlayerRespawnPromptPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        if (data == "Village")
        {
            RespawnWindow.Instance?.Show();
        }
    }
}