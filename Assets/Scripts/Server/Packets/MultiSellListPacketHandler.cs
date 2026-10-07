using System.Text;
using UnityEngine;

public static class MultiSellListPacketHandler
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
                out int npcObjectId) ||
            !int.TryParse(
                parts[1],
                out int multiSellId))
        {
            return;
        }

        if (MultiSellWindow.Instance == null)
        {
            Debug.LogWarning(
                "MultiSellWindow instance was not found.");

            return;
        }

        MultiSellWindow.Instance.Show(
            npcObjectId,
            multiSellId,
            parts[2]);
    }
}