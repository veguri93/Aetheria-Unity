using System.Text;

public static class GuildCreateSuccessPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string guildName =
            Encoding.UTF8.GetString(
                packet.Data);

        if (string.IsNullOrWhiteSpace(
                guildName))
        {
            return;
        }

        GuildCreateWindow.Instance?
            .OnCreateSuccess(
                guildName);
    }
}