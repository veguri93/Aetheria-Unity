using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcHtmlWindow : MonoBehaviour
{
    static NpcHtmlWindow _instance;
    public static bool DiagnosticsEnabled = false; // Enable temporarily when tracing an interaction.
    public static void Trace(string message) { if (DiagnosticsEnabled) Debug.Log("[NPC dialogue] " + message); }
    public static NpcHtmlWindow Instance
    {
        get
        {
            if (!_instance) _instance = FindAnyObjectByType<NpcHtmlWindow>(FindObjectsInactive.Include);
            if (_instance) _instance.Initialize();
            return _instance;
        }
    }
    bool _initialized;
    bool Initialize()
    {
        if (_initialized) return true;
        if (!_contentText || !_buttonContainer || !_buttonTemplate)
        {
            Debug.LogError("[NPC dialogue] Missing required body, action container or button template in scene " + gameObject.scene.name, this);
            return false;
        }
        if (!_buttonTemplate.GetComponentInChildren<TextMeshProUGUI>(true))
        {
            Debug.LogError("[NPC dialogue] Action template has no TMP label.", this);
            return false;
        }
        _initialized = true;
        _buttonTemplate.gameObject.SetActive(false);
        if (_close) _close.onClick.AddListener(Hide);
        Trace("Initialized scene=" + gameObject.scene.name + " layout=" + (_close ? "extended" : "legacy"));
        return true;
    }
    void Status(string message) { if (_status) _status.text = message; else if (message.Length > 0) Debug.LogWarning("[NPC dialogue] " + message); }
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private Transform _buttonContainer;
    [SerializeField] private Button _buttonTemplate;
    [SerializeField] private TextMeshProUGUI _heading, _title, _status;
    [SerializeField] private ScrollRect _scroll;
    [SerializeField] private Button _close;
    [SerializeField] private bool _responsiveLayout;
    readonly List<Button> _buttons = new List<Button>();
    int _npcObjectId, _npcTemplateId;
    string _requestId = "", _token = "";
    Npc _npc;
    float _sentAt;
    bool _waiting;
    public int NpcObjectId => _npcObjectId;
    public int NpcTemplateId => _npcTemplateId;

    void Awake()
    {
        _instance = this;
        // Lazy initialization also supports a scene with an initially inactive window.
        // Do not hide on its first activation if Begin already initialized a conversation.
        bool firstInitialization = !_initialized;
        Initialize();
        if (firstInitialization) gameObject.SetActive(false);
    }
    public string Begin(int id)
    {
        if (!Initialize()) return null;
        Hide();
        _npcObjectId = id;
        _requestId = Guid.NewGuid().ToString("N");
        Trace("Begin npc=" + id);
        return _requestId;
    }
    public void Receive(string[] p, string html)
    {
        if (!Initialize()) return;
        if (p.Length != 9 || p[4] != _requestId || p[0] != _npcObjectId.ToString()) { Trace("Ignored stale/mismatched response"); return; }
        if (p[8] != "page" && p[3] != _token) return;
        if (p[8] == "close") { Hide(); return; }
        if (p[8] == "guild")
        {
            int npcId = _npcObjectId;
            Hide();
            GuildCreateWindow.Instance?.Show(npcId);
            return;
        }
        if (p[8] == "error") { Status(html); _waiting = false; return; }
        _token = p[3];
        _npcTemplateId = int.Parse(p[1]);
        _npc = FindObjectsByType<Npc>().FirstOrDefault(n => n.ObjectId == _npcObjectId);
        if (!ValidNpc()) { Trace("Response rejected: NPC missing or outside interaction range"); Hide(); return; }
        if (_heading) _heading.text = Decode(p[5]);
        if (_title) _title.text = Decode(p[6]);
        Status("");
        _waiting = false;
        ClearButtons();
        if (_responsiveLayout)
        {
            var panel = (RectTransform)transform;
            var parent = transform.parent as RectTransform;
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parent ? Mathf.Min(520, parent.rect.width - 32) : 520);
        }
        var allowed = new HashSet<string>(Decode(p[7]).Split('\n'));
        _contentText.text = NpcDialogueMarkup.Text(html);
        var options = NpcDialogueMarkup.Buttons(html);
        // Older scenes have no permanent close button. Reuse their styled action template.
        if (!_close && !options.Any(o => o.Action == "close" || o.Action == "close 0"))
        { options.Add(new NpcDialogueMarkup.Option { Label = "Close", Action = "close" }); allowed.Add("close"); }
        foreach (var option in options)
        {
            var button = Instantiate(_buttonTemplate, _buttonContainer);
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            label.richText = false;
            label.text = option.Label;
            button.interactable = allowed.Contains(option.Action);
            if (!button.interactable) label.text += " (unavailable)";
            var layout = button.GetComponent<LayoutElement>();
            if (layout) layout.preferredHeight = Mathf.Max(48,
                label.GetPreferredValues(label.text, _responsiveLayout ? Mathf.Max(120, ((RectTransform)transform).rect.width - 84) : 390, 0).y + 20);
            string action = option.Action, token = _token;
            button.onClick.AddListener(() => Click(action, token));
            _buttons.Add(button);
        }
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        Canvas.ForceUpdateCanvases();
        if (_scroll && _scroll.content)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scroll.content);
            if (_responsiveLayout)
            {
                var panel = (RectTransform)transform;
                var parent = transform.parent as RectTransform;
                float maxHeight = parent ? Mathf.Min(620, parent.rect.height - 32) : 620;
                float height = Mathf.Clamp(LayoutUtility.GetPreferredHeight(_scroll.content) + 146, Mathf.Min(260, maxHeight), maxHeight);
                panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_scroll.content);
            }
            _scroll.verticalNormalizedPosition = 1;
        }
        Trace("Opened npc=" + _npcObjectId + " template=" + _npcTemplateId + " buttons=" + _buttons.Count);
    }
    static string Decode(string value) => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
    void Click(string action, string token)
    {
        if (_waiting || token != _token || !ValidNpc()) return;
        if (action == "close" || action == "close 0") { Hide(); return; }
        _waiting = true;
        _sentAt = Time.unscaledTime;
        Status("Waiting for the server…");
        Trace("Action npc=" + _npcObjectId + " action=" + action);
        foreach (var button in _buttons) button.interactable = false;
        GameServerConnection.Instance?.SendNpcDialogueAction(_npcObjectId, token, action);
    }
    bool ValidNpc() => _npc && LocalPlayer.Instance &&
        Vector3.SqrMagnitude(LocalPlayer.Instance.Position - _npc.transform.position) <= 9.25f;
    void Update()
    {
        if (!ValidNpc() || !GameServerConnection.Instance || !GameServerConnection.Instance.IsConnected) { Hide(); return; }
        if (_waiting && Time.unscaledTime - _sentAt > 10f)
        {
            _waiting = false;
            Status("No response. Close this window and speak to the NPC again.");
        }
    }
    public void Hide()
    {
        if (_token.Length > 0) GameServerConnection.Instance?.SendNpcDialogueAction(_npcObjectId, _token, "close");
        _token = _requestId = "";
        _npcObjectId = _npcTemplateId = 0;
        _npc = null;
        _waiting = false;
        ClearButtons();
        gameObject.SetActive(false);
    }
    void ClearButtons()
    {
        foreach (var button in _buttons)
        {
            if (!button) continue;
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
            Destroy(button.gameObject);
        }
        _buttons.Clear();
    }
    void OnDestroy() { if (_instance == this) _instance = null; }
    void OnDisable() { if (_token.Length > 0) Hide(); }
}
