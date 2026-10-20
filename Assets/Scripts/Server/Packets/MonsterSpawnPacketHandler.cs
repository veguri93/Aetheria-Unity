
using System;
using System.Text;
using System.Globalization;
using UnityEngine;

public static class MonsterSpawnPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(packet.Data);

        string[] parts = data.Split('|');

        // 6 = legacy
        // 7 = name
        // 8 = name + optional title
        if (parts.Length < 6 || parts.Length > 8)
            return;

        if (!int.TryParse(parts[0], out int objectId) ||
            !int.TryParse(parts[1], out int templateId) ||
            !float.TryParse(parts[2], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float x) ||
            !float.TryParse(parts[3], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float y) ||
            !float.TryParse(parts[4], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float z) ||
            !float.TryParse(parts[5], NumberStyles.Float,
                CultureInfo.InvariantCulture, out float rotation))
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

        string monsterName =
            parts.Length >= 7
                ? Uri.UnescapeDataString(parts[6])
                : template.Name;

        string monsterTitle =
            parts.Length >= 8
                ? Uri.UnescapeDataString(parts[7])
                : string.Empty;

        GameObject monsterObject =
            UnityEngine.Object.Instantiate(
                template.Prefab,
                new Vector3(x, y, z),
                Quaternion.Euler(0f, rotation, 0f));

        Monster monster =
            monsterObject.GetComponent<Monster>();

        if (monster == null)
        {
            UnityEngine.Object.Destroy(monsterObject);
            return;
        }

        monster.SetNpcName(
            monsterName,
            monsterTitle);

        monster.SetObjectId(objectId);

        MonsterManager.Instance?.AddMonster(monster);
    }
}
