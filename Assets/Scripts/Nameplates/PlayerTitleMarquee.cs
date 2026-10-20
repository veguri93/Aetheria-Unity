using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>Two repeating world-text strips clipped to a stationary, camera-facing viewport.</summary>
[DisallowMultipleComponent]
public sealed class PlayerTitleMarquee : MonoBehaviour
{
    [Header("Clipped world text")]
    [SerializeField] private TextMeshPro firstStrip;
    [SerializeField] private TextMeshPro secondStrip;
    [SerializeField] private Material clippedFontMaterial;
    [Header("Marquee (world units)")]
    [SerializeField, Min(.1f)] private float viewportWidth = 2.2f;
    [SerializeField, Min(.1f)] private float viewportHeight = .65f;
    [SerializeField, Min(0)] private float scrollSpeed = .18f;
    [SerializeField, Min(.02f)] private float repeatSpacing = .32f;

    private static readonly int OriginId = Shader.PropertyToID("_MarqueeOrigin");
    private static readonly int RightId = Shader.PropertyToID("_MarqueeRight");
    private static readonly int UpId = Shader.PropertyToID("_MarqueeUp");
    private static readonly int SizeId = Shader.PropertyToID("_MarqueeHalfSize");
    private MaterialPropertyBlock properties;
    private string title;
    private TMP_FontAsset font;
    private float fontSize, builtWidth, builtHeight, builtSpacing;
    private Color titleColor;
    private float period;
    private double phase;
    private double lastTime;
    private bool configured, shown;
    private Renderer[] firstRenderers, secondRenderers;
    private readonly List<Material> fallbackMaterials = new List<Material>();

    public float Period => period;
    public float Offset => (float)phase;
    public float ViewportWidth => viewportWidth;
    public float ScrollSpeed => scrollSpeed;
    public bool IsShowing => shown;
    public TextMeshPro FirstStrip => firstStrip;
    public TextMeshPro SecondStrip => secondStrip;

    public void Configure(string value, WorldNameplateStyle style)
    {
        value = (value ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
        if (!firstStrip || !secondStrip || !clippedFontMaterial || !style) return;
        titleColor = style.titleColor;
        bool changed = !configured || title != value || font != style.font || fontSize != style.titleFontSize ||
            builtWidth != viewportWidth || builtHeight != viewportHeight || builtSpacing != repeatSpacing;
        if (!changed) return;
        ReleaseFallbackMaterials();
        title = value; font = style.font; fontSize = style.titleFontSize;
        builtWidth = viewportWidth; builtHeight = viewportHeight; builtSpacing = repeatSpacing;
        configured = true; phase = 0; lastTime = Time.unscaledTimeAsDouble;
        if (string.IsNullOrWhiteSpace(title)) { SetVisible(false, 0); return; }
        Prepare(firstStrip); Prepare(secondStrip);

        // Plain text throughout: custom titles cannot inject TMP tags. Measure trailing spaces
        // from the actual glyph advances so the last-to-first gap matches every internal gap.
        firstStrip.text = title + " "; firstStrip.ForceMeshUpdate(true);
        var info = firstStrip.textInfo;
        float space = info.characterCount > 1 ? info.characterInfo[info.characterCount - 1].xAdvance - info.characterInfo[info.characterCount - 1].origin : .05f;
        int spaces = Mathf.Clamp(Mathf.CeilToInt(repeatSpacing / Mathf.Max(space, .001f)), 1, 512);
        string unit = title + new string(' ', spaces);
        firstStrip.text = unit; firstStrip.ForceMeshUpdate(true);
        float unitWidth = MeasureAdvance(firstStrip);
        int count = Mathf.Max(1, Mathf.CeilToInt(viewportWidth / Mathf.Max(unitWidth, .01f)) + 1);
        var builder = new StringBuilder(unit.Length * count);
        for (int i = 0; i < count; i++) builder.Append(unit);
        string strip = builder.ToString();
        firstStrip.text = strip; secondStrip.text = strip;
        firstStrip.ForceMeshUpdate(true); secondStrip.ForceMeshUpdate(true);
        period = Mathf.Max(.01f, MeasureAdvance(firstStrip));
        firstRenderers = CacheRenderers(firstStrip);
        secondRenderers = CacheRenderers(secondStrip);
        properties = properties ?? new MaterialPropertyBlock();
    }
    private void Prepare(TextMeshPro strip)
    {
        strip.font = font; strip.fontSharedMaterial = clippedFontMaterial;
        strip.fontSize = fontSize; strip.color = titleColor;
        strip.richText = false; strip.parseCtrlCharacters = false; strip.raycastTarget = false;
        strip.textWrappingMode = TextWrappingModes.NoWrap; strip.overflowMode = TextOverflowModes.Overflow;
        strip.enableAutoSizing = false; strip.alignment = TextAlignmentOptions.Left;
        strip.margin = Vector4.zero;
        var rect = strip.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f); rect.pivot = new Vector2(0,.5f);
        rect.sizeDelta = new Vector2(viewportWidth, viewportHeight);
        rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one;
    }
    private static float MeasureAdvance(TextMeshPro text)
    {
        var info = text.textInfo;
        return info.characterCount > 0 ? info.characterInfo[info.characterCount - 1].xAdvance : 0;
    }
    public void SetVisible(bool visible, float alpha)
    {
        shown = visible && configured && !string.IsNullOrWhiteSpace(title);
        if (firstStrip) firstStrip.enabled = shown;
        if (secondStrip) secondStrip.enabled = shown;
        if (shown)
        {
            Color c = titleColor; c.a *= alpha;
            firstStrip.color = c; secondStrip.color = c;
        }
        // Resume from the same phase without charging for invisible time.
        if (!shown) lastTime = Time.unscaledTimeAsDouble;
    }
    public void UpdatePose(Camera camera)
    {
        if (!shown || !camera || period <= 0) return;
        double now = Time.unscaledTimeAsDouble;
        phase = (phase + Math.Max(0, now - lastTime) * scrollSpeed) % period;
        lastTime = now;
        firstStrip.transform.localPosition = new Vector3(-viewportWidth * .5f - (float)phase, 0, 0);
        secondStrip.transform.localPosition = new Vector3(-viewportWidth * .5f - (float)phase + period, 0, 0);
        // World-space clipping keeps both strips behind one fixed invisible window.
        properties.SetVector(OriginId, transform.position);
        properties.SetVector(RightId, camera.transform.right);
        properties.SetVector(UpId, camera.transform.up);
        properties.SetVector(SizeId, new Vector4(viewportWidth * .5f, viewportHeight * .5f, 0, 0));
        Apply(firstRenderers); Apply(secondRenderers);
    }
    private Renderer[] CacheRenderers(TextMeshPro strip)
    {
        // Keep fallback atlas materials private; never modify a shared font material.
        foreach (var sub in strip.GetComponentsInChildren<TMP_SubMesh>(true))
        {
            var material = new Material(sub.sharedMaterial) { shader = clippedFontMaterial.shader };
            fallbackMaterials.Add(material); sub.sharedMaterial = material;
        }
        return strip.GetComponentsInChildren<Renderer>(true);
    }
    private void Apply(Renderer[] renderers)
    { if (renderers != null) foreach (var renderer in renderers) if (renderer) renderer.SetPropertyBlock(properties); }
    private void ReleaseFallbackMaterials()
    { foreach (var material in fallbackMaterials) if (material) Destroy(material); fallbackMaterials.Clear(); }
    private void OnDisable() { SetVisible(false, 0); }
    private void OnDestroy() { ReleaseFallbackMaterials(); }
    private void OnValidate()
    {
        viewportWidth = Mathf.Max(.1f, viewportWidth); viewportHeight = Mathf.Max(.1f, viewportHeight);
        scrollSpeed = Mathf.Max(0, scrollSpeed); repeatSpacing = Mathf.Max(.02f, repeatSpacing);
    }
}
