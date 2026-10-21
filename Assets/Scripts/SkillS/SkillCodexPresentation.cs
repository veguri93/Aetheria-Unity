using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Client-side codex presentation. Skill activation, ownership and shortcut handlers stay in existing systems.</summary>
public sealed class SkillCodexPresentation : MonoBehaviour
{
    [SerializeField] private SkillDatabase database;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text metadata;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text collectionCount;
    [SerializeField] private TMP_Text emptyMessage;
    [SerializeField] private Image detailIcon;
    [SerializeField] private Image skillsUnderline;
    [SerializeField] private Image passivesUnderline;
    [SerializeField] private TMP_Text skillsLabel;
    [SerializeField] private TMP_Text passivesLabel;
    [SerializeField] private ScrollRect detailScroll;
    [SerializeField] private LayoutElement detailHeader;
    private bool passivePage;
    public int SelectedSkillId { get; private set; }
    public bool PassivePage => passivePage;
    private void OnEnable()=>Refresh();
    private void OnDisable(){ItemTooltipUI.Instance?.Hide();}
    public void Close()=>gameObject.SetActive(false);
    public void SetPage(bool passive)
    {
        if(passivePage!=passive){passivePage=passive;SelectedSkillId=0;ItemTooltipUI.Instance?.Hide();}
        Refresh();
    }
    public void Select(SkillClientData data)
    {
        if(data==null||!ClientSkillBook.HasSkill(data.SkillId))return;
        SelectedSkillId=data.SkillId;Refresh();
        detailScroll.StopMovement();detailScroll.verticalNormalizedPosition=1;
    }
    public void Refresh()
    {
        if(!database||!skillName)return;
        int count=0;
        foreach(int id in ClientSkillBook.GetOwnedSkillIds())if(database.TryGetSkill(id,out var s)&&(s.OperateType==SkillClientOperateType.P)==passivePage)count++;
        collectionCount.text=count+(passivePage?(count==1?" passive known":" passives known"):(count==1?" skill known":" skills known"));
        emptyMessage.text=passivePage?"No passive skills learned yet.":"No active skills learned yet.";emptyMessage.gameObject.SetActive(count==0);
        skillsUnderline.gameObject.SetActive(!passivePage);passivesUnderline.gameObject.SetActive(passivePage);
        Color lit=new Color(.73f,.80f,1f),muted=new Color(.54f,.57f,.64f);
        skillsLabel.color=passivePage?muted:lit;passivesLabel.color=passivePage?lit:muted;
        SkillClientData selected=null;
        if(SelectedSkillId>0&&ClientSkillBook.HasSkill(SelectedSkillId)&&database.TryGetSkill(SelectedSkillId,out var value)&&(value.OperateType==SkillClientOperateType.P)==passivePage)selected=value;
        if(selected==null)SelectedSkillId=0;
        foreach(var view in GetComponentsInChildren<SkillCodexEntryView>(true))view.RefreshSelection();
        skillName.text=selected!=null?selected.DisplayName:"Your arcane repertoire";
        metadata.text=selected!=null?(passivePage?"PASSIVE":"ACTIVE")+"  /  "+TargetLabel(selected.TargetType):"SKILLS & PASSIVES";
        description.text=selected!=null?selected.Description:"Select a skill to study its inscription.\n\nActive skills keep their existing click-to-use and shortcut-drag behavior.";
        detailIcon.sprite=selected?.Icon;detailIcon.gameObject.SetActive(detailIcon.sprite!=null);
        float nameHeight=Mathf.Max(52,skillName.GetPreferredValues(skillName.text,240,0).y);
        skillName.rectTransform.sizeDelta=new Vector2(240,nameHeight);
        metadata.rectTransform.anchoredPosition=new Vector2(80,-(nameHeight+6));detailHeader.preferredHeight=Mathf.Max(84,nameHeight+30);
        LayoutRebuilder.MarkLayoutForRebuild(detailScroll.content);
    }
    private static string TargetLabel(SkillClientTargetType target)=>target switch
    {
        SkillClientTargetType.SELF=>"Self",SkillClientTargetType.ONE=>"Single target",SkillClientTargetType.AREA=>"Area",SkillClientTargetType.GROUND=>"Ground",
        SkillClientTargetType.PARTY=>"Party",SkillClientTargetType.CLAN=>"Guild",SkillClientTargetType.ALLIANCE=>"Alliance",_=>target.ToString()
    };
}
