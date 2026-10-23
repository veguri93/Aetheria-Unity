using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

[InitializeOnLoad]
public static class HUDDragVerification
{
    const string Folder="C:/Users/Vesha/Desktop/codexx/PlayerHUDDrag/";
    const string Key="HUDDragVerification";
    const string Pref="Aetheria.UI.PlayerHUD.position.verification";
    static int frame,stage,worldClicks,menuClicks,menuIndex;
    static double deadline;
    static PersistentUIPosition position;
    static RectTransform panel,menu;
    static Vector2 initial,menuInitial,press,saved;
    static Mouse mouse;
    static PlayerInputController input;
    static Button[] buttons;
    static CanvasScaler scaler;
    static CanvasScaler.ScaleMode scaleMode;
    static float scale;
    static RectTransform[] excluded;
    static Vector2[] excludedPositions;
    static EditorWindow gameView;
    static int originalSize;
    static PropertyInfo sizeIndex;
    static HUDDragVerification()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))
            {frame=stage=0;worldClicks=menuClicks=menuIndex=0;deadline=EditorApplication.timeSinceStartup+120;new GameObject("HUD verification runner").AddComponent<HUDVerificationRunner>();}
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+".restore",false))
            {
                SessionState.SetBool(Key+".restore",false);EditorSceneManager.OpenScene("Assets/_Recovery/0.unity");
                if(SessionState.GetBool(Key+".second",false))
                {
                    SessionState.SetBool(Key+".second",false);SessionState.SetBool(Key+".restart",true);PreparePlay();
                }
            }
        };
    }
    [MenuItem("Tools/Aetheria/Verify movable PlayerHUD")]
    public static void Run()
    {
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Requires Edit Mode with a saved scene.");
        EditorSceneManager.OpenScene("Assets/_Recovery/0.unity");
        File.WriteAllText(Folder+"unity-tests.txt","Actual Unity gameplay scene, isolated input; networking disabled for this test.\n");
        PlayerPrefs.DeleteKey(Pref);SessionState.SetBool(Key+".restart",false);
        PreparePlay();
    }
    static void PreparePlay()
    {
        EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView")).Focus();
        foreach(var c in UnityEngine.Object.FindObjectsByType<GameServerConnection>(FindObjectsInactive.Include))c.enabled=false;
        var p=UnityEngine.Object.FindFirstObjectByType<PersistentUIPosition>(FindObjectsInactive.Include);
        Set(p,"preferenceKey",Pref);SessionState.SetBool(Key,true);SessionState.SetBool(Key+".restore",true);EditorApplication.EnterPlaymode();
    }
    static void Set(object o,string n,object value)=>o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);File.AppendAllText(Folder+"unity-tests.txt","PASS "+message+"\n");}
    static Vector2 Point(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(rect.rect.xMin+80,rect.rect.yMax-12)));
    static void MouseAt(Vector2 p,bool down=false,bool right=false)
    {var state=new MouseState{position=p};if(down)state=state.WithButton(MouseButton.Left);if(right)state=state.WithButton(MouseButton.Right);InputSystem.QueueStateEvent(mouse,state);InputSystem.Update();EventSystem.current.currentInputModule.Process();}
    public static void Tick()
    {
        if(!EditorApplication.isPlaying||!SessionState.GetBool(Key,false))return;
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("HUD verification timed out");
            if(++frame%8!=0)return;
            switch(stage++)
            {
                case 0:
                    position=UnityEngine.Object.FindFirstObjectByType<PersistentUIPosition>();panel=(RectTransform)position.transform;menu=(RectTransform)panel.parent.Find("MenuButtonsRow");
                    Check(position&&panel&&menu,"Only the saved PlayerInfoPanel is configured with menu-row companion");
                    if(SessionState.GetBool(Key+".restart",false))
                    {
                        var expected=JsonUtility.FromJson<SavedPoint>(File.ReadAllText(Folder+"expected-position.json"));
                        Check(Vector2.Distance(panel.anchoredPosition,new Vector2(expected.x,expected.y))<1,"Saved position automatically restored after exiting and re-entering Play Mode");
                        position.ResetPosition();Check(!PlayerPrefs.HasKey(Pref),"Reset removes only this HUD's saved preference");Finish(true,false);return;
                    }
                    initial=panel.anchoredPosition;menuInitial=menu.anchoredPosition;
                    excluded=new[]{"StatsPanel","SkillShortcutBar","BuffPanel"}.Select(n=>(RectTransform)panel.parent.Find(n)).ToArray();excludedPositions=excluded.Select(t=>t.anchoredPosition).ToArray();
                    input=LocalPlayer.Instance.GetComponent<PlayerInputController>();input.Controls.Player.LeftClick.performed+=CountWorld;
                    mouse=InputSystem.AddDevice<Mouse>("HUDVerificationMouse");
                    TargetManager.Instance.ClearTarget();press=Point(panel);MouseAt(press);break;
                case 1:MouseAt(press,true);break;
                case 2:Check(worldClicks==0&&!input.Controls.Player.LeftClick.enabled,"HUD press is reserved before world movement/target action");MouseAt(press+new Vector2(3,1),true);break;
                case 3:MouseAt(press+new Vector2(3,1));break;
                case 4:
                    var ui=(UnityEngine.InputSystem.UI.InputSystemUIInputModule)EventSystem.current.currentInputModule;
                    File.AppendAllText(Folder+"unity-tests.txt","DIAG focus="+EventSystem.current.isFocused+" click="+ui.leftClick.action.enabled+" point="+ui.point.action.ReadValue<Vector2>()+" controls="+string.Join(",",ui.leftClick.action.controls.Select(c=>c.path))+"\n");
                    var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=press},hits);
                    File.AppendAllText(Folder+"unity-tests.txt","DIAG mouse="+mouse.position.ReadValue()+" point="+press+" hits="+string.Join(",",hits.Select(h=>h.gameObject.name))+" module="+EventSystem.current.currentInputModule+" dragging="+position.IsDragging+" suppressed="+position.SuppressClick+" target="+TargetManager.Instance.CurrentTarget+"\n");
                    Check(ReferenceEquals(TargetManager.Instance.CurrentTarget,LocalPlayer.Instance),"Normal left click with small motion still self-targets");
                    Check(Vector2.Distance(panel.anchoredPosition,initial)<.1f,"Below-threshold motion does not reposition HUD");
                    TargetManager.Instance.ClearTarget();MouseAt(press,true);break;
                case 5:MouseAt(press+new Vector2(350,-200),true);break;
                case 6:Check(position.IsDragging,"Actual EventSystem drag begins beyond its 10-pixel threshold");MouseAt(press+new Vector2(500,-280),true);break;
                case 7:MouseAt(press+new Vector2(500,-280));break;
                case 8:
                    Check(!position.IsDragging&&position.SuppressClick&&TargetManager.Instance.CurrentTarget==null,"Drag release does not self-target");
                    Check(Vector2.Distance(panel.anchoredPosition,initial)>100,"PlayerHUD moved");
                    Check(Vector2.Distance(menu.anchoredPosition-menuInitial,panel.anchoredPosition-initial)<.1f,"Menu buttons move with the status panel");
                    Check(worldClicks==0&&input.Controls.Player.LeftClick.enabled,"Drag and release cause no gameplay click; world action restores afterward");
                    Check(excluded.Select((t,i)=>Vector2.Distance(t.anchoredPosition,excludedPositions[i])<.1f).All(x=>x),"Stats window, Shortcut Bar and BuffPanel did not move");
                    Check(PlayerPrefs.HasKey(Pref),"Drag release saved a local preference");saved=panel.anchoredPosition;
                    panel.anchoredPosition+=new Vector2(90,-40);menu.anchoredPosition+=new Vector2(90,-40);position.RestorePosition();
                    Check(Vector2.Distance(panel.anchoredPosition,saved)<.1f,"Persisted normalized position restores exactly at the same resolution");
                    ScreenCapture.CaptureScreenshot(Folder+"hud-moved.png");break;
                case 9:
                    foreach(var delta in new[]{new Vector2(-10000,-10000),new Vector2(10000,10000),new Vector2(-10000,10000),new Vector2(10000,-10000)})
                    {panel.anchoredPosition+=delta;menu.anchoredPosition+=delta;position.ClampToScreen();Inside();}
                    Check(true,"All four screen boundaries keep panel and menu row visible");
                    position.ResetPosition();press=Point(panel);MouseAt(press,true);break;
                case 10:MouseAt(press+new Vector2(300,-180),true);break;
                case 11:MouseAt(press+new Vector2(300,-180));break;
                case 12:press=Point(panel);MouseAt(press,false,true);break;
                case 13:MouseAt(press);break;
                case 14:
                    Check(Vector2.Distance(panel.anchoredPosition,initial)<.1f&&Vector2.Distance(menu.anchoredPosition,menuInitial)<.1f,"Right-click restores the exact original panel/menu positions");
                    buttons=menu.GetComponentsInChildren<Button>();Check(buttons.Length==5,"All five original menu buttons retained");foreach(var button in buttons)button.onClick.AddListener(CountMenu);
                    MouseAt(RectTransformUtility.WorldToScreenPoint(null,buttons[0].transform.position));break;
                case 15:MouseAt(RectTransformUtility.WorldToScreenPoint(null,buttons[menuIndex].transform.TransformPoint(((RectTransform)buttons[menuIndex].transform).rect.center)),true);break;
                case 16:MouseAt(RectTransformUtility.WorldToScreenPoint(null,buttons[menuIndex].transform.TransformPoint(((RectTransform)buttons[menuIndex].transform).rect.center)));break;
                case 17:
                    Check(menuClicks==menuIndex+1,"Existing menu button receives its click: "+buttons[menuIndex].name);
                    // Close any window the original listener opened, without changing saved state.
                    foreach(var name in new[]{"StatsPanel"})panel.parent.Find(name)?.gameObject.SetActive(false);
                    foreach(var name in new[]{"SkillsWindow","InventoryWindow","QuestWindow","GuildWindow"})panel.GetComponentInParent<Canvas>().rootCanvas.transform.Find(name)?.gameObject.SetActive(false);
                    if(++menuIndex<buttons.Length)stage=15;break;
                case 18:
                    scaler=panel.GetComponentInParent<Canvas>().rootCanvas.GetComponent<CanvasScaler>();scaleMode=scaler.uiScaleMode;scale=scaler.scaleFactor;
                    scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=1.5f;break;
                case 19:Inside();Check(true,"Canvas scale 1.5 retains visible HUD and menu");scaler.scaleFactor=.75f;break;
                case 20:Inside();Check(true,"Canvas scale 0.75 retains visible HUD and menu");scaler.uiScaleMode=scaleMode;scaler.scaleFactor=scale;SetGameSize(1280,720);break;
                case 21:Inside();Check(Screen.width==1280&&Screen.height==720,"Actual 1280 × 720 GameView is bounded");SetGameSize(1024,768);break;
                case 22:Inside();Check(Screen.width==1024&&Screen.height==768,"Actual 1024 × 768 GameView is bounded");sizeIndex.SetValue(gameView,originalSize);break;
                case 23:
                    position.ResetPosition();press=Point(panel);MouseAt(press,true);break;
                case 24:MouseAt(press+new Vector2(230,-160),true);break;
                case 25:MouseAt(press+new Vector2(230,-160));break;
                case 26:
                    File.WriteAllText(Folder+"expected-position.json",JsonUtility.ToJson(new SavedPoint{x=panel.anchoredPosition.x,y=panel.anchoredPosition.y}));
                    Finish(true,true);break;
            }
        }
        catch(Exception e){File.AppendAllText(Folder+"unity-tests.txt","FAIL "+e+"\n");Finish(false,false);}
    }
    [Serializable]class SavedPoint{public float x,y;}
    static void CountWorld(InputAction.CallbackContext context){worldClicks++;}
    static void CountMenu(){menuClicks++;}
    static void Inside()
    {
        var corners=new Vector3[4];foreach(var r in new[]{panel,menu}){r.GetWorldCorners(corners);foreach(var corner in corners){var p=RectTransformUtility.WorldToScreenPoint(null,corner);Check(p.x>=-1&&p.y>=-1&&p.x<=Screen.width+1&&p.y<=Screen.height+1,"Visible bounds: "+r.name);}}
    }
    static void SetGameSize(int width,int height)
    {
        var assembly=typeof(Editor).Assembly;var type=assembly.GetType("UnityEditor.GameView");
        if(!gameView){gameView=EditorWindow.GetWindow(type);sizeIndex=type.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);originalSize=(int)sizeIndex.GetValue(gameView);}
        var sizesType=assembly.GetType("UnityEditor.GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);var sizes=singleton.GetProperty("instance").GetValue(null);
        var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeGroupType"),"Standalone")});
        var gt=group.GetType();int count=(int)gt.GetMethod("GetTotalCount").Invoke(group,null),found=-1;
        for(int i=0;i<count;i++){var size=gt.GetMethod("GetGameViewSize").Invoke(group,new object[]{i});if((int)size.GetType().GetProperty("width").GetValue(size)==width&&(int)size.GetType().GetProperty("height").GetValue(size)==height){found=i;break;}}
        if(found<0){var size=Activator.CreateInstance(assembly.GetType("UnityEditor.GameViewSize"),Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"),"FixedResolution"),width,height,"HUD verification");gt.GetMethod("AddCustomSize").Invoke(group,new[]{size});found=count;}
        sizeIndex.SetValue(gameView,found);gameView.Repaint();
    }
    static void Finish(bool ok,bool restart)
    {
        EditorApplication.update-=Tick;if(input&&input.Controls!=null)input.Controls.Player.LeftClick.performed-=CountWorld;
        if(mouse!=null&&mouse.added)InputSystem.RemoveDevice(mouse);mouse=null;
        if(gameView&&sizeIndex!=null)sizeIndex.SetValue(gameView,originalSize);
        if(!restart){PlayerPrefs.DeleteKey(Pref);PlayerPrefs.Save();}
        SessionState.SetBool(Key,false);SessionState.SetBool(Key+".second",restart);
        if(ok&&!restart)File.AppendAllText(Folder+"unity-tests.txt","COMPLETE all checks passed; original scene and user preference preserved.\n");
        EditorApplication.ExitPlaymode();
    }
}
public sealed class HUDVerificationRunner : MonoBehaviour
{
    private void Update(){HUDDragVerification.Tick();}
}
