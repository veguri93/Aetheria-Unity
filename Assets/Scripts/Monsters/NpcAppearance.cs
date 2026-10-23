using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Build a reusable, inactive model from native visual components only. Never clone gameplay scripts.
// Animator controllers are registered project assets; their existing animation callbacks remain intact.
public static class NpcAppearance
{
    public static GameObject Build(GameObject source, Transform cacheParent)
    {
        var transforms = new Dictionary<Transform, Transform>();
        var renderers = new Dictionary<Renderer, Renderer>();
        GameObject root = new GameObject(source.name + " appearance");
        root.SetActive(false);
        root.transform.SetParent(cacheParent, false);
        CopyTransform(source.transform, root.transform);
        transforms[source.transform] = root.transform;
        CopyChildren(source.transform, root.transform, transforms);
        foreach (var pair in transforms)
        {
            var src = pair.Key;
            var dst = pair.Value.gameObject;
            dst.layer = src.gameObject.layer;
            foreach (var mesh in src.GetComponents<MeshFilter>())
                dst.AddComponent<MeshFilter>().sharedMesh = mesh.sharedMesh;
            foreach (var mesh in src.GetComponents<MeshRenderer>())
            {
                var clone = dst.AddComponent<MeshRenderer>();
                CopyRenderer(mesh, clone, transforms);
                renderers[mesh] = clone;
            }
            foreach (var skin in src.GetComponents<SkinnedMeshRenderer>())
            {
                var clone = dst.AddComponent<SkinnedMeshRenderer>();
                clone.sharedMesh = skin.sharedMesh;
                clone.bones = skin.bones.Select(b => Remap(b, transforms)).ToArray();
                clone.rootBone = Remap(skin.rootBone, transforms);
                clone.localBounds = skin.localBounds;
                clone.quality = skin.quality;
                clone.updateWhenOffscreen = skin.updateWhenOffscreen;
                if (skin.sharedMesh)
                    for (int i = 0; i < skin.sharedMesh.blendShapeCount; i++)
                        clone.SetBlendShapeWeight(i, skin.GetBlendShapeWeight(i));
                CopyRenderer(skin, clone, transforms);
                renderers[skin] = clone;
            }
            foreach (var animator in src.GetComponents<Animator>())
            {
                var clone = dst.AddComponent<Animator>();
                clone.avatar = animator.avatar;
                clone.runtimeAnimatorController = animator.runtimeAnimatorController;
                clone.applyRootMotion = animator.applyRootMotion;
                clone.updateMode = animator.updateMode;
                clone.cullingMode = animator.cullingMode;
                clone.enabled = animator.enabled;
                clone.speed = animator.speed;
            }
        }
        foreach (var pair in transforms)
        {
            var lod = pair.Key.GetComponent<LODGroup>();
            if (!lod) continue;
            var clone = pair.Value.gameObject.AddComponent<LODGroup>();
            clone.localReferencePoint = lod.localReferencePoint;
            clone.size = lod.size;
            clone.fadeMode = lod.fadeMode;
            clone.animateCrossFading = lod.animateCrossFading;
            clone.SetLODs(lod.GetLODs().Select(l => new LOD(l.screenRelativeTransitionHeight,
                l.renderers.Where(r => r && renderers.ContainsKey(r)).Select(r => renderers[r]).ToArray())
                { fadeTransitionWidth = l.fadeTransitionWidth }).ToArray());
        }
        return root;
    }

    static void CopyChildren(Transform source, Transform target, Dictionary<Transform, Transform> map)
    {
        foreach (Transform child in source)
        {
            // World labels, target arrows and health UI are supplied by behavior presentation instead.
            if (child.GetComponent<Canvas>() || child.GetComponent<TMP_Text>() ||
                child.GetComponent<FantasyTargetArrow>() || child.GetComponent<HealthBar>() ||
                child.GetComponent<SpriteRenderer>()) continue;
            var copy = new GameObject(child.name).transform;
            copy.SetParent(target, false);
            CopyTransform(child, copy);
            copy.gameObject.SetActive(child.gameObject.activeSelf);
            map[child] = copy;
            CopyChildren(child, copy, map);
        }
    }
    static void CopyTransform(Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;
    }
    static Transform Remap(Transform source, Dictionary<Transform, Transform> map) =>
        source && map.TryGetValue(source, out var target) ? target : null;
    static void CopyRenderer(Renderer source, Renderer target, Dictionary<Transform, Transform> map)
    {
        target.sharedMaterials = source.sharedMaterials;
        target.enabled = source.enabled;
        target.shadowCastingMode = source.shadowCastingMode;
        target.receiveShadows = source.receiveShadows;
        target.lightProbeUsage = source.lightProbeUsage;
        target.reflectionProbeUsage = source.reflectionProbeUsage;
        target.probeAnchor = Remap(source.probeAnchor, map);
        target.motionVectorGenerationMode = source.motionVectorGenerationMode;
        target.allowOcclusionWhenDynamic = source.allowOcclusionWhenDynamic;
        target.renderingLayerMask = source.renderingLayerMask;
        target.sortingLayerID = source.sortingLayerID;
        target.sortingOrder = source.sortingOrder;
    }

    // Physics belongs to the spawned entity. Copy shape data, never rigid bodies, agents or scripts.
    public static void AddInteractionShape(GameObject entity, GameObject visual)
    {
        var capsule = visual.GetComponent<CapsuleCollider>();
        if (capsule)
        {
            var copy = entity.AddComponent<CapsuleCollider>();
            copy.center = capsule.center; copy.radius = capsule.radius;
            copy.height = capsule.height; copy.direction = capsule.direction;
            copy.isTrigger = capsule.isTrigger;
            return;
        }
        var box = visual.GetComponent<BoxCollider>();
        if (box)
        {
            var copy = entity.AddComponent<BoxCollider>();
            copy.center = box.center; copy.size = box.size; copy.isTrigger = box.isTrigger;
            return;
        }
        // Pure imported models may not have a collider. Derive one from their local renderer bounds.
        var bounds = new Bounds(); bool found = false;
        foreach (var renderer in entity.GetComponentsInChildren<Renderer>(true))
        {
            var b = renderer.localBounds;
            for (int corner = 0; corner < 8; corner++)
            {
                var offset = Vector3.Scale(b.extents, new Vector3((corner & 1) == 0 ? -1 : 1,
                    (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                var point = entity.transform.InverseTransformPoint(renderer.transform.TransformPoint(b.center + offset));
                if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; }
                else bounds.Encapsulate(point);
            }
        }
        var fallback = entity.AddComponent<BoxCollider>();
        fallback.center = found ? bounds.center : Vector3.up;
        fallback.size = found ? Vector3.Max(bounds.size, Vector3.one * .1f) : new Vector3(1, 2, 1);
    }
}
