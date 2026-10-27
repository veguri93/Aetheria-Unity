using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using TMPro;
using UnityEngine.UI;

[InitializeOnLoad]
public static class ShortcutVerification
{
    const string Folder="C:/Users/Vesha/Desktop/codexx/ShortcutExpansion/";
    const string Key="Aetheria.ShortcutVerification";
    static double ready;
    static TcpClient sender,receiver;
    static TcpListener listener;
    static SkillShortcutBarUI bar;
    static SkillShortcutSlotUI[] slots;
    static PersistentUIPosition position;
    static Vector2 expected;
    static Keyboard keyboard;
    static int stage;
    static ShortcutVerification(){EditorApplication.playModeStateChanged+=s=>{
        if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){stage=0;ready=EditorApplication.timeSinceStartup+5;EditorApplication.update+=Tick;}
        if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"restore",false)){SessionState.SetBool(Key+"restore",false);EditorSceneManager.OpenScene("Assets/_Recovery/0.unity");}
    };}
    [MenuItem("Tools/Aetheria/Verify 36 shortcuts")]
    public static void Run(){
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save scene and leave Play Mode first.");
        EditorSceneManager.OpenScene("Assets/_Recovery/0.unity");
        File.WriteAllText(Folder+"verification.txt","Unity Play Mode; isolated handler/input and loopback packet tests, NOT live GameServer.\n");
        foreach(var c in UnityEngine.Object.FindObjectsByType<GameServerConnection>(FindObjectsInactive.Include))c.enabled=false;
        var b=UnityEngine.Object.FindFirstObjectByType<SkillShortcutBarUI>();Set(b.GetComponent<PersistentUIPosition>(),"preferenceKey",Key);
        PlayerPrefs.DeleteKey(Key);SessionState.SetBool(Key,true);SessionState.SetBool(Key+"restore",true);EditorApplication.EnterPlaymode();
    }
    static void Set(object o,string n,object v)=>o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,v);
    static void Check(bool ok,string label){if(!ok)throw new Exception(label);File.AppendAllText(Folder+"verification.txt","PASS "+label+"\n");}
    static PointerEventData Event()=>new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
    static void Tick(){if(EditorApplication.timeSinceStartup<ready)return;try{
        if(stage++==0){
            bar=UnityEngine.Object.FindFirstObjectByType<SkillShortcutBarUI>();slots=bar.GetComponentsInChildren<SkillShortcutSlotUI>().OrderBy(s=>s.SlotNumber).ToArray();position=bar.GetComponent<PersistentUIPosition>();
            Check(slots.Length==36,"36 slots created from original prefab");
            for(int i=0;i<36;i++){var r=(RectTransform)slots[i].transform;Check(r.anchoredPosition==new Vector2(9+i%12*44,9+i/12*44),"Slot "+(i+1)+" in correct row/column");Check(slots[i].GetComponentInChildren<TMP_Text>(true).text==(i<12?"F"+(i+1):""),"Key label "+(i+1));}
            listener=new TcpListener(IPAddress.Loopback,0);listener.Start();sender=new TcpClient();sender.Connect((IPEndPoint)listener.LocalEndpoint);receiver=listener.AcceptTcpClient();Set(GameServerConnection.Instance,"_client",sender);
            var db=(SkillDatabase)typeof(SkillShortcutBarUI).GetField("skillDatabase",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(bar);
            // Clone the database for a self-targeting packet fixture; never edit the asset.
            db=UnityEngine.Object.Instantiate(db);Set(bar,"skillDatabase",db);Set(LocalPlayer.Instance.GetComponent<PlayerInteraction>(),"skillDatabase",db);
            var skill=db.Skills.First(s=>s.Icon);Set(skill,"targetType",SkillClientTargetType.SELF);
            var source=new GameObject("Test skill source").AddComponent<SkillEntryUI>();source.SetSkill(skill);
            foreach(var s in slots){var e=Event();e.pointerDrag=source.gameObject;s.OnDrop(e);Check(s.SkillId==skill.SkillId,"Skill drop into slot "+s.SlotNumber);s.OnPointerClick(Event());}
            var inv=new GameObject("Test inventory source",typeof(RectTransform)).AddComponent<InventorySlotUI>();
            var icon=new GameObject("Icon",typeof(RectTransform),typeof(Image)).GetComponent<Image>();Set(inv,"itemIcon",icon);
            var label=new GameObject("Count",typeof(RectTransform),typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();Set(inv,"quantityText",label);
            var item=new ClientItemInstance(990001,57,5,"None",stackable:true,droppable:true);ClientInventory.Add(item);inv.SetItem(item,skill.Icon);
            foreach(var s in slots){var e=Event();e.pointerDrag=inv.gameObject;s.OnDrop(e);Check(s.ShortcutType==ShortcutType.Item&&s.ReferenceId==57,"Inventory drop into slot "+s.SlotNumber);s.OnPointerClick(Event());}
            Check((bool)typeof(InventorySlotUI).GetField("dropHandled",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(inv),"Inventory assignment marks drop handled; no world drop");
            foreach(var s in slots)s.SetSkill(skill);
            keyboard=InputSystem.AddDevice<Keyboard>();
            var interaction=LocalPlayer.Instance.GetComponent<PlayerInteraction>();var hotkeys=typeof(PlayerInteraction).GetMethod("HandleShortcutHotkeys",BindingFlags.Instance|BindingFlags.NonPublic);
            for(int i=0;i<12;i++){InputSystem.QueueStateEvent(keyboard,new KeyboardState((UnityEngine.InputSystem.Key)((int)UnityEngine.InputSystem.Key.F1+i)));InputSystem.Update();hotkeys.Invoke(interaction,null);InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();}
            InputSystem.RemoveDevice(keyboard);keyboard=null;
            var grip=bar.transform.Find("Drag handle").gameObject;
            Check(ExecuteEvents.GetEventHandler<IDragHandler>(grip)==bar.gameObject,"Grip routes drag to persistent bar");
            Check(slots.All(s=>ExecuteEvents.GetEventHandler<IDragHandler>(s.gameObject)==s.gameObject),"Slot assignment drags remain owned by slots");
            var rect=(RectTransform)bar.transform;var origin=rect.anchoredPosition;var eDrag=Event();eDrag.pressPosition=eDrag.position=RectTransformUtility.WorldToScreenPoint(null,grip.transform.position);
            ExecuteEvents.Execute(bar.gameObject,eDrag,ExecuteEvents.beginDragHandler);eDrag.position+=new Vector2(150,160);ExecuteEvents.Execute(bar.gameObject,eDrag,ExecuteEvents.dragHandler);ExecuteEvents.Execute(bar.gameObject,eDrag,ExecuteEvents.endDragHandler);
            Check(Vector2.Distance(rect.anchoredPosition,origin)>50&&PlayerPrefs.HasKey(Key),"Grip drag moves bar and saves local position");expected=rect.anchoredPosition;rect.anchoredPosition+=Vector2.one*40;position.RestorePosition();Check(Vector2.Distance(rect.anchoredPosition,expected)<.1f,"Saved position restores");
            foreach(var offset in new[]{new Vector2(-9999,-9999),new Vector2(9999,9999)}){rect.anchoredPosition+=offset;position.ClampToScreen();var corners=new Vector3[4];rect.GetWorldCorners(corners);Check(corners.All(c=>c.x>=-1&&c.y>=-1&&c.x<=Screen.width+1&&c.y<=Screen.height+1),"Screen boundaries clamp entire bar");}
            position.RestorePosition();ScreenCapture.CaptureScreenshot(Folder+"shortcut-playmode.png");ready=EditorApplication.timeSinceStartup+2;
        }else{
            var data=new byte[receiver.Available];receiver.GetStream().Read(data,0,data.Length);int at=0,updates=0,skills=0,items=0;
            while(at+4<=data.Length){int len=BitConverter.ToUInt16(data,at),type=BitConverter.ToUInt16(data,at+2);Check(at+len+2<=data.Length,"Complete outgoing packet");if(type==35)updates++;if(type==33)skills++;if(type==21)items++;at+=len+2;}
            Check(updates==72,"All 36 skill and 36 item assignments use existing save packet");Check(skills==48,"36 mouse activations plus 12 original F-key activations use skill packet");Check(items==36,"All 36 mouse item activations use existing item packet");
            var db=(SkillDatabase)typeof(SkillShortcutBarUI).GetField("skillDatabase",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(bar);var id=db.Skills.First().SkillId;
            ShortcutSnapshotPacketHandler.Handle(new ClientPacket(0,Encoding.UTF8.GetBytes(string.Join(";",Enumerable.Range(1,36).Select(i=>"0,"+i+",1,"+id)))));
            Check(slots.All(s=>s.SkillId==id),"Server snapshot restores all 36 assignments");
            File.AppendAllText(Folder+"verification.txt","COMPLETE\n");Finish();
        }
    }catch(Exception e){File.AppendAllText(Folder+"verification.txt","FAIL "+e+"\n");Finish();}}
    static void Finish(){EditorApplication.update-=Tick;sender?.Close();receiver?.Close();listener?.Stop();if(keyboard!=null&&keyboard.added)InputSystem.RemoveDevice(keyboard);PlayerPrefs.DeleteKey(Key);PlayerPrefs.Save();SessionState.SetBool(Key,false);EditorApplication.ExitPlaymode();}
}

