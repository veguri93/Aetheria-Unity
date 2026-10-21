using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public sealed class MerchantButtonFeedback : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler
{
    private Coroutine motion;
    public void OnPointerDown(PointerEventData e){if(GetComponent<Button>().IsInteractable())Animate(.98f);}
    public void OnPointerUp(PointerEventData e)=>Animate(1);
    public void OnPointerExit(PointerEventData e)=>Animate(1);
    private void Animate(float scale){if(motion!=null)StopCoroutine(motion);if(isActiveAndEnabled)motion=StartCoroutine(Move(scale));}
    private IEnumerator Move(float target){Vector3 from=transform.localScale;for(float t=0;t<.12f;t+=Time.unscaledDeltaTime){transform.localScale=Vector3.Lerp(from,Vector3.one*target,t/.12f);yield return null;}transform.localScale=Vector3.one*target;motion=null;}
    private void OnDisable(){transform.localScale=Vector3.one;motion=null;}
}
