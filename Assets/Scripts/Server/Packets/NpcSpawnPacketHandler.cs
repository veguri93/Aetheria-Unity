using System.Text;
using UnityEngine;

public static class NpcSpawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        string[] parts =
            data.Split('|');

        if (parts.Length != 6)
            return;

        if (!int.TryParse(
                parts[0],
                out int objectId) ||
            !int.TryParse(
                parts[1],
                out int templateId) ||
            !float.TryParse(
                parts[2],
                out float x) ||
            !float.TryParse(
                parts[3],
                out float y) ||
            !float.TryParse(
                parts[4],
                out float z) ||
            !float.TryParse(
                parts[5],
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

        GameObject npcObject =
            Object.Instantiate(
                template.Prefab,
                new Vector3(
                    x,
                    y,
                    z),
                Quaternion.Euler(
                    0f,
                    rotation,
                    0f));

        Npc npc =
            npcObject.GetComponent<Npc>();

        if (npc == null)
        {
            Debug.LogWarning(
                $"NPC prefab {template.Name} does not have an Npc component.");

            Object.Destroy(
                npcObject);

            return;
        }

        npc.Initialize(
            objectId,
            templateId,
            template.Name,
            template.Type);
    }
}