using System.Text;
using UnityEngine;

public static class GroundItemDespawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        if (!int.TryParse(
                data,
                out int objectId))
        {
            Debug.LogWarning(
                $"[GROUND ITEM] Invalid despawn packet: {data}");

            return;
        }

        GroundItemManager.Instance?.Despawn(
            objectId);
    }
}