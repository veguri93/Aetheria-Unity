using UnityEngine;

/// <summary>A single shared quad. The material contains the silhouette, metal and
/// animated energy; no per-arrow Update, textures, particles or material instances.</summary>
[DisallowMultipleComponent, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class FantasyTargetArrow : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private bool pointLeft;
    private MaterialPropertyBlock properties;
    private float opacity = -1f;
    private float height = 0.3f;
    private static readonly int OpacityId = Shader.PropertyToID("_Opacity");
    private static readonly int DirectionId = Shader.PropertyToID("_Direction");
    private static readonly int PhaseId = Shader.PropertyToID("_Phase");
    public float Width => height * 0.9f * 0.74f;
    public MeshRenderer Renderer => meshRenderer;
    public float Height => height;

    private void Awake()
    {
        if (!meshRenderer) meshRenderer = GetComponent<MeshRenderer>();
        properties = new MaterialPropertyBlock();
        ApplyProperties(0f);
    }

    public void SetSize(float size) => height = size;
    public void SetMaterial(Material material)
    {
        if (meshRenderer && meshRenderer.sharedMaterial != material)
            meshRenderer.sharedMaterial = material;
    }
    public void SetVisible(bool visible, float alpha)
    {
        if (!meshRenderer) return;
        if (meshRenderer.enabled != visible) meshRenderer.enabled = visible;
        if (opacity != alpha) ApplyProperties(alpha);
    }
    private void ApplyProperties(float alpha)
    {
        if (!meshRenderer) return;
        if (properties == null) properties = new MaterialPropertyBlock();
        opacity = alpha;
        properties.SetFloat(OpacityId, alpha);
        properties.SetFloat(DirectionId, pointLeft ? -1f : 1f);
        properties.SetFloat(PhaseId, pointLeft ? 0.8f : 0f);
        meshRenderer.SetPropertyBlock(properties);
    }
    public void SetPose(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            height * 0.9f / Mathf.Max(Mathf.Abs(parentScale.x), 0.001f),
            height * 1.05f / Mathf.Max(Mathf.Abs(parentScale.y), 0.001f),
            1f / Mathf.Max(Mathf.Abs(parentScale.z), 0.001f));
    }
    private void OnDisable() { if (meshRenderer) meshRenderer.enabled = false; }
}
