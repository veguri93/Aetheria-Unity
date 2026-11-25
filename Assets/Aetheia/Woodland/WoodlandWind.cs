using UnityEngine;

namespace Aetheia.EnvironmentArt
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class WoodlandWind : MonoBehaviour
    {
        [Range(0, 2)] public float strength = .8f;
        [Range(0, 3)] public float speed = 1;
        public Vector2 direction = new Vector2(.84f, .54f);
        [Range(0, 2)] public float gustIntensity = .55f;
        [Range(0, 2)] public float branchMovement = 1;
        [Range(0, 2)] public float leafMovement = 1;
        void OnEnable() => Apply();
        void OnValidate() { if (isActiveAndEnabled) Apply(); }
        void OnDisable() => Shader.SetGlobalVector("_AetheiaWind", Vector4.zero);
        public void Apply()
        {
            var d = direction.normalized;
            Shader.SetGlobalVector("_AetheiaWind", new Vector4(d.x, d.y, strength, speed));
            Shader.SetGlobalVector("_AetheiaGust", new Vector4(gustIntensity, branchMovement, leafMovement, 0));
        }
    }
}
