using System;
using System.Text;

public static class ChatMessagePacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        string data = Encoding.UTF8.GetString(packet.Data);

        string[] parts = data.Split(
            '|',
            4,
            StringSplitOptions.None);

        if (parts.Length != 4)
            return;

        string senderName = parts[2];
        string message = parts[3];

        if (!Enum.TryParse(parts[0], out ChatType type) ||
            !Enum.TryParse(parts[1], out ChatScope scope))
            return;

        // Keep normal chat working.
        ChatManager.Instance?.AddMessage(
            type,
            scope,
            senderName,
            message);

        // Only local general chat appears above players.
        if (type != ChatType.General ||
            scope != ChatScope.Local)
            return;

        // Our own character.
        LocalPlayer local = LocalPlayer.Instance;

        if (local != null &&
            string.Equals(
                local.Name,
                senderName,
                StringComparison.OrdinalIgnoreCase))
        {
            ShowBubble(local.gameObject, message);
            return;
        }

        // Other nearby players.
        if (PlayerManager.Instance != null &&
            PlayerManager.Instance.TryGetPlayerByName(
                senderName,
                out RemotePlayer remotePlayer))
        {
            ShowBubble(remotePlayer.gameObject, message);
        }
    }

    private static void ShowBubble(
        UnityEngine.GameObject player,
        string message)
    {
        OverheadChatBubble bubble =
            player.GetComponent<OverheadChatBubble>();

        if (bubble == null)
            bubble = player.AddComponent<OverheadChatBubble>();

        bubble.ShowMessage(message);
    }
}