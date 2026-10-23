using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class NpcDialogueSetup
{

    static TMP_FontAsset font;
    static Color gold = new Color(.83f,.72f,.51f), ink = new Color(.065f,.069f,.073f,.98f);
    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }
    static void Stretch(RectTransform r, float left=0, float bottom=0, float right=0, float top=0)
    {
        r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one;
        r.offsetMin=new Vector2(left,bottom); r.offsetMax=new Vector2(-right,-top);
    }
    static TextMeshProUGUI Text(string name, Transform parent, int size, Color color)
    {
        var r=Rect(name,parent); var t=r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font=font; t.fontSize=size; t.color=color; t.raycastTarget=false;
        t.textWrappingMode=TextWrappingModes.Normal; return t;
    }
    static Button Button(string name, Transform parent, string label)
    {
        var r=Rect(name,parent); var image=r.gameObject.AddComponent<Image>();
        image.color=Color.white;
        var b=r.gameObject.AddComponent<Button>(); b.targetGraphic=image;
        var c=b.colors; c.normalColor=new Color(.16f,.15f,.13f); c.highlightedColor=new Color(.27f,.23f,.17f);
        c.pressedColor=new Color(.34f,.28f,.18f); c.selectedColor=c.highlightedColor;
        c.disabledColor=new Color(.08f,.09f,.11f,.65f); c.fadeDuration=.12f; b.colors=c;
        var outline=r.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(.44f,.35f,.22f,.8f); outline.effectDistance=new Vector2(1,-1);
        // Separate edge graphics retain their bronze tint during Button color transitions.
        foreach (string side in new[]{"Top","Bottom","Left","Right"})
        {
            var edge=Rect(side+" edge",r);Stretch(edge);
            if(side=="Top" || side=="Bottom") { edge.anchorMin=new Vector2(0,side=="Top"?1:0);edge.anchorMax=new Vector2(1,side=="Top"?1:0);edge.sizeDelta=new Vector2(0,1); }
            else { edge.anchorMin=new Vector2(side=="Right"?1:0,0);edge.anchorMax=new Vector2(side=="Right"?1:0,1);edge.sizeDelta=new Vector2(1,0); }
            var border=edge.gameObject.AddComponent<Image>();border.color=new Color(.43f,.35f,.23f,.85f);border.raycastTarget=false;
        }
        r.gameObject.AddComponent<MerchantButtonFeedback>();
        var t=Text("Label",r,18,new Color(.90f,.86f,.76f)); t.text=label; t.alignment=TextAlignmentOptions.Center;
        Stretch(t.rectTransform,12,6,12,6); t.richText=false;
        return b;
    }
    [MenuItem("Tools/Aetheria/Style existing NPC dialogue window")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Style the NPC window outside Play Mode.");
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.isDirty) throw new InvalidOperationException("Save the current scene before styling the NPC window.");
        var window=UnityEngine.Object.FindAnyObjectByType<NpcHtmlWindow>(FindObjectsInactive.Include);
        if (!window) throw new Exception("Missing existing shared NPC window");
        font=window.GetComponentInChildren<TextMeshProUGUI>(true).font;
        var root=(RectTransform)window.transform;
        foreach(Transform child in root.Cast<Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
        root.anchorMin=root.anchorMax=new Vector2(.5f,.5f); root.pivot=new Vector2(.5f,.5f);
        root.anchoredPosition=Vector2.zero; root.sizeDelta=new Vector2(520,440);
        root.GetComponent<Image>().sprite=null;
        root.GetComponent<Image>().type=Image.Type.Simple;
        root.GetComponent<Image>().color=ink;
        var border=root.GetComponent<Outline>() ?? root.gameObject.AddComponent<Outline>();
        border.effectColor=new Color(.48f,.39f,.26f); border.effectDistance=new Vector2(1,-1);
        var heading=Text("NPC name",root,26,gold); heading.richText=false;
        heading.rectTransform.anchorMin=new Vector2(0,1); heading.rectTransform.anchorMax=Vector2.one;
        heading.rectTransform.pivot=new Vector2(.5f,1); heading.rectTransform.sizeDelta=new Vector2(-80,35); heading.rectTransform.anchoredPosition=new Vector2(-16,-18);
        var title=Text("NPC title",root,15,new Color(.64f,.60f,.51f)); title.richText=false;
        title.rectTransform.anchorMin=new Vector2(0,1); title.rectTransform.anchorMax=Vector2.one;
        title.rectTransform.pivot=new Vector2(.5f,1); title.rectTransform.sizeDelta=new Vector2(-48,24); title.rectTransform.anchoredPosition=new Vector2(0,-53);
        var close=Button("Close",root,"×"); var cr=(RectTransform)close.transform;
        cr.anchorMin=cr.anchorMax=Vector2.one; cr.pivot=Vector2.one; cr.anchoredPosition=new Vector2(-16,-16); cr.sizeDelta=new Vector2(32,32);
        var rule=Rect("Header rule",root); rule.anchorMin=new Vector2(0,1);rule.anchorMax=Vector2.one;rule.sizeDelta=new Vector2(-48,1);rule.anchoredPosition=new Vector2(0,-85);rule.gameObject.AddComponent<Image>().color=gold*.65f;
        var viewport=Rect("Dialogue viewport",root); Stretch(viewport,28,46,28,100); viewport.gameObject.AddComponent<RectMask2D>();
        var hit=viewport.gameObject.AddComponent<Image>();hit.color=new Color(0,0,0,.01f);
        var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
        var content=Rect("Content",viewport); content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;
        var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=20;layout.childControlHeight=true;layout.childControlWidth=true;layout.childForceExpandHeight=false;layout.childForceExpandWidth=true;layout.padding=new RectOffset(2,2,2,8);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport=viewport;scroll.content=content;
        var track=Rect("Scroll track",root); track.anchorMin=new Vector2(1,0);track.anchorMax=Vector2.one;
        track.pivot=new Vector2(1,.5f);track.offsetMin=new Vector2(-15,46);track.offsetMax=new Vector2(-11,-100);
        var trackImage=track.gameObject.AddComponent<Image>();trackImage.color=new Color(.12f,.115f,.10f);
        var scrollbar=track.gameObject.AddComponent<Scrollbar>();scrollbar.direction=Scrollbar.Direction.BottomToTop;
        var handle=Rect("Handle",track);Stretch(handle);var handleImage=handle.gameObject.AddComponent<Image>();handleImage.color=new Color(.49f,.41f,.28f);
        scrollbar.handleRect=handle;scrollbar.targetGraphic=handleImage;
        scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
        var body=Text("Dialogue",content,20,new Color(.87f,.85f,.79f));body.lineSpacing=5;body.paragraphSpacing=8;body.text="";body.overflowMode=TextOverflowModes.Overflow;
        var buttons=Rect("Actions",content);var bl=buttons.gameObject.AddComponent<VerticalLayoutGroup>();bl.spacing=10;bl.childControlHeight=true;bl.childControlWidth=true;bl.childForceExpandHeight=false;
        var template=Button("Action template",buttons,"Option"); var le=template.gameObject.AddComponent<LayoutElement>();le.minHeight=48;le.preferredHeight=48;template.gameObject.SetActive(false);
        var status=Text("Status",root,13,new Color(.70f,.64f,.51f));status.richText=false;status.text="";
        status.rectTransform.anchorMin=Vector2.zero;status.rectTransform.anchorMax=new Vector2(1,0);status.rectTransform.pivot=new Vector2(.5f,0);status.rectTransform.sizeDelta=new Vector2(-48,40);status.rectTransform.anchoredPosition=new Vector2(0,7);
        var so=new SerializedObject(window);
        foreach(var pair in new (string,UnityEngine.Object)[]{("_contentText",body),("_buttonContainer",buttons),("_buttonTemplate",template),("_heading",heading),("_title",title),("_status",status),("_scroll",scroll),("_close",close)}) so.FindProperty(pair.Item1).objectReferenceValue=pair.Item2;
        so.FindProperty("_responsiveLayout").boolValue=true;
        so.ApplyModifiedPropertiesWithoutUndo();
        window.gameObject.SetActive(false);
        EditorUtility.SetDirty(window);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("NPC dialogue styling saved: header, body, scroll area and shared action template are wired.");
        TestMarkup();
    }
    static void TestMarkup()
    {
        var options=NpcDialogueMarkup.Buttons("<BUTTON action='html 1' value='A &amp; B' /><script><button value='bad' action='bad'></script><button value=\"Bye\" action=\"close\">");
        if(options.Count!=2 || options[0].Label!="A & B" || options[0].Action!="html 1")throw new Exception("Parser failed");
        var text=NpcDialogueMarkup.Text("<h2>Hello</h2><p>A &amp; B</p><script>bad</script><sprite=2><br/>End");
        if(text.Contains("bad") || text.Contains("sprite") || !text.Contains("A & B"))throw new Exception("Sanitizer failed");
        Debug.Log("NPC dialogue markup checks passed.");
    }
}
