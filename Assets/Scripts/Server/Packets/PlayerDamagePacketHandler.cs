using System.Text;
using UnityEngine;

public static class PlayerDamagePacketHandler
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
            !int.TryParse(
                parts[1],
                out int templateId) ||
            !int.TryParse(
                parts[2],
                out int damage) ||
            !int.TryParse(
                parts[3],
                out int currentHp) ||
            !int.TryParse(
                parts[4],
                out int maxHp))
        {
            return;
        }

        PlayerHealth health =
            Object.FindAnyObjectByType<PlayerHealth>();

        if (health == null)
            return;

        health.SetServerHealth(
            currentHp,
            maxHp);

        PlayerStatsUI statsUI =
            Object.FindAnyObjectByType<PlayerStatsUI>();

        statsUI?.SetHealth(
            currentHp,
            maxHp);

        string npcName =
            "Unknown NPC";

        if (NpcTemplateManager.Instance != null &&
            NpcTemplateManager.Instance.TryGetTemplate(
                templateId,
                out NpcTemplate template))
        {
            npcName =
                template.Name;
        }

        ChatManager.Instance?.AddCombatMessage(
            GameMessageType.DamageTaken,
            $"{npcName} dealt {damage} damage to you.");
    }
}