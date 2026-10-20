using System.Collections.Generic;
using UnityEngine;

/// <summary>One camera lookup and visibility pass shared by all world nameplates.</summary>
[DefaultExecutionOrder(1000)]
public sealed class WorldNameplateManager : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField, Min(1)] private int maximumVisibleNameplates = 16;
    [SerializeField, Range(0.05f, 1f)] private float visibilityInterval = 0.15f;
    private static readonly List<WorldNameplate> plates = new();
    private readonly List<Candidate> candidates = new();
    private readonly HashSet<WorldNameplate> visible = new();
    private float nextVisibility, nextCameraLookup;
    private struct Candidate { public WorldNameplate plate; public float distance; public bool targeted; }
    private static readonly System.Comparison<Candidate> Nearest = (a, b) =>
        a.targeted != b.targeted ? (a.targeted ? -1 : 1) : a.distance.CompareTo(b.distance);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState() => plates.Clear();
    public static void Register(WorldNameplate plate) { if (!plates.Contains(plate)) plates.Add(plate); }
    public static void Unregister(WorldNameplate plate) => plates.Remove(plate);
    private void LateUpdate()
    {
        // One unscaled GPU clock shared by the two visible arrowheads.
        Shader.SetGlobalFloat(ArrowTimeId, Time.unscaledTime);
        if ((!viewCamera || !viewCamera.isActiveAndEnabled) && Time.unscaledTime >= nextCameraLookup)
        {
            viewCamera = Camera.main; nextCameraLookup = Time.unscaledTime + 1f;
        }
        if (!viewCamera || !viewCamera.isActiveAndEnabled)
        {
            foreach (var p in plates) if (p && p.Visible) p.SetVisible(false, 0f);
            return;
        }
        if (Time.unscaledTime >= nextVisibility)
        {
            nextVisibility = Time.unscaledTime + visibilityInterval;
            UpdateVisibility();
        }
        foreach (var p in plates) if (p && p.Visible) p.UpdatePose(viewCamera);
    }
    private static readonly int ArrowTimeId = Shader.PropertyToID("_AetheriaTargetArrowTime");
    private void UpdateVisibility()
    {
        candidates.Clear();
        visible.Clear();
        Vector3 origin = LocalPlayer.Instance ? LocalPlayer.Instance.transform.position : viewCamera.transform.position;
        for (int i = plates.Count - 1; i >= 0; i--)
        {
            var p = plates[i];
            if (!p) { plates.RemoveAt(i); continue; }
            if (!p.isActiveAndEnabled || !p.Style) continue;
            float distance = (p.transform.position - origin).sqrMagnitude;
            float range = p.Style.visibilityDistance;
            if (distance >= range * range) continue;
            var screen = viewCamera.WorldToViewportPoint(p.AnchorPosition);
            if (screen.z <= 0f || screen.x < -0.05f || screen.x > 1.05f || screen.y < -0.05f || screen.y > 1.05f) continue;
            candidates.Add(new Candidate { plate = p, distance = distance,
                targeted = p.TargetIndicator && p.TargetIndicator.State != TargetIndicatorState.None });
        }
        candidates.Sort(Nearest);
        for (int i = 0; i < Mathf.Min(candidates.Count, maximumVisibleNameplates); i++)
        {
            var c = candidates[i]; float range = c.plate.Style.visibilityDistance;
            float fadeWidth = range * c.plate.Style.fadeDistanceFraction;
            float alpha = fadeWidth > 0f ? Mathf.Clamp01((range - Mathf.Sqrt(c.distance)) / fadeWidth) : 1f;
            c.plate.SetVisible(true, alpha);
            visible.Add(c.plate);
        }
        foreach (var p in plates) if (p && !visible.Contains(p)) p.SetVisible(false, 0f);
    }
    private void OnDisable() { foreach (var p in plates) if (p) p.SetVisible(false, 0f); }
}
