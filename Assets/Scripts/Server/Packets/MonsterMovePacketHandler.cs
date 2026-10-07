using System.Text;
using UnityEngine;

public static class MonsterMovePacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 5)
            return;

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !float.TryParse(
                parts[1],
                out float x) ||
            !float.TryParse(
                parts[2],
                out float y) ||
            !float.TryParse(
                parts[3],
                out float z) ||
            !float.TryParse(
                parts[4],
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
            return;
        }

        monster.SetServerMovement(
            new Vector3(
                x,
                y,
                z),
            rotation);
    }
}