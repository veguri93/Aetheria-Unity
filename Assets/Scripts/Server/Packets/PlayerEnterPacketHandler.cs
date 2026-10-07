using System.Text;
using UnityEngine;

public static class PlayerEnterPacketHandler
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
                6,
                System.StringSplitOptions.None);

        if (parts.Length != 6)
            return;

        if (!int.TryParse(
                parts[0],
                out int playerId))
        {
            return;
        }

        string name =
            parts[1];

        string title =
            parts[2];

        if (!float.TryParse(
                parts[3],
                out float x) ||
            !float.TryParse(
                parts[4],
                out float y) ||
            !float.TryParse(
                parts[5],
                out float z))
        {
            return;
        }

        Vector3 position =
            new Vector3(
                x,
                y,
                z);

        PlayerManager.Instance?
            .AddPlayer(
                playerId,
                name,
                title,
                position);
    }
}