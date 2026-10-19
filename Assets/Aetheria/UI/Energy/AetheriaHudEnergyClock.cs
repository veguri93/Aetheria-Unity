using UnityEngine;
/// <summary>One shared unscaled clock. Does not touch Sliders, stats, sprites or materials.</summary>
[ExecuteAlways, DisallowMultipleComponent]
public sealed class AetheriaHudEnergyClock : MonoBehaviour
{
    [Tooltip("Pause only the decorative energy animation.")]
    public bool animationEnabled = true;
    [Min(0)] public float globalSpeed = 1f;
    private static readonly int TimeId = Shader.PropertyToID("_AetheriaUIEnergyTime");
    private double previous, elapsed;
    private void OnEnable() { previous = Now; }
    private double Now => Application.isPlaying ? Time.unscaledTimeAsDouble : Time.realtimeSinceStartupAsDouble;
    private void Update()
    {
        double now = Now;
        double dt = System.Math.Max(0, now - previous);
        previous = now;
        if (animationEnabled) elapsed += dt * globalSpeed;
        Shader.SetGlobalFloat(TimeId, (float)elapsed);
    }
}

