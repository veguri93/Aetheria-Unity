using System.Text;
using UnityEngine;

public static class MonsterDamagePacketHandler
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

        Monster[] monsters =
            Object.FindObjectsByType<Monster>();

        foreach (Monster monster in monsters)
        {
            if (monster.ObjectId != objectId)
                continue;

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

            monster.SetServerHealth(
                currentHp,
                maxHp);

            if (ChatManager.Instance != null)
            {
                SystemMessage message =
                    SystemMessageTable.Get(
                        3);

                if (message != null)
                {
                    string coloredDamage =
                        ChatManager.Instance.ColorText(
                            GameMessageType.DamageDealt,
                            damage.ToString());

                    string text =
                        message.Text
                            .Replace(
                                "$1",
                                coloredDamage)
                            .Replace(
                                "$2",
                                npcName);

                    ChatManager.Instance.AddCombatMessage(
                        GameMessageType.Normal,
                        text);
                }
            }

            return;
        }
    }
}