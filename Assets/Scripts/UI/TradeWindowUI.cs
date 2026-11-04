using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Uses the scene's inventory assets and canvas; no prefab/Inspector wiring required.
public sealed class TradeWindowUI : MonoBehaviour
{
    public static TradeWindowUI Instance { get; private set; }
    private InventoryUI inventory;
    private ItemVisualDatabase visuals;
    private RectTransform panel, mine, theirs, picker;
    private TMP_Text title, status, confirmations, invitationText;
    private RectTransform invitation;
    private Slider invitationTimer;
    private Button invitationCancel, invitationClose;
    private float invitationExpiresAt;
    private TMP_InputField quantity;
    private Button accept, reject, confirm, cancel, add, close;
    private Guid tradeId, expiredInvitation;
    private readonly HashSet<Guid> finishedTrades = new HashSet<Guid>();
    private int revision, selectedId;
    private bool initiator, active, pending, recovery, myConfirmed, theirConfirmed;
    private readonly List<OfferItem> myItems = new List<OfferItem>();
    private readonly List<OfferItem> theirItems = new List<OfferItem>();
    private sealed class OfferItem { public int Id, Quantity, Template, Enchant; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneLoaded += SceneLoaded;
        EnsureCreated();
    }
    private static void SceneLoaded(Scene scene, LoadSceneMode mode) => EnsureCreated();
    public static void EnsureCreated()
    {
        if (Instance) return;
        var source = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (!source || !source.GetComponentInParent<Canvas>()) return;
        var owner = new GameObject("TradeUI", typeof(RectTransform));
        owner.transform.SetParent(source.GetComponentInParent<Canvas>().rootCanvas.transform, false);
        owner.layer = owner.transform.parent.gameObject.layer;
        var view = owner.AddComponent<TradeWindowUI>();
        view.inventory = source;
        view.visuals = source.ItemVisuals;
        view.Build();
        Instance = view; // Publish only after both windows are initialized.
    }
    private void OnDestroy()
    {
        ClientInventory.Changed -= InventoryChanged;
        if (Instance == this) Instance = null;
    }
    private void Build()
    {
        var root = (RectTransform)transform;
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        panel = Box("TradeWindow", root, 0, 0, 760, 580, new Color(.055f,.065f,.085f,.98f));
        panel.anchorMin = panel.anchorMax = new Vector2(.5f,.5f);
        panel.pivot = new Vector2(.5f,.5f); panel.anchoredPosition = Vector2.zero;
        var bar = Box("TitleBar", panel, 0, 0, 760, 40, new Color(.18f,.16f,.11f));
        bar.gameObject.AddComponent<DraggableWindow>().SetWindow(panel);
        title = Label(bar, "Trade", 16, 3, 650, 34, 22);
        status = Label(panel, "Select a nearby player and use Trade in Actions.", 18, 46, 724, 54, 16);
        confirmations = Label(panel, "", 18, 100, 724, 28, 16);
        Label(panel, "MY OFFER", 18, 132, 345, 25, 16);
        Label(panel, "THEIR OFFER", 392, 132, 345, 25, 16);
        mine = Scroll(panel, "MyOffer", 18, 162, 350, 184);
        theirs = Scroll(panel, "TheirOffer", 392, 162, 350, 184);
        Label(panel, "Select an inventory item below, or select it in your inventory.", 18, 353, 724, 24, 14);
        picker = Scroll(panel, "InventoryItems", 18, 382, 724, 94);
        quantity = Input(panel, 18, 488, 90, 34);
        quantity.text = "1";
        add = Button(panel, "Add / set quantity", 118, 488, 188, () => AddSelected());
        confirm = Button(panel, "Confirm", 440, 488, 140, () => Send(98, tradeId + "|" + revision));
        cancel = Button(panel, "Cancel trade", 590, 488, 152, () => Send(100, tradeId.ToString()));
        close = Button(panel, "Close", 590, 535, 152, () => panel.gameObject.SetActive(false));
        Label(panel, "Click your offered item to edit it; × removes it. Changes reset both confirmations.", 18, 535, 550, 34, 12);
        BuildInvitation(root);
        ClientInventory.Changed += InventoryChanged;
        panel.gameObject.SetActive(false);
    }
    private void BuildInvitation(RectTransform root)
    {
        // Match the guild invitation's compact dark/gold presentation and stacking.
        invitation = Box("TradeInvitation", root, 0, 0, 400, 220, new Color(.055f,.064f,.081f,.995f));
        invitation.anchorMin = invitation.anchorMax = invitation.pivot = new Vector2(.5f,.5f);
        invitation.anchoredPosition = Vector2.zero;
        invitation.gameObject.SetActive(false);
        invitation.gameObject.AddComponent<FantasyPopupPresentation>();
        Label(invitation, "TRADE INVITATION", 20, 16, 360, 30, 20);
        Box("Divider", invitation, 20, 52, 360, 1, new Color(.55f,.44f,.25f));
        invitationText = Label(invitation, "", 20, 66, 360, 72, 17);
        invitationText.alignment = TextAlignmentOptions.Center;
        var timer = Box("InviteTimer", invitation, 20, 146, 360, 6, new Color(.15f,.16f,.18f));
        var fill = Box("Fill", timer, 0, 0, 360, 6, new Color(.65f,.51f,.28f));
        fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one;
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        invitationTimer = timer.gameObject.AddComponent<Slider>();
        invitationTimer.fillRect = fill; invitationTimer.interactable = false;
        invitationTimer.transition = Selectable.Transition.None;
        accept = Button(invitation, "Accept", 20, 168, 170, () => RespondToInvitation(92));
        reject = Button(invitation, "Reject", 210, 168, 170, () => RespondToInvitation(93));
        invitationCancel = Button(invitation, "Cancel", 115, 168, 170, () => RespondToInvitation(100));
        invitationClose = Button(invitation, "Close", 115, 168, 170, () => invitation.gameObject.SetActive(false));
    }
    private void ShowInvitation(string message)
    {
        panel.gameObject.SetActive(false);
        invitationText.text = message;
        invitation.gameObject.SetActive(true);
        invitation.GetComponent<FantasyPopupPresentation>().Present();
        InventoryUI.InterruptTooltips();
    }
    private void RespondToInvitation(ushort type)
    {
        if (pending || active || tradeId == Guid.Empty) return;
        Send(type, tradeId.ToString());
        invitation.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (tradeId != Guid.Empty && (!GameServerConnection.Instance || !GameServerConnection.Instance.IsConnected))
            Disconnected();
        if (tradeId != Guid.Empty && !active)
        {
            invitationTimer.value = Mathf.Clamp01((invitationExpiresAt - Time.unscaledTime) / 30f);
            if (Time.unscaledTime >= invitationExpiresAt)
            {
                ExpireLocalInvitation();
            }
        }
        if (!panel || !panel.gameObject.activeSelf) return;
        var parent = panel.parent as RectTransform;
        float scale = Mathf.Min(1f, parent.rect.width / 780f, parent.rect.height / 600f);
        panel.localScale = Vector3.one * Mathf.Max(.1f, scale);
    }
    private void ExpireLocalInvitation()
    {
        // Clear local pending state; only packet 94 may authorize an active trade.
        // Keep the expired ID to ignore late invites, but allow a server acceptance
        // that won the deadline race to arrive before its terminal packet.
        expiredInvitation = tradeId;
        tradeId = Guid.Empty; pending = false;
        invitation.gameObject.SetActive(false);
        Refresh();
    }
    public void RequestTarget()
    {
        if (tradeId != Guid.Empty || pending) return;
        var target = TargetManager.Instance ? TargetManager.Instance.CurrentTarget as RemotePlayer : null;
        if (!target || !GameServerConnection.Instance || !GameServerConnection.Instance.IsConnected)
        { ShowInvitation("Select another connected player first."); Refresh(); return; }
        pending = true;
        ShowInvitation("Sending trade invitation…");
        GameServerConnection.Instance.SendTradeInvitation(target.ObjectId);
        Refresh();
    }
    public static void Handle(ClientPacket packet)
    {
        EnsureCreated();
        if (Instance) Instance.Receive(packet.Type, Encoding.UTF8.GetString(packet.Data));
    }
    private void Receive(ushort type, string payload)
    {
        string[] p = payload.Split('|');
        if (p.Length == 0 || !Guid.TryParse(p[0], out var id)) return;
        if (type == 91 || type == 94)
        {
            if (p.Length != 4 || (p[3] != "0" && p[3] != "1") || finishedTrades.Contains(id)) return;
            if (type == 91 && (id == expiredInvitation || id == tradeId)) return;
            if (tradeId != Guid.Empty && tradeId != id) return;
            if (active || id == Guid.Empty) return;
            tradeId = id; pending = recovery = myConfirmed = theirConfirmed = false;
            active = type == 94; revision = 0; selectedId = 0;
            myItems.Clear(); theirItems.Clear();
            initiator = type == 94 ? p[3] == "1" : p[3] == "0";
            title.text = "Trade — " + p[2];
            status.text = active ? "Choose items and review both offers before confirming."
                : initiator ? "Invitation sent. Waiting for " + p[2] + " (30 seconds)."
                : p[2] + " would like to trade with you (30 seconds).";
            if (active)
            {
                invitation.gameObject.SetActive(false);
                Show(); RebuildOffers(); RebuildPicker();
            }
            else
            {
                invitationExpiresAt = Time.unscaledTime + 30f;
                invitationTimer.value = 1f;
                ShowInvitation(initiator ? "Waiting for " + p[2] + " to respond."
                    : p[2] + " wants to trade with you.");
            }
            Refresh(); return;
        }
        if (type == 103)
        {
            if (id != Guid.Empty && id != tradeId) return;
            pending = false;
            recovery = recovery || (p.Length > 2 && p[1] == "RECOVERY");
            status.text = string.Join("|", p.Skip(p.Length > 2 && p[1] == "RECOVERY" ? 2 : 1));
            if (active) Show();
            else ShowInvitation(status.text);
            Refresh(); return;
        }
        if ((type == 101 || type == 102) && id != Guid.Empty)
        {
            finishedTrades.Add(id);
            if (id == expiredInvitation) expiredInvitation = Guid.Empty;
        }
        if (id != tradeId || id == Guid.Empty) return;
        if (type == 101 || type == 102)
        {
            bool wasActive = active;
            invitation.gameObject.SetActive(false);
            tradeId = Guid.Empty; active = pending = recovery = false;
            myConfirmed = theirConfirmed = false;
            myItems.Clear(); theirItems.Clear(); RebuildOffers();
            status.text = type == 102 ? "Trade completed. Your inventory has been updated." : "Trade cancelled. No items were exchanged.";
            if (wasActive) Show();
            else panel.gameObject.SetActive(false);
            Refresh(); return;
        }
        if (p.Length != 4 || !int.TryParse(p[1], out int incomingRevision) || incomingRevision < revision) return;
        if (type == 97)
        {
            if (!TryParseOffer(p[2], out var first) || !TryParseOffer(p[3], out var second)) return;
            bool changed = incomingRevision != revision;
            revision = incomingRevision;
            myItems.Clear(); myItems.AddRange(initiator ? first : second);
            theirItems.Clear(); theirItems.AddRange(initiator ? second : first);
            if (changed) myConfirmed = theirConfirmed = false;
            pending = false;
            status.text = changed ? "Offer updated. Both players must confirm this revision." : "Offer synchronized.";
            RebuildOffers();
        }
        else if (type == 99)
        {
            // Never confirm an offer revision whose item contents have not arrived.
            if (incomingRevision != revision || (p[2] != "0" && p[2] != "1") || (p[3] != "0" && p[3] != "1")) return;
            myConfirmed = (initiator ? p[2] : p[3]) == "1";
            theirConfirmed = (initiator ? p[3] : p[2]) == "1";
            pending = false;
        }
        Refresh();
    }
    private static bool TryParseOffer(string text, out List<OfferItem> items)
    {
        items = new List<OfferItem>();
        if (text.Length == 0) return true;
        var ids = new HashSet<int>();
        foreach (string row in text.Split(';'))
        {
            var p = row.Split(',');
            if (items.Count >= 32 || p.Length != 4 || !int.TryParse(p[0], out int id) || id <= 0 || !ids.Add(id) ||
                !int.TryParse(p[1], out int count) || count <= 0 || !int.TryParse(p[2], out int template) || template <= 0 ||
                !int.TryParse(p[3], out int enchant) || enchant < 0) return false;
            items.Add(new OfferItem { Id = id, Quantity = count, Template = template, Enchant = enchant });
        }
        return true;
    }
    private void Show() { panel.gameObject.SetActive(true); transform.SetAsLastSibling(); InventoryUI.InterruptTooltips(); }
    private void Refresh()
    {
        bool editable = active && !pending && !recovery;
        add.interactable = editable; confirm.interactable = editable && !myConfirmed;
        cancel.interactable = tradeId != Guid.Empty && !pending && !recovery;
        accept.gameObject.SetActive(tradeId != Guid.Empty && !active && !initiator);
        reject.gameObject.SetActive(accept.gameObject.activeSelf);
        accept.interactable = reject.interactable = !pending;
        invitationCancel.gameObject.SetActive(tradeId != Guid.Empty && !active && initiator);
        invitationCancel.interactable = !pending;
        invitationClose.gameObject.SetActive(tradeId == Guid.Empty && !pending);
        invitationTimer.gameObject.SetActive(tradeId != Guid.Empty && !active);
        close.gameObject.SetActive(tradeId == Guid.Empty && !pending);
        confirmations.text = active ? $"You: {(myConfirmed ? "CONFIRMED" : "Reviewing")}     •     Other player: {(theirConfirmed ? "CONFIRMED" : "Reviewing")}     •     Revision {revision}" : "";
    }
    private void Send(ushort type, string text)
    {
        if (pending || recovery || tradeId == Guid.Empty) return;
        pending = true; Refresh();
        GameServerConnection.Instance.SendTradePacket(type, text);
    }
    private void AddSelected()
    {
        int id = inventory.SelectedObjectId ?? selectedId;
        if (!ClientInventory.TryGetItem(id, out var item) || item.IsEquipped || item.IsQuestItem ||
            !int.TryParse(quantity.text, out int count) || count < 1 || count > item.Quantity)
        { status.text = "Select an unequipped item and enter a quantity you own."; return; }
        Send(95, tradeId + "|" + id + "|" + count);
    }
    private void InventoryChanged() { if (panel && panel.gameObject.activeSelf) RebuildPicker(); }
    private void RebuildPicker()
    {
        Clear(picker); int row = 0;
        foreach (var item in ClientInventory.Items.Where(i => !i.IsEquipped && !i.IsQuestItem && i.Quantity > 0).OrderBy(i => i.ObjectId))
        {
            int id = item.ObjectId;
            ItemRow(picker, row++, item.ItemId, item.Quantity, 0, 714,
                () => { inventory.ClearSelection(); selectedId = id; quantity.text = "1"; status.text = "Selected " + ItemName(item.ItemId) + ". Enter a quantity, then Add / set quantity."; });
        }
        picker.sizeDelta = new Vector2(0, row * 36);
    }
    private void RebuildOffers()
    {
        Clear(mine); Clear(theirs);
        for (int i = 0; i < myItems.Count; i++)
        {
            var item = myItems[i];
            ItemRow(mine, i, item.Template, item.Quantity, item.Enchant, 296,
                () => { inventory.ClearSelection(); selectedId = item.Id; quantity.text = item.Quantity.ToString(); });
            Button(mine, "×", 299, i * 36, 32, () => { if (active) Send(96, tradeId + "|" + item.Id); });
        }
        for (int i = 0; i < theirItems.Count; i++)
        {
            var item = theirItems[i];
            ItemRow(theirs, i, item.Template, item.Quantity, item.Enchant, 332, null);
        }
        mine.sizeDelta = new Vector2(0, myItems.Count * 36);
        theirs.sizeDelta = new Vector2(0, theirItems.Count * 36);
    }
    public void Disconnected()
    {
        if (!this || !invitation || !panel) return; // A late connection callback may outlive the scene.
        bool hadTrade = tradeId != Guid.Empty;
        invitation.gameObject.SetActive(false);
        if (!active) panel.gameObject.SetActive(false);
        tradeId = Guid.Empty; active = pending = recovery = false;
        myItems.Clear(); theirItems.Clear(); RebuildOffers();
        status.text = hadTrade ? "Disconnected. Reconnect to verify your inventory; a confirmed trade may have completed." : "Not connected.";
        Refresh();
    }
    private string ItemName(int template) => visuals && visuals.TryGet(template, out var definition) ? definition.ItemName : "Item " + template;
    private void ItemRow(RectTransform parent, int row, int template, int count, int enchant, int width, Action action)
    {
        var button = Button(parent, "", 0, row * 36, width, action);
        var icon = Box("Icon", (RectTransform)button.transform, 3, 2, 28, 28, Color.white).GetComponent<Image>();
        if (visuals && visuals.TryGet(template, out var definition)) icon.sprite = definition.Icon;
        icon.preserveAspect = true; icon.raycastTarget = false;
        var label = Label((RectTransform)button.transform, ItemName(template) + (enchant > 0 ? " +" + enchant : "") + "  ×" + count, 38, 0, width - 42, 32, 14);
        label.overflowMode = TextOverflowModes.Ellipsis;
    }
    private static void Clear(RectTransform root) { foreach (Transform child in root) { child.gameObject.SetActive(false); Destroy(child.gameObject); } }
    private static RectTransform Box(string name, RectTransform parent, float x, float y, float w, float h, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.layer = parent.gameObject.layer;
        var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0,1); rect.pivot = new Vector2(0,1);
        rect.anchoredPosition = new Vector2(x,-y); rect.sizeDelta = new Vector2(w,h);
        go.GetComponent<Image>().color = color; return rect;
    }
    private static TMP_Text Label(RectTransform parent, string value, float x, float y, float w, float h, float size)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0,1); rect.pivot = new Vector2(0,1);
        rect.anchoredPosition = new Vector2(x,-y); rect.sizeDelta = new Vector2(w,h);
        var label = go.GetComponent<TextMeshProUGUI>(); label.text = value; label.fontSize = size;
        label.color = new Color(.94f,.89f,.76f); label.raycastTarget = false; label.richText = false;
        return label;
    }
    private static Button Button(RectTransform parent, string text, float x, float y, float width, Action action)
    {
        var rect = Box(text, parent, x,y,width,32,new Color(.22f,.24f,.28f));
        var button = rect.gameObject.AddComponent<Button>();
        if (action != null) button.onClick.AddListener(() => action());
        Label(rect,text,4,3,width-8,26,15).alignment = TextAlignmentOptions.Center;
        return button;
    }
    private static RectTransform Scroll(RectTransform parent, string name, float x, float y, float w, float h)
    {
        var viewport = Box(name,parent,x,y,w,h,new Color(.09f,.1f,.13f));
        viewport.gameObject.AddComponent<RectMask2D>();
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        var content = new GameObject("Content",typeof(RectTransform)).GetComponent<RectTransform>();
        content.gameObject.layer = viewport.gameObject.layer;
        content.SetParent(viewport,false); content.anchorMin = new Vector2(0,1); content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0,1); content.sizeDelta = Vector2.zero;
        scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 28;
        return content;
    }
    private static TMP_InputField Input(RectTransform parent, float x,float y,float w,float h)
    {
        var box = Box("Quantity",parent,x,y,w,h,new Color(.13f,.15f,.18f));
        var field = box.gameObject.AddComponent<TMP_InputField>();
        var text = Label(box,"1",6,3,w-12,h-6,17);
        field.textViewport = box; field.textComponent = text;
        field.contentType = TMP_InputField.ContentType.IntegerNumber; field.characterLimit = 10;
        return field;
    }
}
