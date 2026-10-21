using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Additive visual selection only; SkillEntryUI still handles activation, tooltip and shortcut dragging.</summary>
[RequireComponent(typeof(SkillEntryUI))]
public sealed class SkillCodexEntryView : MonoBehaviour,IPointerClickHandler,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
{
    [SerializeField] private InventorySelectionGlow selectionGlow;
    private SkillEntryUI entry;
    private SkillCodexPresentation codex;
    private Coroutine press;
    private void Awake(){entry=GetComponent<SkillEntryUI>();codex=GetComponentInParent<SkillCodexPresentation>();}
    public void RefreshSelection()
    {
        if(!entry)entry=GetComponent<SkillEntryUI>();if(!codex)codex=GetComponentInParent<SkillCodexPresentation>();
        if(selectionGlow)selectionGlow.gameObject.SetActive(entry.HasSkill&&codex&&entry.SkillId==codex.SelectedSkillId);
    }
    public void OnPointerClick(PointerEventData data){if(data.button==PointerEventData.InputButton.Left&&entry.HasSkill)codex?.Select(entry.Skill);}
    public void OnPointerDown(PointerEventData data){if(entry.HasSkill&&data.button==PointerEventData.InputButton.Left)Animate(.96f);}
    public void OnPointerUp(PointerEventData data)=>Animate(1);
    public void OnPointerExit(PointerEventData data)=>Animate(1);
    private void Animate(float target){if(press!=null)StopCoroutine(press);if(isActiveAndEnabled)press=StartCoroutine(Scale(target));}
    private IEnumerator Scale(float target){var from=transform.localScale;for(float t=0;t<.12f;t+=Time.unscaledDeltaTime){transform.localScale=Vector3.Lerp(from,Vector3.one*target,t/.12f);yield return null;}transform.localScale=Vector3.one*target;press=null;}
    private void OnDisable(){if(press!=null)StopCoroutine(press);press=null;transform.localScale=Vector3.one;}
}
