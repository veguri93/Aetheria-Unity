using System.Text;

public static class ChatMessagePacketHandler
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
                4,
                System.StringSplitOptions.None);

        if (parts.Length != 4)
            return;

        string chatType =
            parts[0];

        string chatScope =
            parts[1];

        string senderName =
            parts[2];

        string message =
            parts[3];

        if (ChatManager.Instance == null)
            return;

        if (System.Enum.TryParse(
                chatType,
                out ChatType type) &&
            System.Enum.TryParse(
                chatScope,
                out ChatScope scope))
        {
            ChatManager.Instance.AddMessage(
                type,
                scope,
                senderName,
                message);
        }
    }
}