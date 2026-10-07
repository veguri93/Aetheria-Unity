using System.Text;
using UnityEngine;

public static class NpcHtmlPacketHandler
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
                3);

        if (parts.Length != 3)
            return;

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !int.TryParse(
                parts[1],
                out int templateId))
        {
            return;
        }

        string html;

        try
        {
            byte[] htmlBytes =
                System.Convert.FromBase64String(
                    parts[2]);

            html =
                Encoding.UTF8.GetString(
                    htmlBytes);
        }
        catch
        {
            Debug.LogWarning(
                $"Invalid NPC HTML packet for NPC {templateId}.");

            return;
        }

        if (NpcHtmlWindow.Instance == null)
        {
            Debug.LogWarning(
                "NpcHtmlWindow instance was not found.");

            return;
        }

        NpcHtmlWindow.Instance.Show(
            objectId,
            templateId,
            html);
    }
}