using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Cosmetic hover tint only; existing slot handlers own every item interaction.</summary>
[RequireComponent(typeof(Image))]
public sealed class InventorySlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color restingColor=new Color(.065f,.071f,.083f,1);
    [SerializeField] private Color hoverColor=new Color(.145f,.132f,.105f,1);
    [SerializeField,Min(0)] private float transitionSeconds=.14f;
    private Image background;
    private Coroutine transition;
    private void Awake(){background=GetComponent<Image>();}
    public void OnPointerEnter(PointerEventData data)=>Tint(hoverColor);
    public void OnPointerExit(PointerEventData data)=>Tint(restingColor);
    private void Tint(Color color)
    {
        if(!background)background=GetComponent<Image>();
        if(transition!=null)StopCoroutine(transition);
        if(!isActiveAndEnabled||transitionSeconds<=0){background.color=color;transition=null;return;}
        transition=StartCoroutine(Blend(color));
    }
    private IEnumerator Blend(Color target)
    {
        Color from=background.color;
        for(float elapsed=0;elapsed<transitionSeconds;elapsed+=Time.unscaledDeltaTime)
        {background.color=Color.Lerp(from,target,elapsed/transitionSeconds);yield return null;}
        background.color=target;transition=null;
    }
    private void OnDisable(){if(transition!=null)StopCoroutine(transition);transition=null;if(background)background.color=restingColor;}
}
