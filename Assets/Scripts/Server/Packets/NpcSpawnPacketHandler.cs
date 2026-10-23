using UnityEngine;

public static class NpcSpawnPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        var registry = NpcTemplateManager.Instance;
        if (!registry || !NpcSpawnData.TryParse(packet, false, registry, out var data)) return;
        var entity = registry.CreateEntity(data.TemplateId, data.VisualTemplateId, data.Type, data.Position, data.Rotation);
        if (!entity) return;
        var npc = entity.GetComponent<Npc>();
        if (!npc || entity.GetComponentInChildren<Monster>(true) || entity.GetComponentInChildren<MonsterAI>(true))
        {
            Debug.LogError("Friendly spawn rejected: incompatible behavior components.");
            Object.Destroy(entity); return;
        }
        entity.SetActive(true);
        npc.Initialize(data.ObjectId, data.TemplateId, data.Name, data.Type, data.Title, data.VisualTemplateId);
    }
}
