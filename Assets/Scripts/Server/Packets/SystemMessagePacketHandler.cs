using System.Text;
using UnityEngine;

public static class SystemMessagePacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (!int.TryParse(
                parts[0],
                out int messageId))
        {
            return;
        }

        SystemMessage message =
            SystemMessageTable.Get(
                messageId);

        if (message == null)
        {
            Debug.LogWarning(
                $"Unknown system message ID: {messageId}");

            return;
        }

        string text =
            message.Text;

        for (int i = 1;
             i < parts.Length;
             i++)
        {
            text =
                text.Replace(
                    $"${i}",
                    parts[i]);
        }

        ChatManager.Instance?.AddCombatMessage(
            GameMessageType.System,
            text);
    }
}