public static class GuildDismissSuccessPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        GuildWindow.Instance?
            .Show();
    }
}