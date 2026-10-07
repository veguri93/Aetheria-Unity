using System.Text;
using UnityEngine;

public static class NpcDespawnPacketHandler
{
    public static void Handle(
        ClientPacket packet)
    {
        string data =
            Encoding.UTF8.GetString(
                packet.Data);

        if (!int.TryParse(
                data,
                out int objectId) ||
            objectId <= 0)
        {
            return;
        }

        if (TargetManager.Instance != null &&
            TargetManager.Instance.CurrentTarget?.ObjectId ==
            objectId)
        {
            TargetManager.Instance.ClearTarget();
        }

        if (MonsterManager.Instance != null &&
            MonsterManager.Instance.RemoveMonster(
                objectId,
                out Monster monster) &&
            monster != null)
        {
            Object.Destroy(
                monster.gameObject);

            return;
        }

        Npc[] npcs =
            Object.FindObjectsByType<Npc>();

        foreach (Npc npc in npcs)
        {
            if (npc.ObjectId != objectId)
                continue;

            Object.Destroy(
                npc.gameObject);

            return;
        }
    }
}