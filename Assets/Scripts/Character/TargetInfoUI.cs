using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Screen presentation only. Target and health authority remain in existing systems.</summary>
public sealed class TargetInfoUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text guildText;
    [SerializeField] private GameObject healthRow;
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Button closeButton;
    [Header("Layout")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0, -44);
    [SerializeField, Min(180)] private float width = 292;
    [SerializeField, Min(30)] private float nameOnlyHeight = 40;
    [SerializeField, Min(45)] private float detailedHeight = 67;
    [Header("Health animation")]
    [SerializeField, Range(0, 1)] private float healthTransitionSeconds = .24f;

    private TargetManager manager;
    private Monster monster;
    private RemotePlayer player;
    private LocalPlayer localPlayer;
    private Coroutine transition;

    private void OnEnable() { if (manager) Bind(); }
    private void Start()
    {
        manager = TargetManager.Instance;
        closeButton.onClick.AddListener(CancelTarget);
        Bind();
    }
    private void Bind()
    {
        if (!manager) { panel.gameObject.SetActive(false); return; }
        manager.TargetChanged -= ShowTarget;
        manager.TargetChanged += ShowTarget;
        ShowTarget(manager.CurrentTarget);
    }
    private void OnDisable()
    {
        if (manager) manager.TargetChanged -= ShowTarget;
        Unsubscribe();
        if (panel) panel.gameObject.SetActive(false);
    }
    private void OnDestroy() { if (closeButton) closeButton.onClick.RemoveListener(CancelTarget); }
    public void CancelTarget() { if (manager) manager.ClearTarget(); }

    private void Unsubscribe()
    {
        if (monster) monster.ServerHealthChanged -= OnHealth;
        if (player) player.GuildChanged -= OnGuild;
        if (localPlayer) localPlayer.GuildChanged -= OnGuild;
        monster = null; player = null; localPlayer = null;
        if (transition != null) StopCoroutine(transition);
        transition = null;
    }
    private void ShowTarget(ITargetable target)
    {
        Unsubscribe();
        nameText.text = string.Empty;
        guildText.text = string.Empty;
        healthText.text = string.Empty;
        healthFill.fillAmount = 0;
        healthRow.SetActive(false);
        guildText.gameObject.SetActive(false);
        bool valid = target != null && (!(target is Component c) || (c && c.gameObject.activeInHierarchy));
        if (target is Monster dead && dead.IsDead) valid = false;
        panel.gameObject.SetActive(valid);
        if (!valid) return;
        nameText.text = target.Name;
        monster = target as Monster;
        player = target as RemotePlayer;
        localPlayer = target as LocalPlayer;
        if (monster)
        {
            healthRow.SetActive(true);
            monster.ServerHealthChanged += OnHealth;
            if (monster.HasServerHealth) OnHealth(monster.CurrentHp, monster.MaxHp, true);
            else healthText.text = "— / —";
        }
        if (player) { player.GuildChanged += OnGuild; OnGuild(player.GuildName); }
        if (localPlayer) { localPlayer.GuildChanged += OnGuild; OnGuild(localPlayer.GuildName); }
        Layout();
    }
    private void OnGuild(string guild)
    {
        guildText.text = guild ?? string.Empty;
        guildText.gameObject.SetActive(!string.IsNullOrWhiteSpace(guild));
        Layout();
    }
    private void Layout()
    {
        panel.anchoredPosition = screenOffset;
        panel.sizeDelta = new Vector2(width, healthRow.activeSelf || guildText.gameObject.activeSelf ? detailedHeight : nameOnlyHeight);
    }
    private void OnHealth(int current, int max, bool snap)
    {
        healthText.SetText("{0} / {1}", current, max);
        float amount = max > 0 ? Mathf.Clamp01((float)current / max) : 0;
        if (transition != null) StopCoroutine(transition);
        transition = null;
        if (snap || healthTransitionSeconds <= 0 || !isActiveAndEnabled) healthFill.fillAmount = amount;
        else transition = StartCoroutine(AnimateHealth(amount));
    }
    private IEnumerator AnimateHealth(float amount)
    {
        float from = healthFill.fillAmount;
        for (float elapsed = 0; elapsed < healthTransitionSeconds; elapsed += Time.unscaledDeltaTime)
        {
            healthFill.fillAmount = Mathf.Lerp(from, amount, Mathf.SmoothStep(0, 1, elapsed / healthTransitionSeconds));
            yield return null;
        }
        healthFill.fillAmount = amount;
        transition = null;
    }
    private void OnValidate() { if (panel && healthRow && guildText) Layout(); }
}
