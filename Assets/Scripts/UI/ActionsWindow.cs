using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation and routing for the existing ActionsPanel only.
[DisallowMultipleComponent]
public sealed class ActionsWindow : MonoBehaviour
{
    [SerializeField] private PartyWindow partyWindow;
    [SerializeField] private PartyLootModeWindow lootModeWindow;
    [SerializeField] private Button inviteButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private TMP_Text tooltipText;
    private ActionsButtonTooltip tooltipOwner;
    private PersistentUIPosition position;
    private RectTransform panel;
    private Vector2 parentSize;
    private float nextRefresh;
    private TMP_Text[] labels;

    private RemotePlayer SelectedPlayer => TargetManager.Instance
        ? TargetManager.Instance.CurrentTarget as RemotePlayer : null;
    private bool Connected => GameServerConnection.Instance && GameServerConnection.Instance.IsConnected;
    private bool CanInvite => Connected && SelectedPlayer && SelectedPlayer.isActiveAndEnabled && SelectedPlayer.ObjectId > 0;
    private bool CanLeave => Connected && partyWindow && partyWindow.IsInParty;

    private void OnEnable()
    {
        panel = (RectTransform)transform;
        position = GetComponent<PersistentUIPosition>();
        if (labels == null) labels = GetComponentsInChildren<TMP_Text>(true);
        parentSize = Vector2.zero;
        HideTooltip(null);
        RefreshAvailability();
        FitToParent();
    }
    private void Update()
    {
        FitToParent();
        // Party state has no change event. Poll only this open window, without scene searches.
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + .15f;
        RefreshAvailability();
    }
    private void FitToParent()
    {
        var parent = panel.parent as RectTransform;
        if (!parent || parent.rect.size == parentSize) return;
        parentSize = parent.rect.size;
        float scale = Mathf.Min(1f, (parentSize.x - 24f) / 800f, (parentSize.y - 24f) / 500f);
        panel.localScale = Vector3.one * Mathf.Max(.1f, scale);
        // Refresh SDF scale data only when canvas dimensions change.
        foreach (var label in labels) label.ForceMeshUpdate();
        position?.ClampToScreen();
    }
    public void RefreshAvailability()
    {
        bool invite = CanInvite, leave = CanLeave;
        bool changed = (inviteButton && inviteButton.interactable != invite) ||
            (leaveButton && leaveButton.interactable != leave);
        if (inviteButton) inviteButton.interactable = invite;
        if (leaveButton) leaveButton.interactable = leave;
        if (changed && tooltipOwner) ShowTooltip(tooltipOwner);
    }
    public void Toggle()
    {
        if (gameObject.activeSelf) Close();
        else { gameObject.SetActive(true); transform.SetAsLastSibling(); }
    }
    public void Close() => gameObject.SetActive(false);
    private void OnDisable() => HideTooltip(null);

    public void InviteParty()
    {
        if (!CanInvite || (position && position.IsDragging)) return;
        int targetId = SelectedPlayer.ObjectId;
        if (partyWindow && partyWindow.IsInParty)
            GameServerConnection.Instance.SendPartyInviteRequest(targetId, partyWindow.LootMode);
        else if (lootModeWindow)
        {
            // The existing dialog's first Awake hides it. Initialize before assigning the target.
            if (!PartyLootModeWindow.Instance) lootModeWindow.gameObject.SetActive(true);
            lootModeWindow.Show(targetId);
        }
    }
    public void LeaveParty()
    {
        if (!CanLeave || (position && position.IsDragging)) return;
        ConfirmationWindow.Instance?.Show("Leave your party?", ConfirmLeaveParty, null);
    }
    private void ConfirmLeaveParty()
    {
        // Recheck after confirmation; membership may have changed while the dialog was open.
        if (CanLeave) GameServerConnection.Instance.SendChatMessage(ChatType.General, "/leave");
    }
    public void ShowTooltip(ActionsButtonTooltip owner)
    {
        if (!tooltipText || !owner || !isActiveAndEnabled) return;
        tooltipOwner = owner;
        string detail;
        if (owner.gameObject == inviteButton.gameObject)
            detail = !Connected ? "Connect to the game to invite a player." : !CanInvite ? "Select another player to invite." : "Invite the selected player to your party.";
        else if (owner.gameObject == leaveButton.gameObject)
            detail = !Connected ? "Connect to the game to leave a party." : !CanLeave ? "You are not in a party." : "Leave your current party. Confirmation required.";
        else detail = "Not available yet.";
        string value = owner.ActionName + "\n<size=12>" + detail + "</size>";
        if (tooltipText.text != value) tooltipText.text = value;
        tooltipText.gameObject.SetActive(true);
    }
    public void HideTooltip(ActionsButtonTooltip owner)
    {
        if (owner && owner != tooltipOwner) return;
        tooltipOwner = null;
        if (tooltipText) tooltipText.gameObject.SetActive(false);
    }
}
