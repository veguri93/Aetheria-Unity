using TMPro;
using UnityEngine;

public enum TargetIndicatorState { None, Selected, Hostile }

/// <summary>Shared name-row layout and target presentation for any ITargetable.
/// All elements are world meshes; there is no per-character Canvas or raycast.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(WorldNameplate))]
public sealed class OverheadTargetIndicator : MonoBehaviour
{
    [SerializeField] private WorldNameplate nameplate;
    [SerializeField] private FantasyTargetArrow leftArrow;
    [SerializeField] private FantasyTargetArrow rightArrow;
    [SerializeField] private SpriteRenderer crestRenderer;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material hostileMaterial;
    [SerializeField, Min(0f)] private float arrowPadding = 0.12f;
    [SerializeField, Min(0f)] private float crestGap = 0.08f;
    [SerializeField, Min(0.01f)] private float crestHeight = 0.22f;
    [SerializeField, Range(1f, 2f)] private float arrowSizeMultiplier = 1.3f;
    private Sprite crest;
    private string cachedName;
    private float cachedSize = -1f;
    private Sprite cachedCrest;
    private float cachedCrestHeight = -1f, cachedCrestGap, cachedPadding, cachedArrowMultiplier;
    private float nameOffset, crestOffset, edgeOffset;
    private bool visible;
    private float alpha;
    public TargetIndicatorState State { get; private set; }
    public float NameOffset => nameOffset;
    public bool HasCrest => crest != null;
    public FantasyTargetArrow LeftArrow => leftArrow;
    public FantasyTargetArrow RightArrow => rightArrow;
    public SpriteRenderer CrestRenderer => crestRenderer;

    public static void SetFor(ITargetable target, TargetIndicatorState state)
    {
        if (target is Component component && component &&
            component.TryGetComponent<OverheadTargetIndicator>(out var indicator))
            indicator.SetState(state);
    }

    public static OverheadTargetIndicator AddPresentation(GameObject entity, OverheadTargetIndicator source, WorldNameplate plate)
    {
        var target = entity.AddComponent<OverheadTargetIndicator>();
        target.nameplate = plate;
        if (source.leftArrow) target.leftArrow = Instantiate(source.leftArrow, entity.transform, false);
        if (source.rightArrow) target.rightArrow = Instantiate(source.rightArrow, entity.transform, false);
        if (source.crestRenderer) target.crestRenderer = Instantiate(source.crestRenderer, entity.transform, false);
        target.selectedMaterial = source.selectedMaterial; target.hostileMaterial = source.hostileMaterial;
        target.arrowPadding = source.arrowPadding; target.crestGap = source.crestGap;
        target.crestHeight = source.crestHeight; target.arrowSizeMultiplier = source.arrowSizeMultiplier;
        return target;
    }
    private void Awake()
    {
        if (!nameplate) nameplate = GetComponent<WorldNameplate>();
        if (crestRenderer) { crest = crestRenderer.sprite; crestRenderer.enabled = false; }
        RefreshVisibility();
    }

    public void SetState(TargetIndicatorState state)
    {
        State = state;
        var material = state == TargetIndicatorState.Hostile ? hostileMaterial : selectedMaterial;
        if (leftArrow && material) leftArrow.SetMaterial(material);
        if (rightArrow && material) rightArrow.SetMaterial(material);
        RefreshLayout();
        RefreshVisibility();
    }

    public void SetGuildCrest(Sprite sprite)
    {
        crest = sprite;
        if (crestRenderer) crestRenderer.sprite = sprite;
        cachedSize = -1f;
        RefreshLayout(); RefreshVisibility();
    }

    public void RefreshLayout()
    {
        if (!nameplate || !nameplate.Label || !nameplate.Style) return;
        var text = nameplate.Label;
        float size = nameplate.Style.fontSize;
        if (cachedName == text.text && cachedSize == size && cachedCrest == crest &&
            cachedCrestHeight == crestHeight && cachedCrestGap == crestGap &&
            cachedPadding == arrowPadding && cachedArrowMultiplier == arrowSizeMultiplier) return;
        cachedName = text.text; cachedSize = size; cachedCrest = crest;
        cachedCrestHeight = crestHeight; cachedCrestGap = crestGap;
        cachedPadding = arrowPadding; cachedArrowMultiplier = arrowSizeMultiplier;
        // Rebuild only on content/style changes, never every frame.
        text.fontSize = size;
        text.ForceMeshUpdate(true);
        float textWidth = text.GetPreferredValues(text.text).x;
        float crestWidth = crest ? crestHeight * crest.rect.width / Mathf.Max(1f, crest.rect.height) : 0f;
        float prefix = crest ? crestWidth + crestGap : 0f;
        nameOffset = prefix * 0.5f;
        float rowWidth = textWidth + prefix;
        crestOffset = -rowWidth * 0.5f + crestWidth * 0.5f;
        float arrowWidth = 0f;
        if (leftArrow)
        {
            leftArrow.SetSize(size * arrowSizeMultiplier * 0.1f);
            arrowWidth = leftArrow.Width;
        }
        if (rightArrow) rightArrow.SetSize(size * arrowSizeMultiplier * 0.1f);
        edgeOffset = rowWidth * 0.5f + arrowPadding + arrowWidth * 0.5f;
        if (crestRenderer && crest)
        {
            var bounds = crest.bounds.size;
            crestRenderer.transform.localScale = new Vector3(
                crestWidth / Mathf.Max(0.001f, bounds.x), crestHeight / Mathf.Max(0.001f, bounds.y), 1f);
        }
    }

    public void SetVisibility(bool isVisible, float opacity)
    {
        visible = isVisible; alpha = opacity;
        RefreshLayout(); RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        bool arrows = visible && State != TargetIndicatorState.None;
        Color tint = new Color(1f, 1f, 1f, alpha);
        if (leftArrow) leftArrow.SetVisible(arrows, alpha);
        if (rightArrow) rightArrow.SetVisible(arrows, alpha);
        if (crestRenderer) { crestRenderer.enabled = visible && crest; crestRenderer.color = tint; }
    }

    public void UpdatePose(Vector3 rowCenter, Quaternion rotation, float scale = 1f)
    {
        Vector3 right = rotation * Vector3.right;
        if (State != TargetIndicatorState.None)
        {
            if (leftArrow) leftArrow.SetPose(rowCenter - right * (edgeOffset * scale), rotation, scale);
            if (rightArrow) rightArrow.SetPose(rowCenter + right * (edgeOffset * scale), rotation, scale);
        }
        if (crestRenderer && crest)
        {
            SetPose(crestRenderer.transform, rowCenter + right * (crestOffset * scale), rotation, false);
            Vector3 parentScale = crestRenderer.transform.parent.lossyScale;
            Vector3 bounds = crest.bounds.size;
            float width = crestHeight * crest.rect.width / Mathf.Max(1f, crest.rect.height);
            crestRenderer.transform.localScale = new Vector3(
                width * scale / Mathf.Max(0.001f, bounds.x * Mathf.Abs(parentScale.x)),
                crestHeight * scale / Mathf.Max(0.001f, bounds.y * Mathf.Abs(parentScale.y)), 1f);
        }
    }

    private static void SetPose(Transform target, Vector3 position, Quaternion rotation, bool normalizeScale = true)
    {
        if (!target) return;
        target.SetPositionAndRotation(position, rotation);
        if (!normalizeScale) return;
        Vector3 scale = target.parent ? target.parent.lossyScale : Vector3.one;
        target.localScale = new Vector3(1f / Mathf.Max(Mathf.Abs(scale.x), 0.001f),
            1f / Mathf.Max(Mathf.Abs(scale.y), 0.001f), 1f / Mathf.Max(Mathf.Abs(scale.z), 0.001f));
    }

    private void OnDisable() { State = TargetIndicatorState.None; visible = false; RefreshVisibility(); }
}
