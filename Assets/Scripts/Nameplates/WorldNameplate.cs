
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WorldNameplate : MonoBehaviour
{
    [SerializeField] private OverheadTargetIndicator targetIndicator;
    public OverheadTargetIndicator TargetIndicator => targetIndicator;
    [SerializeField]
    private WorldNameplateStyle style;

    [Header("Text")]
    [SerializeField]
    private TextMeshPro label;

    [SerializeField]
    private TextMeshPro titleLabel;

    [Header("Position")]
    [Tooltip("Optional animated head bone. Otherwise model bounds determine height.")]
    [SerializeField]
    private Transform headAnchor;

    [SerializeField]
    private float extraVerticalOffset;

    private Vector3 fallbackAnchor;
    private bool initialized;
    private float visibilityAlpha = -1f;
    public float ReadabilityScale { get; private set; } = 1f;

    [Header("Player Title Marquee")]
    [SerializeField] private PlayerTitleMarquee titleMarquee;
    private bool titleScrolling;
    private string actualTitle = string.Empty;

    public void SetTitleScrolling(bool enabled)
    {
        // Only PlayerNameplate opts in; NPC and monster callers retain stationary titles.
        titleScrolling = enabled && titleMarquee;
        if (titleMarquee)
        {
            if (titleScrolling) titleMarquee.Configure(actualTitle, style);
            titleMarquee.SetVisible(titleScrolling && Visible, Mathf.Max(0f, visibilityAlpha));
        }
        if (titleScrolling && titleLabel) titleLabel.enabled = false;
    }

    public bool Visible { get; private set; }

    public WorldNameplateStyle Style => style;

    public TextMeshPro Label => label;

    public TextMeshPro TitleLabel => titleLabel;

    public Vector3 AnchorPosition =>
        (headAnchor
            ? headAnchor.position
            : transform.TransformPoint(fallbackAnchor))
        + Vector3.up *
        ((style ? style.verticalOffset : 0f)
        + extraVerticalOffset);

    // Preserve existing callers.
    public void SetName(string actualName)
    {
        SetInfo(actualName, string.Empty);
    }

    public void SetInfo(
        string actualName,
        string actualTitle)
    {
        if (!label || !style)
            return;

        this.actualTitle = actualTitle ?? string.Empty;

        if (!initialized)
        {
            CalculateAnchor();
            initialized = true;
        }

        ConfigureLabel(
            label,
            actualName,
            style.fontSize,
            style.color);

        if (titleLabel && titleLabel != label)
        {
            ConfigureLabel(
                titleLabel,
                actualTitle,
                style.titleFontSize,
                style.titleColor);
        }
        else if (!string.IsNullOrWhiteSpace(actualTitle))
        {
            Debug.LogWarning(
                $"NPC {actualName} has a title, " +
                "but its WorldNameplate has no Title Label.",
                this);
        }

        if (titleScrolling && titleMarquee) titleMarquee.Configure(this.actualTitle, style);
        visibilityAlpha = -1f;
        SetVisible(false, 0f);
        if (targetIndicator) targetIndicator.RefreshLayout();

        if (isActiveAndEnabled)
        {
            WorldNameplateManager.Register(this);
        }
    }

    // Presentation is selected by entity behavior, never inherited from an unrelated appearance.
    public static WorldNameplate AddPresentation(GameObject entity, WorldNameplate source)
    {
        var plate = entity.AddComponent<WorldNameplate>();
        plate.style = source.style;
        plate.extraVerticalOffset = source.extraVerticalOffset;
        if (source.label) plate.label = Instantiate(source.label, entity.transform, false);
        if (source.titleLabel) plate.titleLabel = Instantiate(source.titleLabel, entity.transform, false);
        // A source model's head bone is not valid for a different skeleton; recalculate model bounds.
        if (source.targetIndicator) plate.targetIndicator = OverheadTargetIndicator.AddPresentation(entity, source.targetIndicator, plate);
        return plate;
    }
    private void CalculateAnchor()
    {
        var renderers =
            GetComponentsInChildren<SkinnedMeshRenderer>(true);

        float top = transform.position.y + 2f;

        if (renderers.Length > 0)
        {
            top = float.NegativeInfinity;

            foreach (var renderer in renderers)
            {
                top = Mathf.Max(
                    top,
                    renderer.bounds.max.y);
            }
        }
        else if (TryGetComponent<Collider>(out var collider))
        {
            top = collider.bounds.max.y;
        }

        fallbackAnchor =
            transform.InverseTransformPoint(
                new Vector3(
                    transform.position.x,
                    top,
                    transform.position.z));
    }

    private void ConfigureLabel(
        TextMeshPro text,
        string value,
        float fontSize,
        Color color)
    {
        text.font = style.font;
        text.fontSharedMaterial = style.fontMaterial;

        text.fontSize = fontSize;
        text.color = color;

        text.alignment = TextAlignmentOptions.Center;
        text.richText = false;
        text.raycastTarget = false;

        text.text = value ?? string.Empty;
    }

    private void OnEnable()
    {
        if (initialized)
        {
            WorldNameplateManager.Register(this);
        }
    }

    private void OnDisable()
    {
        WorldNameplateManager.Unregister(this);
        SetVisible(false, 0f);
    }

    public void SetVisible(
        bool visible,
        float alpha)
    {
        visible &=
            initialized &&
            label &&
            style &&
            !string.IsNullOrWhiteSpace(label.text);

        alpha = Mathf.Clamp01(alpha);
        if (Visible == visible && Mathf.Approximately(visibilityAlpha, alpha)) return;
        visibilityAlpha = alpha;
        Visible = visible;
        if (targetIndicator) targetIndicator.SetVisibility(visible, alpha);

        if (label)
        {
            label.enabled = visible;

            if (visible)
            {
                Color nameColor = style.color;
                nameColor.a *= alpha;

                label.color = nameColor;
                label.fontSize = style.fontSize;
            }
        }

        if (titleLabel && titleLabel != label)
        {
            bool showTitle =
                visible &&
                !string.IsNullOrWhiteSpace(titleLabel.text);

            titleLabel.enabled = showTitle && !titleScrolling;
            if (titleMarquee) titleMarquee.SetVisible(showTitle && titleScrolling, alpha);

            if (showTitle)
            {
                Color titleColor = style.titleColor;
                titleColor.a *= alpha;

                titleLabel.color = titleColor;
                titleLabel.fontSize = style.titleFontSize;
            }
        }
    }

    public void UpdatePose(Camera camera)
    {
        if (!camera || !label)
            return;

        Vector3 anchor = AnchorPosition;
        // Keep a 24-pixel name em at 1080p, proportional at other resolutions.
        // Projection depth (not Euclidean distance) avoids growth at screen edges.
        float depth = Mathf.Max(camera.nearClipPlane, Vector3.Dot(anchor - camera.transform.position, camera.transform.forward));
        float viewHeight = camera.orthographic ? 2f * camera.orthographicSize :
            2f * depth * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
        ReadabilityScale = viewHeight * (24f / 1080f) / Mathf.Max(.01f, style.fontSize * .1f);

        UpdateLabelPose(
            label,
            anchor + camera.transform.right * (targetIndicator ? targetIndicator.NameOffset * ReadabilityScale : 0f),
            camera);
        if (targetIndicator) targetIndicator.UpdatePose(anchor, camera.transform.rotation, ReadabilityScale);


        if (titleLabel &&
            titleLabel != label &&
            (titleLabel.enabled || (titleScrolling && titleMarquee && titleMarquee.IsShowing)))
        {
            Vector3 titlePosition =
                anchor +
                camera.transform.up * (style.titleVerticalSeparation * ReadabilityScale);

            UpdateLabelPose(
                titleLabel,
                titlePosition,
                camera);
            if (titleScrolling && titleMarquee) titleMarquee.UpdatePose(camera);
        }

    }

    private void UpdateLabelPose(
        TextMeshPro text,
        Vector3 position,
        Camera camera)
    {
        Transform target = text.transform;

        
        target.SetPositionAndRotation(position, camera.transform.rotation);
        Vector3 scale =
            target.parent
                ? target.parent.lossyScale
                : Vector3.one;

        target.localScale = new Vector3(
            ReadabilityScale / Mathf.Max(Mathf.Abs(scale.x), 0.001f),
            ReadabilityScale / Mathf.Max(Mathf.Abs(scale.y), 0.001f),
            ReadabilityScale / Mathf.Max(Mathf.Abs(scale.z), 0.001f));
    }
}
