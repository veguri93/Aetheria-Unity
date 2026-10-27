using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ShortcutSlotFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Image frame;
    private bool hovered;
    public static void Apply(SkillShortcutSlotUI slot,Image icon,Image cooldown,TMP_Text key)
    {
        var feedback=slot.gameObject.GetComponent<ShortcutSlotFeedback>()??slot.gameObject.AddComponent<ShortcutSlotFeedback>();
        feedback.frame=slot.GetComponent<Image>();feedback.frame.sprite=null;feedback.frame.color=ShortcutBarChrome.Bronze;
        var well=ShortcutBarChrome.Box(slot.transform,"Inset charcoal",Vector2.zero,Vector2.one,Vector2.one,new Color(.065f,.075f,.095f));
        well.transform.SetAsFirstSibling();
        foreach(var image in new[]{icon,cooldown})if(image){image.rectTransform.offsetMin=new Vector2(3,3);image.rectTransform.offsetMax=new Vector2(-3,-3);image.raycastTarget=false;image.preserveAspect=true;}
        if(key)
        {
            key.fontSize=10;key.enableAutoSizing=false;key.color=new Color(.94f,.84f,.63f);key.raycastTarget=false;
            var r=key.rectTransform;r.anchorMin=r.anchorMax=Vector2.zero;r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(3,2);r.sizeDelta=new Vector2(30,13);
            key.alignment=TextAlignmentOptions.BottomLeft;key.transform.SetAsLastSibling();
            var shadow=key.gameObject.GetComponent<Shadow>()??key.gameObject.AddComponent<Shadow>();shadow.effectColor=Color.black;shadow.effectDistance=new Vector2(1,-1);
        }
    }
    public void OnPointerEnter(PointerEventData e){hovered=true;frame.color=new Color(.76f,.61f,.35f);}
    public void OnPointerExit(PointerEventData e){hovered=false;frame.color=ShortcutBarChrome.Bronze;}
    public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)frame.color=new Color(1,.83f,.48f);}
    public void OnPointerUp(PointerEventData e){frame.color=hovered?new Color(.76f,.61f,.35f):ShortcutBarChrome.Bronze;}
}
