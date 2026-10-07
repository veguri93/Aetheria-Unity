using System.Text;

public static class GuildInviteReceivedPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        int firstSeparator =
            data.IndexOf('|');

        if (firstSeparator < 0)
            return;

        int secondSeparator =
            data.IndexOf(
                '|',
                firstSeparator + 1);

        if (secondSeparator < 0)
            return;

        string inviterName =
            data[..firstSeparator];

        string guildName =
            data[
                (firstSeparator + 1)..secondSeparator];

        string message =
            data[(secondSeparator + 1)..];

        GuildInviteReceiveWindow.Instance?.Show(
            inviterName,
            guildName,
            message);
    }
}