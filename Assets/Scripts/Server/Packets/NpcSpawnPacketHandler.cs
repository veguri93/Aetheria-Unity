
using System;
using System.Text;
using System.Globalization;
using UnityEngine;

public static class NpcSpawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts =
            data.Split('|');

        // Supported formats:
        // 6 fields: Legacy packet
        // 7 fields: NPC name
        // 8 fields: NPC name + optional title
        if (parts.Length < 6 || parts.Length > 8)
            return;

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !int.TryParse(
                parts[1],
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

        NpcTemplateManager manager =
            NpcTemplateManager.Instance;

        if (manager == null)
            return;

        if (!manager.TryGetTemplate(
                templateId,
                out NpcTemplate template))
        {
            Debug.LogWarning(
                $"Unknown NPC template: {templateId}");

            return;
        }

        // Use the server-provided name when available.
        // Fall back to the local template for legacy packets.
        string npcName =
            parts.Length >= 7
                ? Uri.UnescapeDataString(parts[6])
                : template.Name;

        string npcTitle =
    parts.Length >= 8
        ? Uri.UnescapeDataString(parts[7])
        : string.Empty;

        GameObject npcObject =
            UnityEngine.Object.Instantiate(
                template.Prefab,
                new Vector3(x, y, z),
                Quaternion.Euler(0f, rotation, 0f));

        Npc npc =
            npcObject.GetComponent<Npc>();

        if (npc == null)
        {
            Debug.LogWarning(
                $"NPC prefab {template.Name} does not have an Npc component.");

            UnityEngine.Object.Destroy(npcObject);
            return;
        }

        npc.Initialize(
            objectId,
            templateId,
            npcName,
            template.Type,
            npcTitle);
    }
}
