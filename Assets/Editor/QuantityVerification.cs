using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class QuantityVerification
{
    const string Folder="C:/Users/Vesha/Desktop/codexx/MerchantQuantity/";
    const string Key="Aetheria.QuantityVerification";
    static int frame;
    static double deadline;
    static TcpClient socket,peer;
    static QuantityVerification()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))
            {frame=0;deadline=EditorApplication.timeSinceStartup+45;EditorApplication.update+=Tick;}
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+".exit",false))
            {SessionState.SetBool(Key+".exit",false);EditorSceneManager.OpenScene("Assets/_Recovery/0.unity");if(Application.isBatchMode)EditorApplication.Exit(SessionState.GetBool(Key+".ok",false)?0:1);}
        };
    }
    [MenuItem("Tools/Aetheria/Verify merchant quantity")]
    public static void Run()
    {
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Verification requires Edit Mode and a saved scene.");
        EditorSceneManager.OpenScene("Assets/_Recovery/0.unity");
        foreach(var connection in UnityEngine.Object.FindObjectsByType<GameServerConnection>(FindObjectsInactive.Include,FindObjectsSortMode.None))connection.enabled=false;
        File.WriteAllText(Folder+"unity-tests.txt","Actual gameplay scene, Unity Play Mode, isolated transport; no live database.\n");
        SessionState.SetBool(Key,true);EditorApplication.EnterPlaymode();
    }
    static void Check(bool ok,string text){if(!ok)throw new Exception(text);File.AppendAllText(Folder+"unity-tests.txt","PASS "+text+"\n");}
    static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
    static void Tick()
    {
        try
        {
            if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Play Mode verification");
            if(++frame==4)Verify();
            if(frame==12)ScreenCapture.CaptureScreenshot(Folder+"quantity-playmode.png");
            if(frame>30)Finish(true);
        }
        catch(Exception ex){File.AppendAllText(Folder+"unity-tests.txt","FAIL "+ex+"\n");Finish(false);}
    }
    static void Verify()
    {
        var shop=UnityEngine.Object.FindFirstObjectByType<MultiSellWindow>(FindObjectsInactive.Include);Check(shop,"Saved shop exists");
        shop.gameObject.SetActive(true);Check(MultiSellWindow.Instance==shop,"Shop initialized");
        string captured=File.ReadAllText("C:/Users/Vesha/Desktop/codexx/shop-packet.txt");
        var parts=captured.Split(new[]{'|'},3);
        shop.Show(int.Parse(parts[0]),int.Parse(parts[1]),parts[2]);
        var details=Field<PurchaseDetailsWindow>(shop,"_purchaseDetailsWindow");
        var input=Field<TMP_InputField>(details,"quantityInput");var buy=Field<Button>(details,"_buyButton");
        var grid=Field<Transform>(shop,"_entryContainer").GetComponent<GridLayoutGroup>();
        Check(grid.constraintCount==6,"Saved six-column grid preserved");
        ClientInventory.Clear();for(int i=0;i<100;i++)ClientInventory.Add(new ClientItemInstance(99000+i,1001,1,"None"));
        shop.Select(shop.Slots.First(s=>s.Offer!=null));
        Check(input.text=="1"&&details.Quantity==1&&buy.interactable,"Default quantity 1 retains existing availability");
        foreach(int n in new[]{5,100}){input.text=n.ToString();Check(details.Quantity==n&&buy.interactable,"Live quantity "+n);}
        foreach(string invalid in new[]{"","0","-1","abc","1.5","2147483648","999999999999999999999"}){input.text=invalid;Check(!buy.interactable,"Invalid input disables Buy: "+invalid);}
        input.text="5";
        var offer=MerchantOffer.Parse("2~2001:10,2003:2~1001:2,2001:3",Field<ItemVisualDatabase>(shop,"_itemVisualDatabase")).Single();
        details.ShowOffer(offer);Check(input.text=="1","Different offer resets quantity");input.text="100";
        var rows=Field<System.Collections.Generic.List<PurchaseCostRowUI>>(details,"rows");
        Check(Field<TMP_Text>(rows[0],"_costAmount").text==1000.ToString("N0")&&Field<TMP_Text>(rows[1],"_costAmount").text==200.ToString("N0"),"Every ingredient row displays multiplied formatted totals");
        Check(Field<TMP_Text>(details,"_itemName").text.Contains("200")&&Field<TMP_Text>(details,"_descriptionText").text.Contains("300"),"Primary and additional rewards multiply");
        Check(!buy.interactable,"Missing ingredients disable purchasing");
        shop.Select(shop.Slots.First(s=>s.Offer!=null));input.text="5";
        Canvas.ForceUpdateCanvases();
        var corners=new Vector3[4];((RectTransform)input.transform).GetWorldCorners(corners);float inputRight=corners[2].x;((RectTransform)buy.transform).GetWorldCorners(corners);
        Check(inputRight<corners[0].x,"Quantity is immediately left of Buy without overlap");
        Check(((RectTransform)buy.transform).rect.width>=100,"Buy remains usable within original footer");
        foreach(RectTransform child in buy.transform)
        {
            child.GetWorldCorners(corners);float childRight=corners[2].x;
            ((RectTransform)buy.transform).GetWorldCorners(corners);
            Check(childRight<=corners[2].x+1,"Buy decoration remains inside footer: "+child.name);
        }
        var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();peer=new TcpClient();peer.Connect((IPEndPoint)listener.LocalEndpoint);socket=listener.AcceptTcpClient();listener.Stop();
        var connection=GameServerConnection.Instance;typeof(GameServerConnection).GetField("_client",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(connection,socket);
        buy.onClick.Invoke();buy.onClick.Invoke();
        var stream=peer.GetStream();stream.ReadTimeout=3000;byte[] len=new byte[2];stream.Read(len,0,2);var bytes=new byte[BitConverter.ToUInt16(len,0)];int read=0;while(read<bytes.Length)read+=stream.Read(bytes,read,bytes.Length-read);
        var wire=Encoding.UTF8.GetString(bytes,2,bytes.Length-2).Split('|');
        Check(BitConverter.ToUInt16(bytes,0)==46&&wire.Length==5&&wire[3]=="5","Existing Buy sends selected quantity and replay ID via packet 46");
        Check(MerchantPurchaseState.Pending&&!buy.interactable&&peer.Available==0,"Repeated click sends exactly one request");
        ClientPacketHandler.Handle(new ClientPacket(87,Encoding.UTF8.GetBytes(wire[4]+"|1")));
        Check(!MerchantPurchaseState.Pending,"Correlated result releases pending state");
        ClientInventory.Clear();details.RefreshAvailability();Check(!buy.interactable,"Inventory updates refresh purchase availability");
        for(int i=0;i<100;i++)ClientInventory.Add(new ClientItemInstance(99000+i,1001,1,"None"));
        input.text="25";Check(details.Quantity==25,"Restored inventory and quantity ready for screenshot");
    }
    static void Finish(bool ok)
    {
        EditorApplication.update-=Tick;socket?.Close();peer?.Close();SessionState.SetBool(Key,false);
        SessionState.SetBool(Key+".ok",ok);SessionState.SetBool(Key+".exit",true);EditorApplication.ExitPlaymode();
    }
}
