using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>Read-only Character Information view of existing authoritative client snapshots.</summary>
public sealed class CharacterInfoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameValue,guildValue,levelValue,cpValue,hpValue,mpValue;
    [SerializeField] private TMP_Text mightValue,graceValue,aetherValue,resolveValue;
    [SerializeField] private TMP_Text physicalAttackValue,magicAttackValue,physicalDefenseValue,magicDefenseValue,attackSpeedValue,castingSpeedValue;
    [SerializeField] private TMP_Text accuracyValue,evasionValue,criticalRateValue,movementValue,pvpValue,pkValue;
    private PlayerStatsUI stats;
    private PlayerProgressUI progress;
    private LocalPlayer player;
    private void OnEnable()
    {
        stats=GetComponentInParent<PlayerStatsUI>();
        if(!stats)stats=FindAnyObjectByType<PlayerStatsUI>(FindObjectsInactive.Include);
        progress=FindAnyObjectByType<PlayerProgressUI>(FindObjectsInactive.Include);
        player=FindAnyObjectByType<LocalPlayer>(FindObjectsInactive.Include);
        if(stats)stats.Changed+=Refresh;
        if(progress)progress.Changed+=Refresh;
        if(player)player.GuildChanged+=OnGuild;
        Refresh();
    }
    private void OnDisable()
    {
        if(stats)stats.Changed-=Refresh;
        if(progress)progress.Changed-=Refresh;
        if(player)player.GuildChanged-=OnGuild;
    }
    private void OnGuild(string value)=>Refresh();
    public void Close()=>gameObject.SetActive(false);
    private static string Number(int? value)=>value.HasValue?value.Value.ToString(CultureInfo.InvariantCulture):"—";
    private static string Rating(double? value)=>value.HasValue?value.Value.ToString("F1",CultureInfo.InvariantCulture):"—";
    private static string Resource(bool known,int current,int maximum)=>known?current.ToString(CultureInfo.InvariantCulture)+" / "+maximum.ToString(CultureInfo.InvariantCulture):"— / —";
    private void Refresh()
    {
        if(!nameValue)return;
        string name=progress?progress.PlayerName:player?player.Name:string.Empty;
        nameValue.text=string.IsNullOrEmpty(name)?"—":name;
        guildValue.text=player&&!string.IsNullOrEmpty(player.GuildName)?player.GuildName:"—";
        levelValue.text=Number(progress?progress.Level:null);
        cpValue.text=Resource(stats&&stats.HasCombatPoints,stats?stats.CurrentCp:0,stats?stats.MaxCp:0);
        hpValue.text=Resource(stats&&stats.HasHealth,stats?stats.CurrentHp:0,stats?stats.MaxHp:0);
        mpValue.text=Resource(stats&&stats.HasMana,stats?stats.CurrentMp:0,stats?stats.MaxMp:0);
        mightValue.text=Number(stats?stats.Might:null);graceValue.text=Number(stats?stats.Grace:null);aetherValue.text=Number(stats?stats.Aether:null);resolveValue.text=Number(stats?stats.Resolve:null);
        bool known=stats&&stats.HasStats;
        physicalAttackValue.text=Number(known?stats.PhysicalAttack:(int?)null);magicAttackValue.text=Number(known?stats.MagicAttack:(int?)null);
        physicalDefenseValue.text=Number(known?stats.PhysicalDefense:(int?)null);magicDefenseValue.text=Number(known?stats.MagicDefense:(int?)null);
        attackSpeedValue.text=Number(known?stats.AttackSpeed:(int?)null);castingSpeedValue.text=Number(known?stats.CastingSpeed:(int?)null);
        accuracyValue.text=Rating(stats?stats.Accuracy:null);
        evasionValue.text=Rating(stats?stats.Evasion:null);
        // No authoritative final values exist for these fields yet. Never substitute template or client movement values.
        criticalRateValue.text=movementValue.text=pvpValue.text=pkValue.text="—";
    }
}
