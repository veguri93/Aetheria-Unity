using System;
using System.Globalization;
using System.Text;
using UnityEngine;

// Both spawn packet types share this envelope. The packet type remains the behavior boundary.
public sealed class NpcSpawnData
{
    public int ObjectId, TemplateId, VisualTemplateId;
    public NpcType Type;
    public string Name, Title;
    public Vector3 Position;
    public Quaternion Rotation;

    public static bool TryParse(ClientPacket packet, bool hostile, NpcTemplateManager registry, out NpcSpawnData spawn)
    {
        spawn = null;
        var p = Encoding.UTF8.GetString(packet.Data).Split('|');
        if (p.Length < 6 || p.Length > 10 ||
            !int.TryParse(p[0], out int objectId) || objectId <= 0 ||
            !int.TryParse(p[1], out int templateId) || templateId <= 0) return false;
        var coords = new float[4];
        for (int i = 0; i < coords.Length; i++)
            if (!float.TryParse(p[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out coords[i]) ||
                float.IsNaN(coords[i]) || float.IsInfinity(coords[i])) return false;
        registry.TryGetTemplate(templateId, out var legacy);
        NpcType type = hostile ? NpcType.Monster : legacy?.Type ?? NpcType.Citizen;
        if (p.Length >= 9 && (!Enum.TryParse(p[8], out type) || !Enum.IsDefined(typeof(NpcType), type))) return false;
        if (NpcTemplateManager.IsHostile(type) != hostile) return false;
        int visualId = templateId;
        if (p.Length == 10 && (!int.TryParse(p[9], out visualId) || visualId <= 0)) return false;
        spawn = new NpcSpawnData
        {
            ObjectId = objectId, TemplateId = templateId, VisualTemplateId = visualId, Type = type,
            Name = p.Length >= 7 ? Uri.UnescapeDataString(p[6]) : legacy?.Name ?? "NPC",
            Title = p.Length >= 8 ? Uri.UnescapeDataString(p[7]) : "",
            Position = new Vector3(coords[0], coords[1], coords[2]), Rotation = Quaternion.Euler(0, coords[3], 0)
        };
        return true;
    }
}
