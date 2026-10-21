using System.Text;
using UnityEngine;

public static class CharacterStatsPacketHandler
{
    public static void Handle(ClientPacket packet)
    {
        string[] parts=Encoding.UTF8.GetString(packet.Data).Split('|');
        // Preserve both legacy prefixes. Extended server packet appends final attributes and current resources.
        if(parts.Length!=7&&parts.Length!=9&&parts.Length!=16)return;
        var values=new int[7];
        for(int i=0;i<7;i++)if(!int.TryParse(parts[i],out values[i]))return;
        int currentCp=0,maxCp=0,currentHp=-1,currentMp=-1,maxMp=0;
        if(parts.Length>=9&&(!int.TryParse(parts[7],out currentCp)||!int.TryParse(parts[8],out maxCp)))return;
        int? might=null,grace=null,aether=null,resolve=null;
        if(parts.Length==16)
        {
            int?[] attributes=new int?[4];
            for(int i=0;i<4;i++){if(int.TryParse(parts[9+i],out int attribute))attributes[i]=attribute;else if(parts[9+i]!="—")return;}
            might=attributes[0];grace=attributes[1];aether=attributes[2];resolve=attributes[3];
            if(!int.TryParse(parts[13],out currentHp)||!int.TryParse(parts[14],out currentMp)||!int.TryParse(parts[15],out maxMp))return;
        }
        var ui=Object.FindAnyObjectByType<PlayerStatsUI>();if(!ui)return;
        ui.SetStats(values[0],values[1],values[2],values[3],values[4],values[5],values[6]);
        ui.SetEffectiveAttributes(might,grace,aether,resolve);
        if(parts.Length>=9)ui.SetCombatPoints(currentCp,maxCp);
        if(parts.Length==16){if(currentHp>=0)ui.SetHealth(currentHp,values[0]);if(currentMp>=0)ui.SetMana(currentMp,maxMp);}
    }
}
