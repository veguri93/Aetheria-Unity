using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Attach to the existing drag surface. Companions must share its RectTransform parent.
// Saves placement within available screen space, rather than resolution-specific pixels.
[DisallowMultipleComponent]
public sealed class PersistentUIPosition : MonoBehaviour, IPointerDownHandler,
    IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private string preferenceKey = "Aetheria.UI.PlayerHUD.position.v1";
    [SerializeField] private RectTransform[] companions = Array.Empty<RectTransform>();
    [SerializeField] private bool rightClickResets = true;
    private RectTransform panel, parent;
    private Canvas canvas;
    private RectTransform[] members;
    private Vector2[] defaults;
    private Vector2 pointerOrigin, startPosition, placement;
    private Rect lastScreen, lastParent;
    private float lastScale;
    private bool initialized, hasPlacement, dragging, suppressClick;
    private GameObject shield;
    private int shieldUntil;
    private readonly Vector3[] corners = new Vector3[4];
    public bool SuppressClick => suppressClick || dragging;
    public bool IsDragging => dragging;
    public string PreferenceKey => preferenceKey;
    private Camera EventCamera => canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

    private void Start(){Initialize();RestorePosition();}
    private void Initialize()
    {
        if(initialized)return;
        panel=(RectTransform)transform;parent=panel.parent as RectTransform;
        canvas=GetComponentInParent<Canvas>().rootCanvas;
        var list=new System.Collections.Generic.List<RectTransform>{panel};
        foreach(var companion in companions)
            if(companion&&companion!=panel&&companion.parent==parent&&!list.Contains(companion))list.Add(companion);
        members=list.ToArray();defaults=new Vector2[members.Length];
        for(int i=0;i<members.Length;i++)defaults[i]=members[i].anchoredPosition;
        initialized=true;
    }
    private Rect ScreenRect()
    {
        var safe=Screen.safeArea;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,safe.min,EventCamera,out var min);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,safe.max,EventCamera,out var max);
        return Rect.MinMaxRect(Mathf.Max(parent.rect.xMin,min.x),Mathf.Max(parent.rect.yMin,min.y),
            Mathf.Min(parent.rect.xMax,max.x),Mathf.Min(parent.rect.yMax,max.y));
    }
    private Rect Bounds()
    {
        Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
        foreach(var member in members)
        {
            member.GetWorldCorners(corners);
            foreach(var corner in corners){Vector2 p=parent.InverseTransformPoint(corner);min=Vector2.Min(min,p);max=Vector2.Max(max,p);}
        }
        return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
    }
    private void Translate(Vector2 delta){foreach(var member in members)member.anchoredPosition+=delta;}
    public void ClampToScreen()
    {
        Initialize();var bounds=Bounds();var screen=ScreenRect();
        float x=bounds.width>screen.width?screen.xMin:Mathf.Clamp(bounds.xMin,screen.xMin,screen.xMax-bounds.width);
        float y=bounds.height>screen.height?screen.yMax-bounds.height:Mathf.Clamp(bounds.yMin,screen.yMin,screen.yMax-bounds.height);
        Translate(new Vector2(x-bounds.xMin,y-bounds.yMin));
    }
    private void ApplyPlacement()
    {
        var bounds=Bounds();var screen=ScreenRect();
        var target=screen.min+Vector2.Scale(placement,new Vector2(Mathf.Max(0,screen.width-bounds.width),Mathf.Max(0,screen.height-bounds.height)));
        Translate(target-bounds.min);ClampToScreen();
    }
    public void SavePosition()
    {
        Initialize();ClampToScreen();var bounds=Bounds();var screen=ScreenRect();
        placement=new Vector2(screen.width>bounds.width?(bounds.xMin-screen.xMin)/(screen.width-bounds.width):0,
            screen.height>bounds.height?(bounds.yMin-screen.yMin)/(screen.height-bounds.height):1);
        hasPlacement=true;
        PlayerPrefs.SetString(preferenceKey,placement.x.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"|"+placement.y.ToString("R",System.Globalization.CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }
    public void RestorePosition()
    {
        Initialize();hasPlacement=false;
        var values=PlayerPrefs.GetString(preferenceKey,"").Split('|');
        if(values.Length==2&&float.TryParse(values[0],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float x)&&
            float.TryParse(values[1],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float y)&&
            !float.IsNaN(x)&&!float.IsInfinity(x)&&!float.IsNaN(y)&&!float.IsInfinity(y))
        {placement=new Vector2(Mathf.Clamp01(x),Mathf.Clamp01(y));hasPlacement=true;ApplyPlacement();}
        else ResetMembers();
        RememberSize();
    }
    public void ResetPosition()
    {
        Initialize();dragging=false;suppressClick=true;RemoveShield();hasPlacement=false;
        PlayerPrefs.DeleteKey(preferenceKey);PlayerPrefs.Save();ResetMembers();RememberSize();
    }
    private void ResetMembers(){for(int i=0;i<members.Length;i++)members[i].anchoredPosition=defaults[i];ClampToScreen();}
    private void RememberSize(){lastScreen=Screen.safeArea;lastParent=parent.rect;lastScale=canvas.scaleFactor;}
    private void LateUpdate()
    {
        if(!initialized)return;
        if(shield&&!dragging&&Time.frameCount>shieldUntil)RemoveShield();
        if(lastScreen!=Screen.safeArea||lastParent!=parent.rect||!Mathf.Approximately(lastScale,canvas.scaleFactor))
        {if(hasPlacement&&!dragging)ApplyPlacement();else ClampToScreen();RememberSize();}
    }
    public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)suppressClick=false;}
    public void OnInitializePotentialDrag(PointerEventData e){e.useDragThreshold=true;}
    public void OnBeginDrag(PointerEventData e)
    {
        if(e.button!=PointerEventData.InputButton.Left)return;
        Initialize();
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.pressPosition,e.pressEventCamera,out pointerOrigin))return;
        startPosition=panel.anchoredPosition;dragging=true;suppressClick=true;e.eligibleForClick=false;
        shield=new GameObject("UI drag input shield",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        var rect=(RectTransform)shield.transform;rect.SetParent(canvas.transform,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        var image=shield.GetComponent<Image>();image.color=Color.clear;image.raycastTarget=true;
    }
    public void OnDrag(PointerEventData e)
    {
        if(!dragging||e.button!=PointerEventData.InputButton.Left)return;
        e.eligibleForClick=false;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.position,e.pressEventCamera,out var point))
        {Translate(startPosition+point-pointerOrigin-panel.anchoredPosition);ClampToScreen();}
    }
    public void OnEndDrag(PointerEventData e)
    {
        if(!dragging||e.button!=PointerEventData.InputButton.Left)return;
        e.eligibleForClick=false;dragging=false;shieldUntil=Time.frameCount+1;SavePosition();
    }
    public void OnPointerClick(PointerEventData e)
    {if(rightClickResets&&e.button==PointerEventData.InputButton.Right&&!e.dragging)ResetPosition();}
    private void RemoveShield(){if(shield){shield.SetActive(false);Destroy(shield);shield=null;}}
    private void OnDisable(){if(initialized&&dragging)SavePosition();dragging=false;RemoveShield();}
    private void OnApplicationFocus(bool focus){if(!focus&&dragging){SavePosition();dragging=false;suppressClick=true;RemoveShield();}}
}
