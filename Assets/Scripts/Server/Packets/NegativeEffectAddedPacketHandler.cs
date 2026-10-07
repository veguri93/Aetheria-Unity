using System.Text;
using UnityEngine;

public static class NegativeEffectAddedPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(
                parts[0],
                out int skillId) ||
            !int.TryParse(
                parts[1],
                out int durationMilliseconds))
        {
            return;
        }

        if (skillId <= 0 ||
            durationMilliseconds <= 0)
        {
            return;
        }

        BuffBarUI buffBar =
            Object.FindAnyObjectByType<BuffBarUI>();

        if (buffBar == null)
            return;

        buffBar.AddOrRefreshNegativeEffect(
            skillId,
            durationMilliseconds);
    }
}