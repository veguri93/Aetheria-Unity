
using System.Text;
using UnityEngine;

public static class AttackResultPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        string[] parts =
            Encoding.UTF8.GetString(packet.Data).Split('|');

        if (parts.Length != 4)
            return;

        string result = parts[0];

        if (result != "ACCEPTED" &&
            result != "COOLDOWN" &&
            result != "REJECTED")
            return;

        if (!int.TryParse(parts[1], out int objectId) ||
            !int.TryParse(parts[2], out int milliseconds) ||
            !long.TryParse(parts[3], out long requestId))
            return;

        PlayerCombat combat =
            LocalPlayer.Instance?
                .GetComponent<PlayerCombat>();

        combat?.OnAttackResult(
            objectId,
            result,
            milliseconds,
            requestId);
    }
}
