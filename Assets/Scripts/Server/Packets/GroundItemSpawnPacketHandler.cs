using System.Text;
using UnityEngine;

public static class GroundItemSpawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 6)
        {
            Debug.LogWarning(
                $"[GROUND ITEM] Invalid spawn packet: {data}");

            return;
        }

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !int.TryParse(
                parts[1],
                out int itemId) ||
            !int.TryParse(
                parts[2],
                out int quantity) ||
            !float.TryParse(
                parts[3],
                out float x) ||
            !float.TryParse(
                parts[4],
                out float y) ||
            !float.TryParse(
                parts[5],
                out float z))
        {
            Debug.LogWarning(
                $"[GROUND ITEM] Could not parse spawn packet: {data}");

            return;
        }

        GroundItemManager.Instance?.Spawn(
            objectId,
            itemId,
            quantity,
            new Vector3(
                x,
                y,
                z));
    }
}