using UnityEngine;
using UnityEngine.UI;

/// <summary>Transparent-center fantasy frame. Only the selected graphic animates;
/// uses the standard UI material, including stencil and scroll-rect masking.</summary>
[AddComponentMenu("UI/Aetheria Inventory Selection Glow")]
[RequireComponent(typeof(CanvasRenderer))]
public sealed class InventorySelectionGlow : MaskableGraphic
{
    [SerializeField, Range(0f, 3f)] private float glowIntensity = 1f;
    [SerializeField, Range(0f, 1f)] private float animationSpeed = 0.22f;
    [SerializeField, Range(0.5f, 4f)] private float borderWidth = 1.2f;
    [SerializeField, Range(1f, 8f)] private float glowWidth = 3.5f;
    [SerializeField] private Color shimmerColor = new Color(1f, 0.94f, 0.69f, 1f);

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = false;
    }

    private void Update() => SetVerticesDirty();

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = GetPixelAdjustedRect();
        if (r.width < 2 || r.height < 2) return;
        float width = Mathf.Min(borderWidth, Mathf.Min(r.width, r.height) * 0.1f);
        float halo = Mathf.Min(glowWidth, Mathf.Min(r.width, r.height) * 0.22f);
        // All geometry stays inside the slot. No quad covers the item center.
        Ring(vh, r, 0f, width, 0.92f, 0.92f);
        Ring(vh, r, width, width + halo, 0.36f, 0f);
        Ring(vh, r, width + halo + 0.4f, width + halo + 0.85f, 0.18f, 0.18f);
    }

    private void Ring(VertexHelper vh, Rect r, float outer, float inner, float a, float b)
    {
        const int segments = 128;
        float phase = Application.isPlaying ? Time.unscaledTime * animationSpeed : 0.12f;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float distance = Mathf.Abs(Mathf.Repeat(t - phase + 0.5f, 1f) - 0.5f);
            float shimmer = Mathf.Exp(-distance * distance / 0.004f);
            float pulse = 0.94f + 0.06f * Mathf.Sin(phase * Mathf.PI * 4f);
            Color tint = Color.Lerp(color, shimmerColor, shimmer);
            float brightness = glowIntensity * pulse * (0.7f + 0.3f * shimmer);
            int index = vh.currentVertCount;
            Color c = tint; c.a = Mathf.Clamp01(color.a * a * brightness);
            vh.AddVert(Edge(r, outer, t), c, Vector2.zero);
            c.a = Mathf.Clamp01(color.a * b * brightness);
            vh.AddVert(Edge(r, inner, t), c, Vector2.zero);
            if (i == 0) continue;
            vh.AddTriangle(index - 2, index, index - 1);
            vh.AddTriangle(index, index + 1, index - 1);
        }
    }

    private static Vector2 Edge(Rect r, float inset, float t)
    {
        float x0 = r.xMin + inset, x1 = r.xMax - inset;
        float y0 = r.yMin + inset, y1 = r.yMax - inset;
        float p = t * 4f;
        if (p <= 1) return new Vector2(Mathf.Lerp(x0, x1, p), y1);
        if (p <= 2) return new Vector2(x1, Mathf.Lerp(y1, y0, p - 1));
        if (p <= 3) return new Vector2(Mathf.Lerp(x1, x0, p - 2), y0);
        return new Vector2(x0, Mathf.Lerp(y0, y1, p - 3));
    }
}
