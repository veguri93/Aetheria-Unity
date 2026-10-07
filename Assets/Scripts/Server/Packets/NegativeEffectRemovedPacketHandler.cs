using System.Text;
using UnityEngine;

public static class NegativeEffectRemovedPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        if (!int.TryParse(
                data,
                out int skillId))
        {
            return;
        }

        if (skillId <= 0)
            return;

        BuffBarUI buffBar =
            Object.FindAnyObjectByType<BuffBarUI>();

        if (buffBar == null)
            return;

        buffBar.RemoveNegativeEffect(
            skillId);
    }
}