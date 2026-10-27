using UnityEngine;
using UnityEngine.UI;

// Decoration only. The grip routes dragging to the bar's PersistentUIPosition;
// slot drag handlers continue to own assignment gestures.
public sealed class ShortcutBarChrome : MonoBehaviour
{
    public static readonly Color Bronze = new Color(.44f,.34f,.20f,1);
    public static Image Box(Transform parent,string name,Vector2 min,Vector2 max,Vector2 inset,Color color,bool raycast=false)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        var rect=(RectTransform)go.transform;rect.SetParent(parent,false);
        rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=inset;rect.offsetMax=-inset;
        var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=raycast;return image;
    }
    private void Awake()
    {
        Box(transform,"Bronze rim",Vector2.zero,Vector2.one,Vector2.zero,Bronze);
        Box(transform,"Charcoal backing",Vector2.zero,Vector2.one,Vector2.one,new Color(.035f,.045f,.063f,.97f));
        var grip=Box(transform,"Drag handle",new Vector2(.5f,1),new Vector2(.5f,1),Vector2.zero,new Color(.08f,.09f,.11f),true);
        var rect=grip.rectTransform;rect.pivot=new Vector2(.5f,1);rect.sizeDelta=new Vector2(68,12);rect.anchoredPosition=new Vector2(0,-2);
        for(int i=0;i<3;i++)
        {
            var line=Box(grip.transform,"Grip ridge",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,Bronze);
            line.rectTransform.sizeDelta=new Vector2(26,1);line.rectTransform.anchoredPosition=new Vector2(0,i*3-3);
        }
    }
}
