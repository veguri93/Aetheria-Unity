public static class GuildLeaveSuccessPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        GuildWindow.Instance?.Hide();
    }
}