using System.Globalization;
using System.Text;
using UnityEngine;

public static class MonsterRespawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 6 && parts.Length != 10)
            return;

        if (!int.TryParse(
                parts[0],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int objectId) ||
            !int.TryParse(
                parts[1],
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int templateId) ||
            !float.TryParse(
                parts[2],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float x) ||
            !float.TryParse(
                parts[3],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float y) ||
            !float.TryParse(
                parts[4],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float z) ||
            !float.TryParse(
                parts[5],
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float rotation))
        {
            return;
        }

        if (MonsterManager.Instance == null)
            return;

        if (!MonsterManager.Instance.TryGetMonster(
                objectId,
                out Monster monster))
        {
            Debug.LogWarning(
                $"Monster {objectId} not found for respawn.");

            return;
        }

        monster.RespawnFromServer(
            new Vector3(
                x,
                y,
                z),
            rotation);
    }
}