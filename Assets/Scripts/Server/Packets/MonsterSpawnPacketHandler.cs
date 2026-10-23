using UnityEngine;

public static class MonsterSpawnPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        var registry = NpcTemplateManager.Instance;
        if (!registry || !NpcSpawnData.TryParse(packet, true, registry, out var data)) return;
        var entity = registry.CreateEntity(data.TemplateId, data.VisualTemplateId, data.Type, data.Position, data.Rotation);
        if (!entity) return;
        var monster = entity.GetComponent<Monster>();
        if (!monster || entity.GetComponentInChildren<Npc>(true))
        {
            Debug.LogError("Monster spawn rejected: incompatible behavior components.");
            Object.Destroy(entity); return;
        }
        monster.SetTemplateIdentity(data.TemplateId, data.VisualTemplateId, data.Type);
        monster.SetObjectId(data.ObjectId);
        entity.SetActive(true);
        monster.SetNpcName(data.Name, data.Title);
        MonsterManager.Instance?.AddMonster(monster);
    }
}
