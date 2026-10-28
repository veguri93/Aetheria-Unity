using System.Collections.Generic;
using UnityEngine;

/// <summary>Allocation-free presentation pass over registered nameplates only.</summary>
[DefaultExecutionOrder(1000)]
public sealed class WorldNameplateManager : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    private static readonly List<WorldNameplate> plates = new();
    private float nextCameraLookup;
    private static readonly int ArrowTimeId = Shader.PropertyToID("_AetheriaTargetArrowTime");
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState() => plates.Clear();
    public static void Register(WorldNameplate plate) { if (!plates.Contains(plate)) plates.Add(plate); }
    public static void Unregister(WorldNameplate plate) => plates.Remove(plate);

    public static float DistanceAlpha(float distanceSquared, bool targeted)
    {
        if (targeted || distanceSquared <= 30f * 30f) return 1f;
        if (distanceSquared >= 40f * 40f) return 0f;
        return 1f - Mathf.SmoothStep(0f, 1f, (Mathf.Sqrt(distanceSquared) - 30f) / 10f);
    }

    private void LateUpdate()
    {
        Shader.SetGlobalFloat(ArrowTimeId, Time.unscaledTime);
        if ((!viewCamera || !viewCamera.isActiveAndEnabled) && Time.unscaledTime >= nextCameraLookup)
        {
            viewCamera = Camera.main;
            nextCameraLookup = Time.unscaledTime + 1f;
        }
        var player = LocalPlayer.Instance;
        if (!viewCamera || !viewCamera.isActiveAndEnabled || !player)
        {
            // There is no valid player-distance origin during login/disconnect.
            foreach (var plate in plates) if (plate && plate.Visible) plate.SetVisible(false, 0f);
            return;
        }
        Vector3 origin = player.transform.position;
        var selected = TargetManager.Instance ? TargetManager.Instance.CurrentTarget as Component : null;
        for (int i = plates.Count - 1; i >= 0; i--)
        {
            var plate = plates[i];
            if (!plate) { plates.RemoveAt(i); continue; }
            bool targeted = selected && selected.transform == plate.transform;
            float alpha = DistanceAlpha((plate.transform.position - origin).sqrMagnitude, targeted);
            bool show = plate.isActiveAndEnabled && plate.Style && alpha > 0f;
            if (show)
            {
                var screen = viewCamera.WorldToViewportPoint(plate.AnchorPosition);
                show = screen.z > viewCamera.nearClipPlane && screen.x >= -0.1f && screen.x <= 1.1f &&
                    screen.y >= -0.1f && screen.y <= 1.1f;
            }
            plate.SetVisible(show, show ? alpha : 0f);
            if (plate.Visible) plate.UpdatePose(viewCamera);
        }
    }
    private void OnDisable() { foreach (var plate in plates) if (plate) plate.SetVisible(false, 0f); }
}
